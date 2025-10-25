using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MerchantSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Image highlight;

    private DjimatItem itemData;
    private int itemPrice;
    private MerchantCatalog merchantCatalog;

    private ItemInfoDisplayCatalog infoDisplay;

    public DjimatItem item => itemData;
    public bool isDarkened { get; private set; } = false;

    private void Start()
    {
        infoDisplay = FindObjectOfType<ItemInfoDisplayCatalog>();
    }

    public void Setup(DjimatItem item, int price, MerchantCatalog catalog)
    {
        itemData = item;
        itemPrice = price;
        merchantCatalog = catalog;

        if (iconImage) iconImage.sprite = item.icon;
        if (nameText) nameText.text = item.itemName;
        if (priceText) priceText.text = price.ToString();
        if (highlight) highlight.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlight) highlight.enabled = true;
        if (infoDisplay != null && itemData != null)
            infoDisplay.ShowInfo(itemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlight) highlight.enabled = false;
        if (infoDisplay != null)
            infoDisplay.ClearInfo();
    }

    public void OnClickBuy()
    {
        // Don't allow buying if already darkened
        if (isDarkened)
        {
            Debug.Log("This item is already purchased / locked.");
            return;
        }

        merchantCatalog.TryPurchase(itemData, itemPrice, this);
        Debug.Log("Bought");
    }

    public void SetDarkened(bool state)
    {
        isDarkened = state;

        if (iconImage != null)
            iconImage.color = state ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;

        // Disable the button so it can't be clicked anymore
        var btn = GetComponent<Button>();
        if (btn != null)
            btn.interactable = !state;
    }
}
