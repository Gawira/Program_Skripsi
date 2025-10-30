using System;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class DjimatSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private GridMaker gridMaker;
    [SerializeField] private DjimatLimitUI limitUI;

    // NEW: so we can control movement speed for Haste
    [SerializeField] private TPCharacter tpChar;
    [SerializeField] private float hasteMultiplier = 3f; // “super super fast”

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

        // pick up TPCharacter so we can scale speed
        if (tpChar == null)
        {
            tpChar = playerManager != null
                ? (playerManager.thirdPersonCharacter != null
                    ? playerManager.thirdPersonCharacter
                    : playerManager.GetComponent<TPCharacter>())
                : FindObjectOfType<TPCharacter>();
        }

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

    public int SlotCapacity => playerManager != null ? playerManager.slotMax : 2;

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

    public void SyncBaseStatsFromPlayer()
    {
        if (playerManager == null) return;
        baseHealth = playerManager.playerHealth;
        baseDamage = playerManager.damage;
        baseLifesteal = playerManager.lifesteal;
        baseDefense = playerManager.defense;
        // speeds: TPCharacter caches its own base on Awake; nothing to do here
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

        // 1) Reset player base stats
        playerManager.playerHealth = baseHealth;
        playerManager.damage = baseDamage;
        playerManager.lifesteal = baseLifesteal;
        playerManager.defense = baseDefense;

        // 2) Reset special flags
        playerManager.canReviveOnce = false;
        playerManager.hasRegen = false;
        playerManager.regenPerSecond = 0;
        playerManager.healthMultiplier = 1f;

        // NEW: movement reset
        if (tpChar != null) tpChar.ResetSpeedToBase();

        // track effects we need to apply after scanning all items
        bool wantGodMode = false;
        bool wantHaste = false;

        // 3) Scan equipped items
        foreach (var eqSlot in gridMaker.equippedGridParent.GetComponentsInChildren<EquippedSlotUI>())
        {
            if (eqSlot.equippedDjimat == null) continue;

            DjimatItem item = eqSlot.equippedDjimat;

            // flat stats
            playerManager.playerHealth += item.healthBonus;
            playerManager.damage += item.damageBonus;
            playerManager.lifesteal += item.lifestealBonus;
            playerManager.defense += item.defenseBonus;

            // specials
            switch (item.itemName)
            {
                case "Paper of Oath":
                    playerManager.canReviveOnce = true;
                    break;

                case "Sacred Vest":
                    playerManager.healthMultiplier *= 0.5f;
                    break;

                case "Pure Water":
                    playerManager.hasRegen = true;
                    playerManager.regenPerSecond += 2;
                    break;

                // === NEW ===
                case "Haste":
                    wantHaste = true;
                    break;

                case "God Mode":
                    wantGodMode = true;
                    break;
            }
        }

        // 4) Apply HP multiplier after flats
        playerManager.playerHealth = Mathf.RoundToInt(playerManager.playerHealth * playerManager.healthMultiplier);

        // 5) Clamp current HP
        if (playerManager.currentHealth > playerManager.playerHealth)
            playerManager.currentHealth = playerManager.playerHealth;

        // 6) Apply movement + invincibility toggles
        if (tpChar != null && wantHaste)
            tpChar.ApplySpeedMultiplier(hasteMultiplier);

        if (wantGodMode)
            playerManager.SetInvincible();
        else
            playerManager.SetVulnerable();

        Debug.Log(
            $"[DjimatSystem] Final Stats → HP:{playerManager.playerHealth}, DMG:{playerManager.damage}, " +
            $"LS:{playerManager.lifesteal}, DEF:{playerManager.defense} | " +
            $"Revive:{playerManager.canReviveOnce} Regen:{playerManager.hasRegen}({playerManager.regenPerSecond}/s) " +
            $"HPx{playerManager.healthMultiplier} | Haste:{wantHaste} GodMode:{wantGodMode}"
        );
    }

    private void UpdateLimitUI()
    {
        if (limitUI != null)
            limitUI.UpdateUsage(GetCurrentUsedSlots());
    }
}
