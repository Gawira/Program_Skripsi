using System.Collections;
using UnityEngine;
using UnityStandardAssets.Cameras;
using UnityStandardAssets.Characters.ThirdPerson;

namespace UnityEngine
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Player Stats")]
        public int playerHealth = 100;
        public int currentHealth;
        public int lifesteal = 8;
        public int defense = 5;

        public int money = 0;
        public int damage = 20;
        public int slotMax = 2;

        [Header("Respawn Settings")]
        private Vector3 respawnPoint;
        private Quaternion respawnRotation;

        private Animator anim;
        private bool canTakeDamage = true;

        public TPCharacter thirdPersonCharacter;

        [Header("UI References")]
        public CanvasGroup youDiedCanvas; // Assign in inspector
        public float fadeDuration = 2f;   // how long the fade takes

        // --- internal guard ---
        public bool isDead = false;

        private void Start()
        {
            anim = GetComponent<Animator>();

            // Prevent spawn conflict
            bool useSpawnManager = SceneSpawnManager.overrideSpawnThisScene;

            if (SaveManager.SaveExistsForActiveSlot() && !useSpawnManager)
            {
                SaveData data = SaveManager.LoadGame();
                transform.position = data.checkpointPosition;
                currentHealth = data.playerHealth;
                money = data.playerMoney;

                playerHealth = data.playerHealth;
                damage = data.damage;
                lifesteal = data.lifesteal;
                defense = data.defense;
                slotMax = data.slotMax;

                StartCoroutine(RestoreInventoriesAfterLoad(data));

                respawnPoint = data.checkpointPosition;
                respawnRotation = transform.rotation;

                Debug.Log($"Player loaded from active save slot (checkpoint).");
            }
            else
            {
                // either new game OR spawn manager override
                currentHealth = playerHealth;
                respawnPoint = transform.position;
                respawnRotation = transform.rotation;
                Debug.Log(useSpawnManager
                    ? "Player spawned via SceneSpawnManager override."
                    : "No save found; starting fresh.");
            }

            if (youDiedCanvas != null)
            {
                youDiedCanvas.alpha = 0f;
                youDiedCanvas.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            // Only trigger Die once when health reaches 0
            if (!isDead && currentHealth <= 0)
            {
                Die();
            }
        }

        // =======================
        // Checkpoint & Respawn
        // =======================
        public void SetCheckpoint(Vector3 position, Quaternion rotation)
        {
            respawnPoint = position;
            respawnRotation = rotation;
            Debug.Log($"Checkpoint set at: {position}");
        }

        private void Die()
        {
            // guard to prevent multiple calls
            if (isDead) return;
            isDead = true;

            Debug.Log("Player has died!");
            if (anim != null)
                anim.SetTrigger("Die");

            // Disable player control and movement
            TPUserControl controller = GetComponent<TPUserControl>();
            if (controller != null) controller.enabled = false;

            TPCharacter character = GetComponent<TPCharacter>();
            if (character != null) character.enabled = false;

            // Disable physical collision temporarily (keep invincible)
            SetInvincible();

            // Turn off lock-on if present
            LockOnTarget lockontarget = GetComponent<LockOnTarget>();
            if (lockontarget != null)
            { 
                lockontarget.LockOn = false;
                lockontarget.UnlockTarget();
                lockontarget.LockOntoNewTarget();
            }

            // Reset money on death
            money = 0;

            // 🔹 Start the full death flow — fade in, respawn, then fade out
            StartCoroutine(HandleDeathSequence());
        }

        private IEnumerator HandleDeathSequence()
        {
            // Step 1: Fade In the death screen
            yield return StartCoroutine(FadeIn());

            // Step 2: Wait a bit while screen is fully visible
            yield return new WaitForSeconds(1f);

            // Step 3: Respawn player
            Respawn();

            // Step 4: Wait a moment before fade-out (optional)
            yield return new WaitForSeconds(0.5f);

            // Step 5: Fade Out the death screen
            yield return StartCoroutine(FadeOut());
        }

        public void Respawn()
        {
            // Move player to checkpoint and restore states
            transform.position = respawnPoint;
            transform.rotation = respawnRotation;

            currentHealth = playerHealth;

            // Re-enable player controls
            TPUserControl controller = GetComponent<TPUserControl>();
            if (controller != null) controller.enabled = true;

            TPCharacter character = GetComponent<TPCharacter>();
            if (character != null) character.enabled = true;

            // Re-enable vulnerability after respawn
            SetVulnerable();

            EnemyRespawner respawner = FindObjectOfType<EnemyRespawner>();
            if (respawner != null)
                respawner.RespawnEnemy();

            if (anim != null)
                anim.SetTrigger("Respawned");

            isDead = false; // allow future deaths
            Debug.Log("Player respawned at last checkpoint!");
        }

        private IEnumerator RespawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Respawn();
        }

        // =======================
        // Damage & Defense
        // =======================
        public void TakeDamage(int amount)
        {
            if (!canTakeDamage) return;

            int finalDamage = Mathf.Max(0, amount - defense);
            currentHealth -= finalDamage;

            // Prevent health from going below 0
            if (currentHealth < 0)
                currentHealth = 0;

            Debug.Log($"Player took {finalDamage} damage (blocked {amount - finalDamage}). Current HP: {currentHealth}");

            if (anim != null)
                anim.SetTrigger("Hurt");
        }

        private IEnumerator FadeIn()
        {
            if (youDiedCanvas == null)
                yield break;

            youDiedCanvas.gameObject.SetActive(true);
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                youDiedCanvas.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                yield return null;
            }

            youDiedCanvas.alpha = 1f;
        }
        private IEnumerator FadeOut()
        {
            if (youDiedCanvas == null)
                yield break;

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                youDiedCanvas.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                yield return null;
            }

            youDiedCanvas.alpha = 0f;
            youDiedCanvas.gameObject.SetActive(false);
        }

        // =======================
        // Lifesteal
        // =======================
        public void ApplyLifesteal()
        {
            if (lifesteal <= 0) return;

            currentHealth += lifesteal;
            if (currentHealth > playerHealth)
                currentHealth = playerHealth;

            Debug.Log($"Lifesteal +{lifesteal} HP. Current HP: {currentHealth}");
        }

        public void HealFromLifesteal(int damageDealt)
        {
            int healAmount = lifesteal;
            currentHealth = Mathf.Min(currentHealth + healAmount, playerHealth);
            Debug.Log($"Player healed {healAmount} from lifesteal. Current HP: {currentHealth}");
        }

        // =======================
        // Money & Damage
        // =======================
        public void AddMoney(int amount)
        {
            money += amount;
            Debug.Log($"Money increased. Current money: {money}");
        }

        public void SpendMoney(int amount)
        {
            if (money >= amount)
            {
                money -= amount;
                Debug.Log($"Money spent. Current money: {money}");
            }
            else
            {
                Debug.Log("Not enough money!");
            }
        }

        public int DealDamage()
        {
            ApplyLifesteal();
            return damage;
        }

        // =======================
        // Invincibility
        // =======================
        public void SetInvincible()
        {
            canTakeDamage = false;
            Debug.Log("Player is now INVINCIBLE");
        }

        public void SetVulnerable()
        {
            canTakeDamage = true;
            Debug.Log("Player is now VULNERABLE");
        }

        

        private IEnumerator RestoreInventoriesAfterLoad(SaveData data)
        {
            // Make sure all the other scripts (Start/OnEnable) have run.
            // EndOfFrame twice = super safe for UI grids that build in Start()
            yield return new WaitForEndOfFrame();
            yield return null;

            // --- Grab refs ---
            GridMaker grid = FindObjectOfType<GridMaker>();
            DjimatSystem djimatSystem = FindObjectOfType<DjimatSystem>();

            SacredStoneGridMaker sacredGrid = FindObjectOfType<SacredStoneGridMaker>();
            KeyItemGridMaker keyGrid = FindObjectOfType<KeyItemGridMaker>();

            // ==========================
            // 1. Djimat (equipped + bag)
            // ==========================
            if (grid != null)
            {
                // wipe UI first
                foreach (var eq in grid.equippedGridParent.GetComponentsInChildren<EquippedSlotUI>())
                    eq.AssignDjimat(null);

                foreach (var inv in grid.inventoryGridParent.GetComponentsInChildren<InventorySlotUI>())
                    inv.AssignDjimat(null);

                // restore equipped djimats in order
                foreach (string id in data.equippedDjimatIDs)
                {
                    DjimatItem item = Resources.Load<DjimatItem>($"Items/{id}");
                    if (item == null) continue;

                    foreach (var slot in grid.equippedGridParent.GetComponentsInChildren<EquippedSlotUI>())
                    {
                        if (slot.equippedDjimat == null)
                        {
                            slot.AssignDjimat(item);
                            break;
                        }
                    }
                }

                // restore backpack djimats
                foreach (string id in data.inventoryDjimatIDs)
                {
                    DjimatItem item = Resources.Load<DjimatItem>($"Items/{id}");
                    if (item != null)
                    {
                        grid.AddToInventory(item);
                    }
                }
            }

            // ==========================
            // 2. Sacred Stones
            // ==========================
            if (sacredGrid != null && sacredGrid.stoneInventory != null)
            {
                var sacredInv = sacredGrid.stoneInventory;

                // clear first
                sacredInv.stones.Clear();

                // IMPORTANT: use AddStone() so it fires OnInventoryChanged
                foreach (string id in data.sacredStoneIDs)
                {
                    DjimatItem item = Resources.Load<DjimatItem>($"Items/{id}");
                    if (item != null)
                    {
                        sacredInv.AddStone(item);
                    }
                }

                // Safety refresh just in case
                sacredGrid.RefreshGrid();
            }

            // ==========================
            // 3. Key Items
            // ==========================
            if (keyGrid != null && keyGrid.keyItemInventory != null)
            {
                var keyInv = keyGrid.keyItemInventory;

                // clear existing
                keyInv.ClearInventory(); // this also fires OnInventoryChanged

                // IMPORTANT: use AddKeyItem() so it fires OnInventoryChanged
                foreach (string id in data.keyItemIDs)
                {
                    DjimatItem item = Resources.Load<DjimatItem>($"Items/{id}");
                    if (item != null)
                    {
                        keyInv.AddKeyItem(item);
                    }
                }

                // Safety refresh
                keyGrid.RefreshGrid();
            }

            // ==========================
            // 4. Merchant sold state
            // ==========================
            MerchantCatalog merchant = FindObjectOfType<MerchantCatalog>();
            if (merchant != null)
            {
                merchant.ApplySoldOutFromSave(data.soldOutItems);
            }

            // ==========================
            // 5. Sync DjimatSystem stats & UI
            // ==========================
            if (djimatSystem != null)
            {
                // first, tell DjimatSystem that the "base" stats should now be whatever
                // we loaded from disk (after merchant upgrades etc)
                djimatSystem.SyncBaseStatsFromPlayer();

                // then re-apply bonuses from equipped djimats on top of that
                djimatSystem.ApplyBonusesAfterLoad();

                // then force the diamond-slot UI to redraw using the loaded slotMax
                djimatSystem.RefreshLimitUIAfterLoad();
            }

            // ==========================
            // 6. Restore Weapon Upgrade
            // ==========================
            WeaponUpgradeManager wum = FindObjectOfType<WeaponUpgradeManager>();
            if (wum != null)
            {
                // pull saved level
                wum.currentLevel = data.weaponUpgradeLevel;

                // apply its damage + UI
                wum.ApplyDamageForCurrentLevel();
            }

            // 7. Restore world persistent state (door unlocks, picked items)
            if (GameManager.Instance != null)
            {
                // load lists from save into GameManager's hash sets
                GameManager.Instance.ApplyLoadedState(data.openedDoorIDs, data.collectedPickupIDs);

                // force all doors in the scene to visually match that state
                foreach (var door in FindObjectsOfType<LockedDoorInteraction>())
                {
                    door.ApplyWorldState();
                }

                // force all pickups in the scene to despawn if already collected
                foreach (var pickup in FindObjectsOfType<PickableItem>())
                {
                    pickup.ApplyWorldState();
                }
            }

        }


    }

}
