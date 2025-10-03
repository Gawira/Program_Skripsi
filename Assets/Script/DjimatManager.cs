using System;
using System.Collections.Generic;
using UnityEngine;

public class DjimatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerManager playerManager; // drag player manager here (auto-find if null)

    [Header("Inventory")]
    public List<DjimatItem> inventory = new List<DjimatItem>(); // blue block (found but not equipped)

    [Header("Equipped Slots (Red)")]
    public DjimatItem[] equippedSlots; // length = number of red slots (configure in inspector)

    // cached base stats (so we can reapply bonuses without losing base)
    private int baseHealth;
    private int baseDamage;

    // event for UI to refresh
    public event Action OnChanged;

    void Awake()
    {
        if (playerManager == null)
            playerManager = FindObjectOfType<PlayerManager>();

        if (playerManager != null)
        {
            baseHealth = playerManager.playerHealth;
            baseDamage = playerManager.damage;
        }

        // ensure array exists
        if (equippedSlots == null)
            equippedSlots = new DjimatItem[4]; // default 4 slots if not set
    }

    // ========== Slot capacity logic ==========
    public int SlotCapacity => (playerManager != null) ? playerManager.slotMax : 2;

    public int GetCurrentUsedSlots()
    {
        int used = 0;
        foreach (var d in equippedSlots)
            if (d != null) used += d.slotCost;
        return used;
    }

    // ========== Equip / Unequip ==========
    /// <summary>Equip item into a specific equipped-slot index. If slot already had item it's moved back to inventory. Returns true on success.</summary>
    public bool EquipToSlot(int slotIndex, DjimatItem item)
    {
        if (item == null) return false;
        if (slotIndex < 0 || slotIndex >= equippedSlots.Length) return false;

        int currentUsed = GetCurrentUsedSlots();
        int existingCost = equippedSlots[slotIndex] != null ? equippedSlots[slotIndex].slotCost : 0;

        if (currentUsed - existingCost + item.slotCost > SlotCapacity)
        {
            Debug.LogWarning("Not enough slot capacity to equip " + item.itemName);
            return false;
        }

        // remove the chosen item from inventory (if present)
        if (!inventory.Remove(item))
        {
            // If it's not in inventory it's okay (maybe obtained directly)
            // but do not fail.
        }

        // move old equipped item back to inventory
        if (equippedSlots[slotIndex] != null)
            inventory.Add(equippedSlots[slotIndex]);

        equippedSlots[slotIndex] = item;

        ApplyBonuses();
        OnChanged?.Invoke();
        return true;
    }

    public void UnequipSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSlots.Length) return;
        if (equippedSlots[slotIndex] == null) return;

        inventory.Add(equippedSlots[slotIndex]);
        equippedSlots[slotIndex] = null;
        ApplyBonuses();
        OnChanged?.Invoke();
    }

    /// <summary>Add item to inventory (blue list)</summary>
    public void AddToInventory(DjimatItem item)
    {
        if (item == null) return;
        inventory.Add(item);
        OnChanged?.Invoke();
    }

    // ========== Apply Bonuses ==========
    private void ApplyBonuses()
    {
        if (playerManager == null) return;

        // reset to base
        playerManager.playerHealth = baseHealth;
        playerManager.damage = baseDamage;

        // sum bonuses
        foreach (var d in equippedSlots)
        {
            if (d == null) continue;
            playerManager.playerHealth += d.healthBonus;
            playerManager.damage += d.damageBonus;
        }

        // ensure currentHealth not greater than max
        if (playerManager.currentHealth > playerManager.playerHealth)
            playerManager.currentHealth = playerManager.playerHealth;
    }
}
