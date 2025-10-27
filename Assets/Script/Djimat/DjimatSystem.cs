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

    // called after load
    public void SyncBaseStatsFromPlayer()
    {
        if (playerManager == null) return;
        baseHealth = playerManager.playerHealth;
        baseDamage = playerManager.damage;
        baseLifesteal = playerManager.lifesteal;
        baseDefense = playerManager.defense;
    }

    public void ApplyBonusesAfterLoad()
    {
        ApplyBonuses();
        UpdateLimitUI();
    }

    public void RefreshLimitUIAfterLoad()
    {
        if (limitUI != null)
        {
            limitUI.GenerateSlots(SlotCapacity);
            limitUI.UpdateUsage(GetCurrentUsedSlots());
        }
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

    // ===== THE IMPORTANT PART =====
    private void ApplyBonuses()
    {
        if (playerManager == null) return;

        // 1. Reset player stats back to base (no Djimat)
        playerManager.playerHealth = baseHealth;
        playerManager.damage = baseDamage;
        playerManager.lifesteal = baseLifesteal;
        playerManager.defense = baseDefense;

        // 2. Reset special Djimat effects
        playerManager.canReviveOnce = false;
        playerManager.hasRegen = false;
        playerManager.regenPerSecond = 0;
        playerManager.healthMultiplier = 1f;

        // 3. Add bonuses from each equipped Djimat
        foreach (var eqSlot in gridMaker.equippedGridParent.GetComponentsInChildren<EquippedSlotUI>())
        {
            if (eqSlot.equippedDjimat == null) continue;

            DjimatItem item = eqSlot.equippedDjimat;

            // flat stat bonuses
            playerManager.playerHealth += item.healthBonus;
            playerManager.damage += item.damageBonus;
            playerManager.lifesteal += item.lifestealBonus;
            playerManager.defense += item.defenseBonus;

            // special passive logic based on itemName
            switch (item.itemName)
            {
                case "Paper of Oath":
                    // gives 1 free revive instead of dying
                    playerManager.canReviveOnce = true;
                    break;

                case "Sacred Vest":
                    // make health bar longer by multiplying max HP
                    playerManager.healthMultiplier *= 0.5f;
                    break;

                case "Pure Water":
                    // passive regen over time
                    playerManager.hasRegen = true;
                    playerManager.regenPerSecond += 2; // +2 HP/sec
                    break;
            }
        }

        // 4. Apply HP multiplier AFTER adding flat bonuses
        playerManager.playerHealth = Mathf.RoundToInt(playerManager.playerHealth * playerManager.healthMultiplier);

        // 5. Clamp current HP so it's not above new max
        if (playerManager.currentHealth > playerManager.playerHealth)
            playerManager.currentHealth = playerManager.playerHealth;

        Debug.Log(
            $"[DjimatSystem] Final Stats → HP:{playerManager.playerHealth}, DMG:{playerManager.damage}, " +
            $"LS:{playerManager.lifesteal}, DEF:{playerManager.defense} | " +
            $"Revive:{playerManager.canReviveOnce} Regen:{playerManager.hasRegen}({playerManager.regenPerSecond}/s) " +
            $"HPx{playerManager.healthMultiplier}"
        );
    }

    private void UpdateLimitUI()
    {
        if (limitUI != null)
            limitUI.UpdateUsage(GetCurrentUsedSlots());
    }
}
