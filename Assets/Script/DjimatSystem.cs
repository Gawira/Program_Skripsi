using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class DjimatSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private GridMaker gridMaker;
    [SerializeField] private DjimatLimitUI limitUI; // NEW

    private int baseHealth;
    private int baseDamage;

    public event Action OnChanged;

    void Awake()
    {
        if (playerManager == null)
            playerManager = FindObjectOfType<PlayerManager>();

        if (gridMaker == null)
            gridMaker = FindObjectOfType<GridMaker>();

        if (limitUI == null)
            limitUI = FindObjectOfType<DjimatLimitUI>();

        if (playerManager != null)
        {
            baseHealth = playerManager.playerHealth;
            baseDamage = playerManager.damage;
        }
    }

    void Start()
    {
        if (limitUI != null)
        {
            limitUI.GenerateSlots(SlotCapacity);
            limitUI.UpdateUsage(GetCurrentUsedSlots());
        }
    }

    public int SlotCapacity => (playerManager != null) ? playerManager.slotMax : 2;

    public int GetCurrentUsedSlots()
    {
        int used = 0;
        foreach (var eqSlot in gridMaker.equippedGridParent.GetComponentsInChildren<EquippedSlotUI>())
        {
            if (eqSlot.equippedDjimat != null)
                used += eqSlot.equippedDjimat.slotCost;
        }
        return used;
    }

    public bool EquipToSlot(EquippedSlotUI slot, DjimatItem item)
    {
        if (slot == null || item == null) return false;

        int used = GetCurrentUsedSlots();
        int newUsed = used;

        if (slot.equippedDjimat != null)
            newUsed -= slot.equippedDjimat.slotCost;

        newUsed += item.slotCost;

        if (newUsed > SlotCapacity)
        {
            Debug.LogWarning("Not enough capacity!");
            return false;
        }

        slot.AssignDjimat(item);
        ApplyBonuses();

        OnChanged?.Invoke();
        UpdateLimitUI(); // NEW
        return true;
    }

    public void UnequipSlot(EquippedSlotUI slot)
    {
        if (slot == null || slot.equippedDjimat == null) return;

        gridMaker.AddToInventory(slot.equippedDjimat);
        slot.AssignDjimat(null);

        ApplyBonuses();
        OnChanged?.Invoke();
        UpdateLimitUI(); // NEW
    }

    private void ApplyBonuses()
    {
        if (playerManager == null) return;

        playerManager.playerHealth = baseHealth;
        playerManager.damage = baseDamage;

        foreach (var eqSlot in gridMaker.equippedGridParent.GetComponentsInChildren<EquippedSlotUI>())
        {
            if (eqSlot.equippedDjimat == null) continue;

            playerManager.playerHealth += eqSlot.equippedDjimat.healthBonus;
            playerManager.damage += eqSlot.equippedDjimat.damageBonus;
        }

        if (playerManager.currentHealth > playerManager.playerHealth)
            playerManager.currentHealth = playerManager.playerHealth;
    }

    private void UpdateLimitUI()
    {
        if (limitUI != null)
            limitUI.UpdateUsage(GetCurrentUsedSlots());
    }
}
