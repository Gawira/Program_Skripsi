using UnityEngine;
using System.Collections.Generic;

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

    // persistent record of "already purchased"
    private HashSet<string> soldItemNames = new HashSet<string>();

    // expose read access for save system
    public IEnumerable<string> GetSoldOutItemNames()
    {
        return soldItemNames;
    }

    // call this during load to re-darken slots that were already sold
    public void ApplySoldOutFromSave(List<string> savedSoldList)
    {
        soldItemNames.Clear();
        foreach (var id in savedSoldList)
            soldItemNames.Add(id);

        // Now update all visible slot UIs
        foreach (var slot in GetComponentsInChildren<MerchantSlotUI>(true))
        {
            if (slot.item != null)
            {
                bool shouldBeDark = soldItemNames.Contains(slot.item.itemName)
                                    || PlayerAlreadyOwns(slot.item);
                slot.SetDarkened(shouldBeDark);
            }
        }
    }

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

            // after setup, auto-darken if we already bought it or already own it
            bool alreadyBoughtBefore = soldItemNames.Contains(itemsForSale[i].itemName);
            bool alreadyOwnNow = PlayerAlreadyOwns(itemsForSale[i]);
            if (alreadyBoughtBefore || alreadyOwnNow)
            {
                ui.SetDarkened(true);
            }
        }
    }

    // Check if the player already has this item in their inventories/progression
    private bool PlayerAlreadyOwns(DjimatItem item)
    {
        if (item == null) return false;

        switch (item.itemType)
        {
            case DjimatItem.ItemType.Djimat:
                // Check equipped slots and backpack slots in GridMaker
                if (gridMaker != null)
                {
                    // equipped
                    foreach (var eq in gridMaker.equippedGridParent
                                      .GetComponentsInChildren<EquippedSlotUI>(true))
                    {
                        if (eq.equippedDjimat == item) return true;
                        // fallback by name if you sometimes clone ScriptableObjects
                        if (eq.equippedDjimat != null &&
                            eq.equippedDjimat.itemName == item.itemName) return true;
                    }

                    // inventory
                    foreach (var inv in gridMaker.inventoryGridParent
                                       .GetComponentsInChildren<InventorySlotUI>(true))
                    {
                        if (inv.assignedDjimat == item) return true;
                        if (inv.assignedDjimat != null &&
                            inv.assignedDjimat.itemName == item.itemName) return true;
                    }
                }
                return false;

            case DjimatItem.ItemType.SacredStone:
                if (sacredStoneGridMaker != null &&
                    sacredStoneGridMaker.stoneInventory != null)
                {
                    foreach (var stone in sacredStoneGridMaker.stoneInventory.stones)
                    {
                        if (stone == item) return true;
                        if (stone != null && stone.itemName == item.itemName) return true;
                    }
                }
                return false;

            case DjimatItem.ItemType.KeyItem:
                if (keyItemGridMaker != null &&
                    keyItemGridMaker.keyItemInventory != null)
                {
                    foreach (var key in keyItemGridMaker.keyItemInventory.keyItems)
                    {
                        if (key == item) return true;
                        if (key != null && key.itemName == item.itemName) return true;
                    }
                }
                return false;

            case DjimatItem.ItemType.DiamondSlot:
                // For diamond slot upgrades, treat as one-time purchase.
                // If we ever bought it (in soldItemNames), we won't let you again.
                return soldItemNames.Contains(item.itemName);

            default:
                return false;
        }
    }

    public void TryPurchase(DjimatItem item, int price, MerchantSlotUI slotUI)
    {
        if (playerManager == null) return;
        if (item == null) return;

        // Guard: can't buy if it's already darkened or already owned
        if (slotUI != null && slotUI.isDarkened)
        {
            Debug.Log("Item already purchased / locked.");
            return;
        }
        if (PlayerAlreadyOwns(item))
        {
            Debug.Log("Player already owns this item. Purchase blocked.");
            if (slotUI != null)
                slotUI.SetDarkened(true); // visually lock it now
            soldItemNames.Add(item.itemName);
            return;
        }

        // Check money
        if (playerManager.money < price)
        {
            Debug.Log("❌ Not enough money!");
            return;
        }

        // Deduct money
        playerManager.money -= price;

        // Give item / apply effect
        switch (item.itemType)
        {
            case DjimatItem.ItemType.Djimat:
                gridMaker?.AddToInventory(item);
                break;

            case DjimatItem.ItemType.SacredStone:
                sacredStoneGridMaker?.AddToInventory(item);
                break;

            case DjimatItem.ItemType.KeyItem:
                keyItemGridMaker?.AddToInventory(item);
                break;

            case DjimatItem.ItemType.DiamondSlot:
                playerManager.slotMax += item.plusslotCost;
                if (limitUI != null)
                {
                    // redraw diamonds after buying +slot
                    limitUI.GenerateSlots(playerManager.slotMax);
                }
                break;
        }

        // Mark sold in memory so it persists across save
        soldItemNames.Add(item.itemName);

        // Mark sold in UI so you can't rebuy
        if (slotUI != null)
            slotUI.SetDarkened(true);

        Debug.Log($"[Merchant] Sold {item.itemName} for {price}");
    }
}
