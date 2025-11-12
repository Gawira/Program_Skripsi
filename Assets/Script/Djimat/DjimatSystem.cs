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
    public void RecomputeNow()
    {
        ApplyBonuses();
        OnChanged?.Invoke();
        UpdateLimitUI();
    }

    // --- NEW: compute additive weapon-upgrade bonus from WeaponUpgradeManager ---
    private int GetWeaponUpgradeBonus()
    {
        var w = FindObjectOfType<WeaponUpgradeManager>();
        if (w == null || w.damagePerLevel == null || w.damagePerLevel.Length == 0) return 0;

        int clamped = Mathf.Clamp(w.currentLevel, 0, w.damagePerLevel.Length - 1);
        int baseVal = w.damagePerLevel[0];
        // treat it as a bonus above base weapon damage
        return Mathf.Max(0, w.damagePerLevel[clamped] - baseVal);
    }

    // ===== THE IMPORTANT PART =====
    private void ApplyBonuses()
    {
        if (playerManager == null) return;

        // --- keep previous state to preserve ratio ---
        int prevMax = Mathf.Max(1, playerManager.playerHealth); // avoid div-by-zero
        int prevCurrent = Mathf.Clamp(playerManager.currentHealth, 0, prevMax);

        // 1) Reset player base stats (includes weapon-upgrade bonus on damage)
        int upgradeBonus = GetWeaponUpgradeBonus();

        playerManager.playerHealth = baseHealth;
        playerManager.damage = baseDamage + upgradeBonus;
        playerManager.lifesteal = baseLifesteal;
        playerManager.defense = baseDefense;

        // 2) Reset specials
        playerManager.canReviveOnce = false;
        playerManager.hasRegen = false;
        playerManager.regenPerSecond = 0;
        if (tpChar != null) tpChar.ResetSpeedToBase();

        bool wantGodMode = false;
        bool wantHaste = false;

        // 3) Add Djimat bonuses
        foreach (var eqSlot in gridMaker.equippedGridParent.GetComponentsInChildren<EquippedSlotUI>())
        {
            if (eqSlot.equippedDjimat == null) continue;

            var item = eqSlot.equippedDjimat;
            playerManager.playerHealth += item.healthBonus;
            playerManager.damage += item.damageBonus;
            playerManager.lifesteal += item.lifestealBonus;
            playerManager.defense += item.defenseBonus;

            switch (item.itemName)
            {
                case "Paper of Oath": playerManager.canReviveOnce = true; break;
                case "Pure Water": playerManager.hasRegen = true; playerManager.regenPerSecond += 2; break;
                case "Haste": wantHaste = true; break;
                case "God Mode": wantGodMode = true; break;
            }
        }

        // 4) Preserve health percentage relative to new max
        int newMax = Mathf.Max(1, playerManager.playerHealth);
        float ratio = Mathf.Clamp01(prevCurrent / (float)prevMax);  // e.g., 0.75
        playerManager.currentHealth = Mathf.Clamp(Mathf.RoundToInt(newMax * ratio), 0, newMax);

        // 5) Toggles
        if (tpChar != null && wantHaste) tpChar.ApplySpeedMultiplier(hasteMultiplier);
        if (wantGodMode) playerManager.SetInvincible(); else playerManager.SetVulnerable();

        Debug.Log($"[DjimatSystem] Final Stats → HP:{playerManager.playerHealth} (cur {playerManager.currentHealth}), " +
                  $"DMG:{playerManager.damage}, LS:{playerManager.lifesteal}, DEF:{playerManager.defense}");
    }

    private void UpdateLimitUI()
    {
        if (limitUI != null)
            limitUI.UpdateUsage(GetCurrentUsedSlots());
    }
}
