using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string playerTag = "Player";
    public float interactDistance = 3f;

    private Transform player;
    private EnemyRespawner respawner;
    private DjimatSystem djimatSystem;
    private bool isPlayerNear = false;

    private void Start()
    {
        respawner = FindObjectOfType<EnemyRespawner>();
        djimatSystem = FindObjectOfType<DjimatSystem>();

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;
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
            PromptUIManagerCheckpoint.Instance?.ShowPrompt("[E] Interact");
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
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        if (playerManager == null) return;

        // Heal player to max
        playerManager.currentHealth = playerManager.playerHealth;

        // Update respawn point
        playerManager.SetCheckpoint(transform.position, transform.rotation);
        Debug.Log($"Checkpoint updated at {transform.position}");

        // Respawn enemies
        // Respawn / reset ALL enemies in the scene
        EnemyRespawner[] allRespawners = FindObjectsOfType<EnemyRespawner>();
        foreach (var r in allRespawners)
        {
            r.ForceRespawnNow();
        }

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
            // use its memory instead of scanning UI children
            foreach (var soldName in merchant.GetSoldOutItemNames())
            {
                data.soldOutItems.Add(soldName);
            }
        }

        // --- Weapon Upgrade ---
        WeaponUpgradeManager wum = FindObjectOfType<WeaponUpgradeManager>();
        if (wum != null)
        {
            data.weaponUpgradeLevel = wum.currentLevel;
        }

        // --- World State (doors opened, pickups collected) ---
        if (GameManager.Instance != null)
        {
            foreach (var doorId in GameManager.Instance.GetOpenedDoors())
                data.openedDoorIDs.Add(doorId);

            foreach (var pickupId in GameManager.Instance.GetCollectedPickups())
                data.collectedPickupIDs.Add(pickupId);
        }


        // Save it once
        SaveManager.SaveGame(data);
        Debug.Log("Checkpoint saved — Player + Djimat + Stones + Keys + Merchant + WeaponUpgrade");

        // Hide prompt UIs
        PromptUIManager.Instance?.HidePrompt();
        PromptUIManagerCheckpoint.Instance?.HidePrompt();


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
