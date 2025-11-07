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

    [Header("Audio (via AudioManager)")]
    public AudioClip upgradeSfx;   // play on successful upgrade
    public AudioClip errorSfx;     // play on failure (optional)

    private PlayerManager playerManager;
    private SacredStoneGridMaker stoneInventory;
    private DjimatSystem djimatSystem;

    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        stoneInventory = FindObjectOfType<SacredStoneGridMaker>();
        djimatSystem = FindObjectOfType<DjimatSystem>();

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(TryUpgrade);

        ApplyDamageForCurrentLevel(); // just updates labels + recompute totals
        if (priceText != null && currentLevel < pricePerLevel.Length)
            priceText.text = pricePerLevel[currentLevel];
    }

    void Update()
    {
        if (priceText != null && currentLevel < pricePerLevel.Length)
            priceText.text = pricePerLevel[currentLevel];
    }

    void TryUpgrade()
    {
        // guard: already max
        if (currentLevel >= maxLevel)
        {
            if (feedbackText) feedbackText.text = "Max level reached!";
            PlayError();
            return;
        }

        // guard: money
        int cost = (currentLevel < upgradeCosts.Length) ? upgradeCosts[currentLevel] : 0;
        if (playerManager.money < cost)
        {
            if (feedbackText) feedbackText.text = "Not enough money!";
            PlayError();
            return;
        }

        // guard: stone requirement
        DjimatItem requiredStone = GetRequiredStoneForLevel(currentLevel + 1);
        if (requiredStone == null || stoneInventory == null || !stoneInventory.HasStone(requiredStone))
        {
            if (feedbackText) feedbackText.text = "Missing required stone!";
            PlayError();
            return;
        }

        // pay + consume
        playerManager.money -= cost;
        stoneInventory.RemoveFromInventory(requiredStone); // consume from SO + refresh UI

        // level up
        currentLevel++;

        // refresh labels + recompute stacked stats (Djimat + upgrade)
        ApplyDamageForCurrentLevel();

        if (feedbackText) feedbackText.text = $"Weapon upgraded to +{currentLevel}!";
        if (weaponinfoText) weaponinfoText.text = $"Courteous+{currentLevel}";
        if (weaponinfoText_Inventory) weaponinfoText_Inventory.text = $"Courteous+{currentLevel}";

        // SFX success
        PlayUpgrade();
    }

    /// <summary>
    /// Only updates labels and asks DjimatSystem to rebuild totals so upgrades stack with Djimat bonuses.
    /// </summary>
    public void ApplyDamageForCurrentLevel()
    {
        if (weaponinfoText) weaponinfoText.text = $"Courteous+{currentLevel}";
        if (weaponinfoText_Inventory) weaponinfoText_Inventory.text = $"Courteous+{currentLevel}";

        if (djimatSystem == null) djimatSystem = FindObjectOfType<DjimatSystem>();
        // Recompute final stats (base + upgrade + djimat)
        djimatSystem?.ApplyBonusesAfterLoad();
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

    // ---------- SFX helpers ----------
    private void PlayUpgrade()
    {
        if (upgradeSfx != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(upgradeSfx);
    }

    private void PlayError()
    {
        if (errorSfx != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(errorSfx);
    }
}
