using UnityEngine;
using TMPro;

public class MerchantCatalog : MonoBehaviour
{
    [Header("References")]
    public GameObject merchantSlotPrefab;  // Prefab with MerchantSlotUI attached
    public Transform gridParent;           // GridLayoutGroup parent in Merchant UI

    [Header("Merchant Inventory")]
    public DjimatItem[] itemsForSale;      // Assign items in Inspector
    public int[] itemPrices;               // Corresponding prices for each item

    private PlayerManager playerManager;
    private GridMaker gridMaker;
    private SacredStoneGridMaker sacredStoneGridMaker;
    private KeyItemGridMaker keyItemGridMaker;

    private DjimatLimitUI limitUI;

    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        gridMaker = FindObjectOfType<GridMaker>();
        sacredStoneGridMaker = FindObjectOfType<SacredStoneGridMaker>();
        keyItemGridMaker = FindObjectOfType<KeyItemGridMaker>();
        limitUI = FindObjectOfType<DjimatLimitUI>();

        BuildCatalog();
    }

    void BuildCatalog()
    {
        for (int i = 0; i < itemsForSale.Length; i++)
        {
            GameObject go = Instantiate(merchantSlotPrefab, gridParent);
            MerchantSlotUI ui = go.GetComponent<MerchantSlotUI>();

            ui.Setup(itemsForSale[i], itemPrices[i], this);
        }
    }

    public void TryPurchase(DjimatItem item, int price, MerchantSlotUI slotUI)
    {
        if (playerManager == null) return;

        // Check if the player has enough money
        if (playerManager.money < price)
        {
            Debug.Log("❌ Not enough money!");
            return;
        }

        // Deduct the money first
        playerManager.money -= price;

        // Check what type of item this is
        switch (item.itemType)
        {
            case DjimatItem.ItemType.Djimat:
                if (gridMaker != null)
                    gridMaker.AddToInventory(item);
                Debug.Log($"🪬 Purchased Djimat: {item.itemName} for {price} gold.");
                break;

            case DjimatItem.ItemType.SacredStone:
                if (sacredStoneGridMaker != null)
                    sacredStoneGridMaker.AddToInventory(item);
                Debug.Log($"🪨 Purchased Sacred Stone: {item.itemName} for {price} gold.");
                break;

            case DjimatItem.ItemType.KeyItem:
                if (keyItemGridMaker != null)
                    keyItemGridMaker.AddToInventory(item);
                Debug.Log($"🗝 Purchased Key Item: {item.itemName} for {price} gold.");
                break;

            case DjimatItem.ItemType.DiamondSlot:
                playerManager.slotMax += item.plusslotCost;
                limitUI.GenerateSlots(playerManager.slotMax);
                Debug.Log($"💎 Purchased Diamond Slot: +{item.plusslotCost} slots.");
                break;

            default:
                Debug.LogWarning("Unknown item type!");
                return;
        }

        // 🟡 Darken the slot icon through UI reference
        if (slotUI != null)
            slotUI.SetDarkened(true);
    }
}
