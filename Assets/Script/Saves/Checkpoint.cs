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

    private void Interact()
    {
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        if (playerManager == null) return;

        // === Heal Player ===
        playerManager.currentHealth = playerManager.playerHealth;

        // === Update respawn point ===
        playerManager.SetCheckpoint(transform.position, transform.rotation);
        Debug.Log($"Checkpoint updated at {transform.position}");

        // === Respawn enemies ===
        if (respawner != null)
            respawner.RespawnEnemy();

        // === Save Djimat setup + player data ===
        SaveData data = new SaveData
        {
            playerHealth = playerManager.currentHealth,
            playerMoney = playerManager.money,
            checkpointPosition = transform.position
        };

        if (djimatSystem != null)
        {
            foreach (var eqSlot in FindObjectsOfType<EquippedSlotUI>())
            {
                if (eqSlot.equippedDjimat != null)
                    data.equippedDjimatIDs.Add(eqSlot.equippedDjimat.itemName);
            }

            foreach (var invSlot in FindObjectsOfType<InventorySlotUI>())
            {
                if (invSlot.assignedDjimat != null)
                    data.inventoryDjimatIDs.Add(invSlot.assignedDjimat.itemName);
            }
        }

        SaveManager.SaveGame(data);
        Debug.Log("Checkpoint saved, enemies respawned, and respawn point set!");

        PromptUIManager.Instance?.HidePrompt();
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
