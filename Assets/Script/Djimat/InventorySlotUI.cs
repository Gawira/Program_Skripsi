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

    private ItemInfoDisplay infoDisplay;

    [Header("Audio")]
    [Tooltip("Played when mouse cursor enters this slot.")]
    public AudioClip hoverSFX;
    [Tooltip("Played when clicking this slot.")]
    public AudioClip clickSFX;
    [Tooltip("Avoid hover spam when UI re-fires enter events (seconds).")]
    public float hoverCooldown = 0.05f;
    [Tooltip("If false, hover SFX plays only when slot has an item.")]
    public bool playHoverWhenEmpty = true;

    private float _lastHoverTime = -999f;

    private void Awake()
    {
        if (djimatSystem == null)
            djimatSystem = FindObjectOfType<DjimatSystem>();
    }

    private void Start()
    {
        UpdateUI();
        if (highlight != null) highlight.enabled = false;
        infoDisplay = FindObjectOfType<ItemInfoDisplay>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClickSFX();

        EquippedSlotUI selected = EquippedSlotUI.GetSelectedSlot();
        if (selected != null)
        {
            // CASE 1: Inventory slot has a Djimat -> Try to equip it
            if (assignedDjimat != null)
            {
                DjimatItem itemToEquip = assignedDjimat;
                DjimatItem previous = selected.equippedDjimat;

                if (djimatSystem.EquipToSlot(selected, itemToEquip))
                {
                    // Clear this inventory slot
                    assignedDjimat = null;
                    UpdateUI();

                    // Return previously equipped, if any, to inventory
                    if (previous != null)
                    {
                        GridMaker grid = FindObjectOfType<GridMaker>();
                        if (grid != null)
                            grid.AddToInventory(previous);
                    }
                }
            }
            // CASE 2: Empty inventory slot clicked -> Unequip from selected slot into here
            else
            {
                if (selected.equippedDjimat != null)
                {
                    DjimatItem unequipped = selected.equippedDjimat;

                    djimatSystem.UnequipSlot(selected);

                    assignedDjimat = unequipped;
                    UpdateUI();
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlight != null) highlight.enabled = true;

        if (infoDisplay != null && assignedDjimat != null)
            infoDisplay.ShowInfo(assignedDjimat);

        if (Time.unscaledTime - _lastHoverTime >= hoverCooldown)
        {
            if (playHoverWhenEmpty || assignedDjimat != null)
                PlayHoverSFX();

            _lastHoverTime = Time.unscaledTime;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlight != null) highlight.enabled = false;

        if (infoDisplay != null)
            infoDisplay.ClearInfo();
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

    public void AssignDjimat(DjimatItem item)
    {
        assignedDjimat = item;
        UpdateUI();
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
