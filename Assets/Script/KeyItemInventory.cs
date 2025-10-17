using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Key Item Inventory")]
public class KeyItemInventory : ScriptableObject
{
    [Header("Key Items the player currently has")]
    public List<DjimatItem> keyItems = new List<DjimatItem>();

    public event System.Action OnInventoryChanged;

    /// <summary>
    /// Adds a key item to the inventory if it's not already there.
    /// </summary>
    public void AddKeyItem(DjimatItem item)
    {
        if (item == null) return;

        if (!keyItems.Contains(item))
        {
            keyItems.Add(item);
            Debug.Log($"[KeyItemInventory] Added key item: {item.name}");
            OnInventoryChanged?.Invoke();
        }
        else
        {
            Debug.Log($"[KeyItemInventory] Item '{item.name}' is already in inventory.");
        }
    }

    /// <summary>
    /// Removes a key item from the inventory.
    /// </summary>
    public void RemoveKeyItem(DjimatItem item)
    {
        if (item == null) return;

        if (keyItems.Contains(item))
        {
            keyItems.Remove(item);
            Debug.Log($"[KeyItemInventory] Removed key item: {item.name}");
            OnInventoryChanged?.Invoke();
        }
    }

    /// <summary>
    /// Checks if the inventory contains the specified key item.
    /// </summary>
    public bool HasKeyItem(DjimatItem item)
    {
        return keyItems.Contains(item);
    }

    /// <summary>
    /// Clears all key items (useful for debugging or resets).
    /// </summary>
    public void ClearInventory()
    {
        keyItems.Clear();
        Debug.Log("[KeyItemInventory] Inventory cleared.");
        OnInventoryChanged?.Invoke();
    }
}
