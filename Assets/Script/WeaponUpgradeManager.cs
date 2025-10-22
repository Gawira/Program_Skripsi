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

    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        stoneInventory = FindObjectOfType<SacredStoneGridMaker>();

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(TryUpgrade);

        ApplyDamageForCurrentLevel();
    }

    void Update()
    {
        priceText.text = pricePerLevel[currentLevel];
    }

        void TryUpgrade()
        {
        if (currentLevel >= maxLevel)
        {
            feedbackText.text = "Max level reached!";
            return;
        }

        int cost = upgradeCosts[currentLevel];
        
        if (playerManager.money < cost)
        {
            feedbackText.text = "Not enough money!";
            return;
        }

        DjimatItem requiredStone = GetRequiredStoneForLevel(currentLevel + 1);
        if (requiredStone == null || !stoneInventory.HasStone(requiredStone))
        {
            feedbackText.text = "Missing required stone!";
            return;
        }

        playerManager.money -= cost;
        stoneInventory.RemoveStone(requiredStone);

        currentLevel++;
        ApplyDamageForCurrentLevel();

        feedbackText.text = $"Weapon upgraded to +{currentLevel}!";
        weaponinfoText.text = $"Courteous+{currentLevel}";
        weaponinfoText_Inventory.text = $"Courteous+{currentLevel}";
    }

    private void ApplyDamageForCurrentLevel()
    {
        if (playerManager != null && currentLevel < damagePerLevel.Length)
        {
            playerManager.damage = damagePerLevel[currentLevel];
        }
    }

    DjimatItem GetRequiredStoneForLevel(int level)
    {
        switch (level)
        {
            case 1: 
                return tarnishedStone;
            case 2: 
                return sacredStone;
            case 3: 
                return pureStone;
            case 4: 
                return divineStone;
            default: 
                return null;
        }
    }
}
