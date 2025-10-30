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

    [Header("Audio")]
    [Tooltip("Played when mouse cursor enters this slot.")]
    public AudioClip hoverSFX;
    [Tooltip("Played when clicking/selecting/deselecting this slot.")]
    public AudioClip clickSFX;
    [Tooltip("Avoid hover spam when UI re-fires enter events (in seconds).")]
    public float hoverCooldown = 0.05f;
    [Tooltip("If false, hover sfx only plays when the slot has an item.")]
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
        infoDisplay = FindObjectOfType<ItemInfoDisplay>(); // auto find
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // SFX
        PlayClickSFX();

        if (selectedSlot == this)
        {
            // Deselect if clicked again
            selectedSlot = null;
            if (highlight != null) highlight.enabled = false;
        }
        else
        {
            // Deselect previous
            if (selectedSlot != null && selectedSlot.highlight != null)
                selectedSlot.highlight.enabled = false;

            // Select this
            selectedSlot = this;
            if (highlight != null) highlight.enabled = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlight != null && selectedSlot != this)
            highlight.enabled = true;

        if (infoDisplay != null && equippedDjimat != null)
            infoDisplay.ShowInfo(equippedDjimat);

        // SFX (with cooldown + optional “only when has item”)
        if (Time.unscaledTime - _lastHoverTime >= hoverCooldown)
        {
            if (playHoverWhenEmpty || equippedDjimat != null)
                PlayHoverSFX();

            _lastHoverTime = Time.unscaledTime;
        }
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

    // ---------- Audio helpers ----------
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
