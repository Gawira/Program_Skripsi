using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponUpgradeManager : MonoBehaviour
{
    [Header("Upgrade Settings")]
    public int currentLevel = 0;       // 0 = base
    public int maxLevel = 4;
    public int[] upgradeCosts;

    [Header("Damage Settings")]
    public int[] damagePerLevel = { 10, 20, 25, 28, 40 }; // index 0 = base damage
    public string[] pricePerLevel = { "100", "150", "200", "400", };

    [Header("Required Stones Per Level")]
    public DjimatItem tarnishedStone;
    public DjimatItem sacredStone;
    public DjimatItem pureStone;
    public DjimatItem divineStone;

    [Header("References")]
    public Button upgradeButton;
    public TMP_Text feedbackText;
    public TMP_Text weaponinfoText;
    public TMP_Text weaponinfoText_Inventory;
    public TMP_Text priceText;

    private PlayerManager playerManager;
    private SacredStoneGridMaker stoneInventory;
    private DjimatSystem djimatSystem; // <-- to recompute final stats

    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        stoneInventory = FindObjectOfType<SacredStoneGridMaker>();
        djimatSystem = FindObjectOfType<DjimatSystem>();

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(TryUpgrade);

        // no longer sets damage directly; just refreshes UI and asks DjimatSystem to recompute
        ApplyDamageForCurrentLevel();
    }

    void Update()
    {
        if (priceText != null && currentLevel < pricePerLevel.Length)
            priceText.text = pricePerLevel[currentLevel];
    }

    void TryUpgrade()
    {
        if (currentLevel >= maxLevel)
        {
            if (feedbackText) feedbackText.text = "Max level reached!";
            return;
        }

        int cost = (currentLevel < upgradeCosts.Length) ? upgradeCosts[currentLevel] : 0;
        if (playerManager.money < cost)
        {
            if (feedbackText) feedbackText.text = "Not enough money!";
            return;
        }

        DjimatItem requiredStone = GetRequiredStoneForLevel(currentLevel + 1);
        if (requiredStone == null || !stoneInventory.HasStone(requiredStone))
        {
            if (feedbackText) feedbackText.text = "Missing required stone!";
            return;
        }

        // pay + consume
        playerManager.money -= cost;
        stoneInventory.RemoveStone(requiredStone);

        // level up
        currentLevel++;

        // refresh labels + recompute stats (now additive with Djimat)
        ApplyDamageForCurrentLevel();

        if (feedbackText) feedbackText.text = $"Weapon upgraded to +{currentLevel}!";
        if (weaponinfoText) weaponinfoText.text = $"Courteous+{currentLevel}";
        if (weaponinfoText_Inventory) weaponinfoText_Inventory.text = $"Courteous+{currentLevel}";
    }

    /// <summary>
    /// Now only updates UI and tells DjimatSystem to rebuild totals.
    /// The actual damage math lives in DjimatSystem so it stacks with items.
    /// </summary>
    public void ApplyDamageForCurrentLevel()
    {
        // Update labels
        if (weaponinfoText) weaponinfoText.text = $"Courteous+{currentLevel}";
        if (weaponinfoText_Inventory) weaponinfoText_Inventory.text = $"Courteous+{currentLevel}";

        // Ask DjimatSystem to recompute final stats (base + upgrade bonus + djimat)
        if (djimatSystem == null) djimatSystem = FindObjectOfType<DjimatSystem>();
        djimatSystem?.RecomputeNow();
    }

    DjimatItem GetRequiredStoneForLevel(int level)
    {
        switch (level)
        {
            case 1: return tarnishedStone;
            case 2: return sacredStone;
            case 3: return pureStone;
            case 4: return divineStone;
            default: return null;
        }
    }
}
