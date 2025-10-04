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
    private DjimatSystem djimatSystem;

    private void Start()
    {
        UpdateUI();
        if (highlight != null) highlight.enabled = false;
    }

    private void Awake()
    {
        if (djimatSystem == null)
            djimatSystem = FindObjectOfType<DjimatSystem>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        EquippedSlotUI selected = EquippedSlotUI.GetSelectedSlot();
        if (selected != null)
        {
            // CASE 1: Inventory slot has a Djimat -> Try to equip it
            if (assignedDjimat != null)
            {
                DjimatItem itemToEquip = assignedDjimat;
                DjimatItem previous = selected.equippedDjimat;

                // Ask system to handle equip logic
                if (djimatSystem.EquipToSlot(selected, itemToEquip))
                {
                    // If equip succeeded, clear this inventory slot
                    assignedDjimat = null;
                    UpdateUI();

                    // If there was a previous Djimat in this slot, return it to inventory
                    if (previous != null)
                    {
                        GridMaker grid = FindObjectOfType<GridMaker>();
                        if (grid != null)
                            grid.AddToInventory(previous);
                    }
                }
            }
            else
            {
                // CASE 2: Empty inventory slot clicked -> Unequip from selected slot into here
                if (selected.equippedDjimat != null)
                {
                    DjimatItem unequipped = selected.equippedDjimat;

                    // Request unequip
                    djimatSystem.UnequipSlot(selected);

                    // Place into this empty inventory slot
                    assignedDjimat = unequipped;
                    UpdateUI();
                }
            }
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

    public void UpdateUI()
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
