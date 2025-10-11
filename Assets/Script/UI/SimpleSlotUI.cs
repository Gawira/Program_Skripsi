using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleSlotUI : MonoBehaviour
{
    public Image iconImage;                 // assign in prefab
    public TextMeshProUGUI nameText;        // optional
    public Button button;                   // assign in prefab
    public GameObject highlight;            // optional highlight visual

    // internal state
    private DjimatItem assignedDjimat;
    private int slotIndex;
    private DjimatUIManager uiManager;
    private SlotType slotType;

    public enum SlotType { Equipped, Inventory }

    // Call this from DjimatUIManager when creating/refreshing slots
    public void Setup(DjimatItem dj, int index, DjimatUIManager manager, SlotType type)
    {
        assignedDjimat = dj;
        slotIndex = index;
        uiManager = manager;
        slotType = type;

        // set visuals
        if (iconImage != null)
        {
            iconImage.sprite = dj != null ? dj.icon : null;
            iconImage.enabled = dj != null;
        }

        if (nameText != null)
            nameText.text = dj != null ? dj.itemName : "";

        // wire button to this OnClick (removes old listeners first)
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    // Called when the player clicks this UI slot
    public void OnClick()
    {
        if (uiManager == null) return;

        if (slotType == SlotType.Equipped)
            uiManager.OnEquippedSlotClicked(slotIndex);
        else // Inventory
            uiManager.OnInventoryItemClicked(slotIndex);
    }

    // small helpers
    public void SetHighlight(bool on)
    {
        if (highlight != null) highlight.SetActive(on);
    }

    // update icons without recreating slot instances
    public void UpdateIcon(DjimatItem dj)
    {
        assignedDjimat = dj;
        if (iconImage != null)
        {
            iconImage.sprite = dj != null ? dj.icon : null;
            iconImage.enabled = dj != null;
        }
        if (nameText != null)
            nameText.text = dj != null ? dj.itemName : "";
    }
}
