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

    [Header("Audio")]
    [Tooltip("Played when mouse cursor enters this slot.")]
    public AudioClip hoverSFX;
    [Tooltip("Played when buying / clicking this slot.")]
    public AudioClip clickSFX;
    [Tooltip("Avoid hover spam when UI re-fires enter events (seconds).")]
    public float hoverCooldown = 0.05f;
    [Tooltip("If false, hover SFX will not play when the item is already sold/darkened.")]
    public bool playHoverWhenDarkened = false;

    private float _lastHoverTime = -999f;

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

        if (Time.unscaledTime - _lastHoverTime >= hoverCooldown)
        {
            if (playHoverWhenDarkened || !isDarkened)
                PlayHoverSFX();

            _lastHoverTime = Time.unscaledTime;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlight) highlight.enabled = false;

        if (infoDisplay != null)
            infoDisplay.ClearInfo();
    }

    public void OnClickBuy()
    {
        // Click sound first (feels snappier)
        PlayClickSFX();

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

    // --- Audio helpers ---
    private void PlayHoverSFX()
    {
        if (hoverSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(hoverSFX); // 2D UI SFX
    }

    private void PlayClickSFX()
    {
        if (clickSFX != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clickSFX); // 2D UI SFX
    }
}
