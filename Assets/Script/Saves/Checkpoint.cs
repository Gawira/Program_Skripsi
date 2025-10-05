using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private EnemyRespawner respawner;
    private DjimatSystem djimatSystem;

    private void Start()
    {
        respawner = FindObjectOfType<EnemyRespawner>();
        djimatSystem = FindObjectOfType<DjimatSystem>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerManager player = other.GetComponent<PlayerManager>();
        if (player == null) return;

        // === Heal Player ===
        player.currentHealth = player.playerHealth; // restore full HP

        // === Respawn enemies ===
        if (respawner != null)
            respawner.RespawnEnemy();

        // === Save Djimat setup + player data ===
        SaveData data = new SaveData();
        data.playerHealth = player.currentHealth;
        data.playerMoney = player.money;
        data.checkpointPosition = transform.position;

        // Save Djimat equipped and inventory items
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
        Debug.Log("Checkpoint saved and enemies respawned!");
    }
}
