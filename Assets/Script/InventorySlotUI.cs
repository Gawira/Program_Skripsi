using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image iconImage;
    public Image highlight;

    [Header("Assigned Djimat")]
    public DjimatItem assignedDjimat;

    private void Start()
    {
        UpdateUI();
        if (highlight != null) highlight.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        EquippedSlotUI selected = EquippedSlotUI.GetSelectedSlot();
        if (selected != null && assignedDjimat != null)
        {
            selected.AssignDjimat(assignedDjimat);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlight != null) highlight.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlight != null) highlight.enabled = false;
    }

    private void UpdateUI()
    {
        if (assignedDjimat != null && iconImage != null)
        {
            iconImage.sprite = assignedDjimat.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }
}
