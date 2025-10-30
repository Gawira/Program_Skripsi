using UnityEngine;
using TMPro;

public class Checkpoint : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string playerTag = "Player";
    public float interactDistance = 3f;

    private Transform player;
    private EnemyRespawner respawner;
    private DjimatSystem djimatSystem;
    private bool isPlayerNear = false;

    // ===== Effects & Banner =====
    [Header("Effects")]
    [Tooltip("ParticleSystem played when checkpoint is used.")]
    public ParticleSystem particleEffect;
    public float particleDuration = 2f; // if > 0, we stop/clear after this time

    [Header("UI Banner")]
    [Tooltip("CanvasGroup for the fade-in/out banner UI (background + text).")]
    public CanvasGroup bannerCanvas;
    public TMP_Text bannerText;
    public string checkpointText = "Checkpoint Reached";
    public float fadeIn = 0.4f, hold = 1.0f, fadeOut = 0.6f;

    // ===== Audio (NEW) =====
    [Header("Audio")]
    [Tooltip("SFX played when checkpoint is used.")]
    public AudioClip checkpointSFX;
    [Tooltip("If true, plays as 3D positional SFX at checkpoint location; otherwise as 2D UI SFX.")]
    public bool playAs3D = true;
    [Range(0f, 1f)]
    [Tooltip("Spatial blend for 3D playback (ignored if playAs3D = false).")]
    public float sfxSpatialBlend = 1f;

    private void Start()
    {
        respawner = FindObjectOfType<EnemyRespawner>();
        djimatSystem = FindObjectOfType<DjimatSystem>();

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;

        // Ensure banner starts hidden
        if (bannerCanvas != null)
        {
            bannerCanvas.alpha = 0f;
            bannerCanvas.gameObject.SetActive(false);
        }

        // Ensure particle is idle
        if (particleEffect != null)
            particleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool wasNear = isPlayerNear;
        isPlayerNear = distance <= interactDistance;

        // Show prompt only when player is in range
        if (isPlayerNear && !wasNear)
        {
            PromptUIManagerCheckpoint.Instance?.ShowPrompt("[E] Interact Checkpoint");
        }
        else if (!isPlayerNear && wasNear)
        {
            PromptUIManagerCheckpoint.Instance?.HidePrompt();
        }

        // Press E to interact
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public void Interact()
    {
        if (player == null) return;
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        if (playerManager == null) return;

        // Heal to max & set checkpoint
        playerManager.currentHealth = playerManager.playerHealth;
        playerManager.SetCheckpoint(transform.position, transform.rotation);
        Debug.Log($"Checkpoint updated at {transform.position}");

        // === Play SFX (NEW) ===
        PlayCheckpointSFX();

        // === Particles ===
        if (particleEffect != null)
        {
            particleEffect.Play(true);
            if (particleDuration > 0f)
                StartCoroutine(StopParticleAfter(particleDuration));
        }

        // === Banner ===
        if (bannerCanvas != null)
        {
            if (bannerText != null) bannerText.text = checkpointText;
            StartCoroutine(FadeBanner()); // fades in, holds, fades out
        }

        // Respawn / reset ALL enemies in the scene
        EnemyRespawner[] allRespawners = FindObjectsOfType<EnemyRespawner>();
        foreach (var r in allRespawners)
            r.ForceRespawnNow();

        // Build save data
        SaveData data = new SaveData();

        // --- Player Stats ---
        data.playerHealth = playerManager.playerHealth;
        data.currentHealth = playerManager.currentHealth;
        data.playerMoney = playerManager.money;
        data.damage = playerManager.damage;
        data.lifesteal = playerManager.lifesteal;
        data.defense = playerManager.defense;
        data.slotMax = playerManager.slotMax;

        data.checkpointPosition = transform.position;
        data.checkpointRotation = transform.rotation;

        // --- Djimat System (equipped + bag) ---
        GridMaker gridMaker = FindObjectOfType<GridMaker>();
        if (gridMaker != null)
        {
            foreach (var eqSlot in gridMaker.equippedGridParent.GetComponentsInChildren<EquippedSlotUI>())
            {
                if (eqSlot.equippedDjimat != null)
                    data.equippedDjimatIDs.Add(eqSlot.equippedDjimat.itemName);
            }
            foreach (var invSlot in gridMaker.inventoryGridParent.GetComponentsInChildren<InventorySlotUI>())
            {
                if (invSlot.assignedDjimat != null)
                    data.inventoryDjimatIDs.Add(invSlot.assignedDjimat.itemName);
            }
        }

        // --- Sacred Stones ---
        SacredStoneInventory sacredInv = FindObjectOfType<SacredStoneGridMaker>()?.stoneInventory;
        if (sacredInv != null)
        {
            foreach (var stone in sacredInv.stones)
                data.sacredStoneIDs.Add(stone.itemName);
        }

        // --- Key Items ---
        KeyItemInventory keyInv = FindObjectOfType<KeyItemGridMaker>()?.keyItemInventory;
        if (keyInv != null)
        {
            foreach (var keyItem in keyInv.keyItems)
                data.keyItemIDs.Add(keyItem.itemName);
        }

        // --- Merchant State (sold out items) ---
        MerchantCatalog merchant = FindObjectOfType<MerchantCatalog>();
        if (merchant != null)
        {
            foreach (var soldName in merchant.GetSoldOutItemNames())
                data.soldOutItems.Add(soldName);
        }

        // --- Weapon Upgrade ---
        WeaponUpgradeManager wum = FindObjectOfType<WeaponUpgradeManager>();
        if (wum != null)
            data.weaponUpgradeLevel = wum.currentLevel;

        // --- World State (doors opened, pickups collected) ---
        if (GameManager.Instance != null)
        {
            foreach (var doorId in GameManager.Instance.GetOpenedDoors())
                data.openedDoorIDs.Add(doorId);
            foreach (var pickupId in GameManager.Instance.GetCollectedPickups())
                data.collectedPickupIDs.Add(pickupId);
        }

        // Save once
        SaveManager.SaveGame(data);
        Debug.Log("Checkpoint saved — Player + Djimat + Stones + Keys + Merchant + WeaponUpgrade");

        // Hide prompts after interaction
        PromptUIManager.Instance?.HidePrompt();
        PromptUIManagerCheckpoint.Instance?.HidePrompt();
    }

    // ===== Helpers =====
    private System.Collections.IEnumerator StopParticleAfter(float t)
    {
        yield return new WaitForSeconds(t);
        if (particleEffect != null)
            particleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private System.Collections.IEnumerator FadeBanner()
    {
        bannerCanvas.gameObject.SetActive(true);
        bannerCanvas.alpha = 0f;

        float timer = 0f;
        while (timer < fadeIn)
        {
            timer += Time.deltaTime;
            bannerCanvas.alpha = Mathf.Lerp(0f, 1f, timer / fadeIn);
            yield return null;
        }

        yield return new WaitForSeconds(hold);

        timer = 0f;
        while (timer < fadeOut)
        {
            timer += Time.deltaTime;
            bannerCanvas.alpha = Mathf.Lerp(1f, 0f, timer / fadeOut);
            yield return null;
        }

        bannerCanvas.gameObject.SetActive(false);
    }

    private void PlayCheckpointSFX()
    {
        if (checkpointSFX == null || AudioManager.Instance == null) return;

        if (playAs3D)
        {
            // Uses AudioManager’s pooled one-shot with spatial blend (min/max handled inside)
            AudioManager.Instance.PlaySFXAtPoint(checkpointSFX, transform.position, Mathf.Clamp01(sfxSpatialBlend));
        }
        else
        {
            // Pure 2D SFX (UI/global) — follows SFX volume slider
            AudioManager.Instance.PlaySFX(checkpointSFX);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactDistance);

#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, "Checkpoint Interact Range");
#endif
    }
}
