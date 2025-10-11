using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquippedSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image iconImage;
    public Image highlight;

    [Header("Assigned Djimat")]
    public DjimatItem equippedDjimat;
    private DjimatSystem djimatSystem;

    private static EquippedSlotUI selectedSlot;
    private ItemInfoDisplay infoDisplay;

    private void Start()
    {
        UpdateUI();
        if (highlight != null) highlight.enabled = false;
        infoDisplay = FindObjectOfType<ItemInfoDisplay>(); // auto find
    }

    private void Awake()
    {
        if (djimatSystem == null)
            djimatSystem = FindObjectOfType<DjimatSystem>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selectedSlot == this)
        {
            // Deselect if clicked again
            selectedSlot = null;
            if (highlight != null) highlight.enabled = false;
            
        }
        else
        {
            // Select this slot
            if (selectedSlot != null && selectedSlot.highlight != null)
                selectedSlot.highlight.enabled = false;

            selectedSlot = this;
            if (highlight != null) highlight.enabled = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlight != null && selectedSlot != this)
            highlight.enabled = true;
 
        if (infoDisplay != null && equippedDjimat != null)
            infoDisplay.ShowInfo(equippedDjimat); //ngasih tauin panel info

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlight != null && selectedSlot != this)
            highlight.enabled = false;
        if (infoDisplay != null)
            infoDisplay.ClearInfo();
    }

    public void AssignDjimat(DjimatItem newDjimat)
    {
        equippedDjimat = newDjimat;
        UpdateUI();

        // After assignment, deselect
        if (selectedSlot == this)
        {
            selectedSlot = null;
            if (highlight != null) highlight.enabled = false;
        }
    }

    private void UpdateUI()
    {
        if (equippedDjimat != null && iconImage != null)
        {
            iconImage.sprite = equippedDjimat.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    public static EquippedSlotUI GetSelectedSlot()
    {
        return selectedSlot;
    }
}
