using UnityEngine;
using System.Collections.Generic;

public class MerchantCatalog : MonoBehaviour
{
    // === NEW: explicit result codes so UI can play different SFX ===
    public enum PurchaseResult
    {
        Success,
        NotEnoughMoney,
        AlreadyPurchased, // slot darkened or flagged sold
        AlreadyOwned,     // player already has it
        DiamondCapReached,
        Invalid
    }

    [Header("References")]
    public GameObject merchantSlotPrefab;
    public Transform gridParent;

    [Header("Merchant Inventory")]
    public DjimatItem[] itemsForSale;
    public int[] itemPrices;

    [Header("Diamond Slot Limit")]
    [Tooltip("Maximum Diamond Slot purchases allowed in this catalog instance.")]
    public int diamondSlotMaxPerCatalog = 4;

    [SerializeField, Tooltip("Runtime count of Diamond Slots bought in this catalog.")]
    private int diamondSlotsBoughtCount = 0;

    private PlayerManager playerManager;
    private GridMaker gridMaker;
    private SacredStoneGridMaker sacredStoneGridMaker;
    private KeyItemGridMaker keyItemGridMaker;
    private DjimatLimitUI limitUI;

    private HashSet<string> soldItemNames = new HashSet<string>();

    public IEnumerable<string> GetSoldOutItemNames() => soldItemNames;

    public void ApplySoldOutFromSave(List<string> savedSoldList)
    {
        soldItemNames.Clear();
        foreach (var id in savedSoldList) soldItemNames.Add(id);

        foreach (var slot in GetComponentsInChildren<MerchantSlotUI>(true))
        {
            if (slot.item != null)
            {
                bool shouldBeDark = soldItemNames.Contains(slot.item.itemName) || PlayerAlreadyOwns(slot.item);
                slot.SetDarkened(shouldBeDark);
            }
        }
        RecountDiamondSlotPurchases();
        EnforceDiamondSlotCapIfNeeded();
    }

    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        gridMaker = FindObjectOfType<GridMaker>();
        sacredStoneGridMaker = FindObjectOfType<SacredStoneGridMaker>();
        keyItemGridMaker = FindObjectOfType<KeyItemGridMaker>();
        limitUI = FindObjectOfType<DjimatLimitUI>();

        BuildCatalog();
        RecountDiamondSlotPurchases();
        EnforceDiamondSlotCapIfNeeded();
    }

    void BuildCatalog()
    {
        for (int i = 0; i < itemsForSale.Length; i++)
        {
            GameObject go = Instantiate(merchantSlotPrefab, gridParent);
            MerchantSlotUI ui = go.GetComponent<MerchantSlotUI>();
            ui.Setup(itemsForSale[i], itemPrices[i], this);

            bool alreadyBoughtBefore = soldItemNames.Contains(itemsForSale[i].itemName);
            bool alreadyOwnNow = PlayerAlreadyOwns(itemsForSale[i]);
            if (alreadyBoughtBefore || alreadyOwnNow)
                ui.SetDarkened(true);
        }
    }

    private bool PlayerAlreadyOwns(DjimatItem item)
    {
        if (item == null) return false;

        switch (item.itemType)
        {
            case DjimatItem.ItemType.Djimat:
                if (gridMaker != null)
                {
                    foreach (var eq in gridMaker.equippedGridParent.GetComponentsInChildren<EquippedSlotUI>(true))
                        if (eq.equippedDjimat != null && eq.equippedDjimat.itemName == item.itemName) return true;

                    foreach (var inv in gridMaker.inventoryGridParent.GetComponentsInChildren<InventorySlotUI>(true))
                        if (inv.assignedDjimat != null && inv.assignedDjimat.itemName == item.itemName) return true;
                }
                return false;

            case DjimatItem.ItemType.SacredStone:
                if (sacredStoneGridMaker != null && sacredStoneGridMaker.stoneInventory != null)
                    foreach (var stone in sacredStoneGridMaker.stoneInventory.stones)
                        if (stone != null && stone.itemName == item.itemName) return true;
                return false;

            case DjimatItem.ItemType.KeyItem:
                if (keyItemGridMaker != null && keyItemGridMaker.keyItemInventory != null)
                    foreach (var key in keyItemGridMaker.keyItemInventory.keyItems)
                        if (key != null && key.itemName == item.itemName) return true;
                return false;

            case DjimatItem.ItemType.DiamondSlot:
                return soldItemNames.Contains(item.itemName);

            default:
                return false;
        }
    }

    // === NEW: returns a result so UI can decide which SFX to play ===
    public PurchaseResult TryPurchaseWithResult(DjimatItem item, int price, MerchantSlotUI slotUI)
    {
        if (playerManager == null || item == null) return PurchaseResult.Invalid;

        // Diamond cap
        if (item.itemType == DjimatItem.ItemType.DiamondSlot &&
            diamondSlotsBoughtCount >= diamondSlotMaxPerCatalog)
        {
            if (slotUI != null) slotUI.SetDarkened(true);
            return PurchaseResult.DiamondCapReached;
        }

        // Already locked/darkened or already own it
        if (slotUI != null && slotUI.isDarkened) return PurchaseResult.AlreadyPurchased;
        if (PlayerAlreadyOwns(item))
        {
            if (slotUI != null) slotUI.SetDarkened(true);
            soldItemNames.Add(item.itemName);
            return PurchaseResult.AlreadyOwned;
        }

        // Money check
        if (playerManager.money < price) return PurchaseResult.NotEnoughMoney;

        // Pay
        playerManager.money -= price;

        // Grant item / effect
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
                diamondSlotsBoughtCount++;
                if (limitUI != null) limitUI.GenerateSlots(playerManager.slotMax);
                break;
        }

        soldItemNames.Add(item.itemName);
        if (slotUI != null) slotUI.SetDarkened(true);

        if (diamondSlotsBoughtCount >= diamondSlotMaxPerCatalog)
            EnforceDiamondSlotCapIfNeeded();

        Debug.Log($"[Merchant] Sold {item.itemName} for {price}");
        return PurchaseResult.Success;
    }

    // Back-compat wrapper (if any other code still calls old signature)
    public void TryPurchase(DjimatItem item, int price, MerchantSlotUI slotUI)
    {
        TryPurchaseWithResult(item, price, slotUI);
    }

    // ===== Helpers for the Diamond Slot cap =====
    private void RecountDiamondSlotPurchases()
    {
        int count = 0;
        foreach (var slot in GetComponentsInChildren<MerchantSlotUI>(true))
        {
            if (slot.item != null && slot.item.itemType == DjimatItem.ItemType.DiamondSlot)
            {
                if (slot.isDarkened ||
                    soldItemNames.Contains(slot.item.itemName) ||
                    PlayerAlreadyOwns(slot.item))
                {
                    count++;
                }
            }
        }
        diamondSlotsBoughtCount = count;
    }

    private void EnforceDiamondSlotCapIfNeeded()
    {
        if (diamondSlotsBoughtCount < diamondSlotMaxPerCatalog) return;

        foreach (var slot in GetComponentsInChildren<MerchantSlotUI>(true))
        {
            if (slot.item != null && slot.item.itemType == DjimatItem.ItemType.DiamondSlot)
                slot.SetDarkened(true);
        }
        Debug.Log($"[Merchant] Diamond Slot limit reached ({diamondSlotsBoughtCount}/{diamondSlotMaxPerCatalog}) — further purchases disabled in this shop.");
    }
}
