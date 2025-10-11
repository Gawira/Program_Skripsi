using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class SacredStoneSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public Image iconImage;
    public Image highlight;

    [Header("Assigned Sacred Stone")]
    public DjimatItem assignedStone;  // Your ScriptableObject

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

        if (assignedStone != null)
        {
            ItemInfoPanel.ShowInfo(assignedStone);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlight != null) highlight.enabled = false;
        ItemInfoPanel.ClearInfo();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // optional: can implement using logic here if needed
    }

    public void AssignStone(DjimatItem stone)
    {
        assignedStone = stone;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (assignedStone != null && iconImage != null)
        {
            iconImage.sprite = assignedStone.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }
}
