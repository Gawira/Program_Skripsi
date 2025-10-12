using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class DjimatSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private GridMaker gridMaker;
    [SerializeField] private DjimatLimitUI limitUI;

    private int baseHealth;
    private int baseDamage;
    private int baseLifesteal;
    private int baseDefense;

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
            baseLifesteal = playerManager.lifesteal;
            baseDefense = playerManager.defense;
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

    public int SlotCapacity
    {
        get
        {
            return playerManager != null ? playerManager.slotMax : 2;
        }
    }

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
        UpdateLimitUI();
        return true;
    }

    public void UnequipSlot(EquippedSlotUI slot)
    {
        if (slot == null || slot.equippedDjimat == null) return;

        slot.AssignDjimat(null);

        ApplyBonuses();
        OnChanged?.Invoke();
        UpdateLimitUI();
    }

    private void ApplyBonuses()
    {
        if (playerManager == null) return;

        // Reset to base stats
        playerManager.playerHealth = baseHealth;
        playerManager.damage = baseDamage;
        playerManager.lifesteal = baseLifesteal;
        playerManager.defense = baseDefense;

        // Add bonuses from equipped Djimats
        foreach (var eqSlot in gridMaker.equippedGridParent.GetComponentsInChildren<EquippedSlotUI>())
        {
            if (eqSlot.equippedDjimat == null) continue;

            DjimatItem item = eqSlot.equippedDjimat;
            playerManager.playerHealth += item.healthBonus;
            playerManager.damage += item.damageBonus;
            playerManager.lifesteal += item.lifestealBonus;
            playerManager.defense += item.defenseBonus;
        }

        // Clamp health so it doesn't exceed the new max
        if (playerManager.currentHealth > playerManager.playerHealth)
            playerManager.currentHealth = playerManager.playerHealth;

        Debug.Log($"[DjimatSystem] Final Stats → HP: {playerManager.playerHealth}, DMG: {playerManager.damage}, LS: {playerManager.lifesteal}, DEF: {playerManager.defense}");
    }

    private void UpdateLimitUI()
    {
        if (limitUI != null)
            limitUI.UpdateUsage(GetCurrentUsedSlots());
    }
}
