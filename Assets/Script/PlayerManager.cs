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
        public int damage = 10;
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

        [SerializeField] private PlayerAudioController audioController;

        // --- internal guard ---
        public bool isDead = false;

        // >>> NEW <<< Djimat special effect flags
        [Header("Djimat Special Effects")]
        [Tooltip("If true: lethal damage will consume this and revive you instead of full death.")]
        public bool canReviveOnce = false;

        [Tooltip("If true: you heal passively over time.")]
        public bool hasRegen = false;

        [Tooltip("HP recovered per second if hasRegen is true.")]
        public int regenPerSecond = 0;

        [Tooltip("Multiplier for max HP from Sacred Vest, applied in DjimatSystem.")]
        public float healthMultiplier = 1f;

        // regen timer for ticking 1/sec
        private float regenTimer = 0f;
        // >>> END NEW <<<

        private void Start()
        {
            anim = GetComponent<Animator>();
            if (audioController == null)
                audioController = GetComponent<PlayerAudioController>();

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
            // >>> NEW <<< Passive regen from Pure Water
            if (!isDead && hasRegen && regenPerSecond > 0 && currentHealth > 0 && currentHealth < playerHealth)
            {
                regenTimer += Time.deltaTime;
                if (regenTimer >= 1f)
                {
                    regenTimer -= 1f;

                    currentHealth += regenPerSecond;
                    if (currentHealth > playerHealth)
                        currentHealth = playerHealth;

                    Debug.Log($"[Regen] +{regenPerSecond} HP from Pure Water. Current HP: {currentHealth}");
                }
            }
            // >>> END NEW <<<

            // Only trigger death logic once
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

            // >>> NEW <<< Second chance from Paper of Oath
            if (canReviveOnce)
            {
                // consume the revive
                canReviveOnce = false;

                Debug.Log("Player avoided death thanks to Paper of Oath!");

                // bring player back with half HP (at least 1)
                currentHealth = Mathf.Max(1, playerHealth / 2);

                // short invulnerability so you don't instantly die again
                StartCoroutine(TemporaryInvulnerability(2f));

                // play a 'revive' style anim (reusing Respawned trigger)
                if (anim != null)
                    anim.SetTrigger("Respawned");

                // IMPORTANT: do NOT mark isDead, do NOT run normal death flow
                return;
            }
            // >>> END NEW <<<

            // this is actual death flow
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

            // Full death sequence (fade -> respawn -> fade out)
            StartCoroutine(HandleDeathSequence());
        }

        // >>> NEW <<< short invulnerability after revive
        private IEnumerator TemporaryInvulnerability(float seconds)
        {
            SetInvincible();
            yield return new WaitForSeconds(seconds);
            SetVulnerable();
        }
        // >>> END NEW <<<

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

            float reduction = amount * (defense / 100f);
            float dmgAfterReduction = amount - reduction;
            int finalDamage = Mathf.Max(0, Mathf.RoundToInt(dmgAfterReduction));

            // only bother doing hurt SFX if we actually lost HP > 0
            bool actuallyTookDamage = finalDamage > 0;

            currentHealth -= finalDamage;
            if (currentHealth < 0)
                currentHealth = 0;

            Debug.Log(
                $"Player took {finalDamage} damage " +
                $"(raw {amount}, reduced by {reduction:0.0} from DEF {defense}). " +
                $"Current HP: {currentHealth}"
            );

            if (actuallyTookDamage)
            {
                if (anim != null)
                    anim.SetTrigger("Hurt");

                if (audioController != null)
                    audioController.PlayHurtSFX();
            }
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

        // =======================
        // RestoreInventoriesAfterLoad
        // (no changes here, leaving yours as-is)
        // =======================
        private IEnumerator RestoreInventoriesAfterLoad(SaveData data)
        {
            // Let other scripts initialize first so UI grids exist
            yield return new WaitForEndOfFrame();
            yield return null;

            // --- references ---
            GridMaker grid = FindObjectOfType<GridMaker>();
            DjimatSystem djimatSystem = FindObjectOfType<DjimatSystem>();
            SacredStoneGridMaker sacredGrid = FindObjectOfType<SacredStoneGridMaker>();
            KeyItemGridMaker keyGrid = FindObjectOfType<KeyItemGridMaker>();

            // Djimat load...
            if (grid != null)
            {
                foreach (var eq in grid.equippedGridParent.GetComponentsInChildren<EquippedSlotUI>())
                    eq.AssignDjimat(null);

                foreach (var inv in grid.inventoryGridParent.GetComponentsInChildren<InventorySlotUI>())
                    inv.AssignDjimat(null);

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

                foreach (string id in data.inventoryDjimatIDs)
                {
                    DjimatItem item = Resources.Load<DjimatItem>($"Items/{id}");
                    if (item != null)
                    {
                        grid.AddToInventory(item);
                    }
                }
            }

            // Sacred Stones
            if (sacredGrid != null && sacredGrid.stoneInventory != null)
            {
                var sacredInv = sacredGrid.stoneInventory;
                sacredInv.stones.Clear();

                foreach (string id in data.sacredStoneIDs)
                {
                    DjimatItem item = Resources.Load<DjimatItem>($"Items/{id}");
                    if (item != null)
                    {
                        sacredInv.AddStone(item);
                    }
                }

                sacredGrid.RefreshGrid();
            }

            // Key Items
            if (keyGrid != null && keyGrid.keyItemInventory != null)
            {
                var keyInv = keyGrid.keyItemInventory;
                keyInv.ClearInventory();

                foreach (string id in data.keyItemIDs)
                {
                    DjimatItem item = Resources.Load<DjimatItem>($"Items/{id}");
                    if (item != null)
                    {
                        keyInv.AddKeyItem(item);
                    }
                }

                keyGrid.RefreshGrid();
            }

            // Merchant
            MerchantCatalog merchant = FindObjectOfType<MerchantCatalog>();
            if (merchant != null)
            {
                merchant.ApplySoldOutFromSave(data.soldOutItems);
            }

            // DjimatSystem sync
            if (djimatSystem != null)
            {
                djimatSystem.SyncBaseStatsFromPlayer();
                djimatSystem.ApplyBonusesAfterLoad();
                djimatSystem.RefreshLimitUIAfterLoad();
            }

            // Weapon Upgrade
            WeaponUpgradeManager wum = FindObjectOfType<WeaponUpgradeManager>();
            if (wum != null)
            {
                wum.currentLevel = data.weaponUpgradeLevel;
                wum.ApplyDamageForCurrentLevel();
            }

            // World state (doors, pickups)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ApplyLoadedState(
                    data.openedDoorIDs,
                    data.collectedPickupIDs
                );

                foreach (var doorLocked in FindObjectsOfType<LockedDoorInteraction>())
                {
                    doorLocked.ApplyWorldState();
                }

                foreach (var doorOneWay in FindObjectsOfType<DoorInteraction>())
                {
                    doorOneWay.ApplyWorldState();
                }

                foreach (var pickup in FindObjectsOfType<PickableItem>())
                {
                    pickup.ApplyWorldState();
                }
            }
        }
    }
}
