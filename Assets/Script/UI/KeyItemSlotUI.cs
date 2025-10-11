using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public Image iconImage;
    public Image highlight;

    [Header("Assigned Key Item")]
    public DjimatItem assignedKeyItem;  // ScriptableObject

    private ItemInfoDisplay ItemInfoPanel;

    private void Start()
    {
        UpdateUI();
        if (highlight != null) highlight.enabled = false;
        ItemInfoPanel = FindObjectOfType<ItemInfoDisplay>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlight != null) highlight.enabled = true;

        if (assignedKeyItem != null)
        {
            ItemInfoPanel.ShowInfo(assignedKeyItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlight != null) highlight.enabled = false;
        ItemInfoPanel.ClearInfo();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // optional: implement behavior if you want to use key items
    }

    public void AssignKeyItem(DjimatItem keyItem)
    {
        assignedKeyItem = keyItem;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (assignedKeyItem != null && iconImage != null)
        {
            iconImage.sprite = assignedKeyItem.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }
}
