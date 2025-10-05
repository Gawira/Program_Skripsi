using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DjimatUIManager : MonoBehaviour
{
    [Header("References")]
    public DjimatManager djimatManager;

    [Header("Prefabs & UI Parents")]
    public Transform equippedParent;        // Parent that holds EquippedDjimatSlot1–4
    public Transform inventoryParent;       // Grid parent for inventory
    public GameObject slotPrefab;           // Only used for inventory
    public List<Image> diamondIcons = new List<Image>();

    // internal references
    private List<SimpleSlotUI> equippedUI = new List<SimpleSlotUI>();
    private List<GameObject> inventoryUI = new List<GameObject>();

    private int selectedEquippedIndex = -1;

    void Start()
    {
        if (djimatManager == null)
            djimatManager = FindObjectOfType<DjimatManager>();

        if (djimatManager == null)
        {
            Debug.LogError("DjimatManager not found!");
            return;
        }

        // Instead of instantiating, grab already-existing equipped slots
        BuildEquippedUI();

        RefreshAll();
        djimatManager.OnChanged += RefreshAll;
    }

    void OnDestroy()
    {
        if (djimatManager != null) djimatManager.OnChanged -= RefreshAll;
    }

    void BuildEquippedUI()
    {
        equippedUI.Clear();

        // Loop through children of equippedParent
        for (int i = 0; i < equippedParent.childCount; i++)
        {
            var child = equippedParent.GetChild(i);
            var ui = child.GetComponent<SimpleSlotUI>();

            if (ui != null)
            {
                int index = i;
                ui.button.onClick.RemoveAllListeners();
                ui.button.onClick.AddListener(() => OnEquippedSlotClicked(index));
                equippedUI.Add(ui);
            }
        }
    }

    public void RefreshAll()
    {
        RefreshEquippedUI();
        RefreshInventoryUI();
        RefreshDiamonds();
        ClearSelection();
    }

    void RefreshEquippedUI()
    {
        for (int i = 0; i < equippedUI.Count; i++)
        {
            var ui = equippedUI[i];
            var item = djimatManager.equippedSlots[i];
            ui.UpdateIcon(item);
            ui.SetHighlight(selectedEquippedIndex == i);
        }
    }

    void RefreshInventoryUI()
    {
        foreach (var go in inventoryUI) Destroy(go);
        inventoryUI.Clear();

        for (int i = 0; i < djimatManager.inventory.Count; i++)
        {
            var item = djimatManager.inventory[i];
            GameObject go = Instantiate(slotPrefab, inventoryParent);
            var ui = go.GetComponent<SimpleSlotUI>();
            int index = i;
            ui.UpdateIcon(item);
            ui.button.onClick.RemoveAllListeners();
            ui.button.onClick.AddListener(() => OnInventoryItemClicked(index));
            inventoryUI.Add(go);
        }
    }

    void RefreshDiamonds()
    {
        int used = djimatManager.GetCurrentUsedSlots();
        int capacity = djimatManager.SlotCapacity;

        for (int i = 0; i < diamondIcons.Count; i++)
        {
            if (i < capacity)
            {
                diamondIcons[i].gameObject.SetActive(true);
                diamondIcons[i].color = (i < used) ? Color.white : new Color(1, 1, 1, 0.25f);
            }
            else
            {
                diamondIcons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnEquippedSlotClicked(int index)
    {
        if (selectedEquippedIndex == index)
        {
            ClearSelection();
            return;
        }

        selectedEquippedIndex = index;
        RefreshEquippedUI();
        Debug.Log("Selected equip slot " + index);
    }

    public void OnInventoryItemClicked(int inventoryIndex)
    {
        if (selectedEquippedIndex == -1)
        {
            Debug.Log("Select an equipped slot first.");
            return;
        }

        var item = djimatManager.inventory[inventoryIndex];
        if (item == null)
            return;

        bool ok = djimatManager.EquipToSlot(selectedEquippedIndex, item);
        if (!ok)
        {
            Debug.Log("Could not equip item.");
        }
        else
        {
            Debug.Log($"Equipped {item.itemName} into slot {selectedEquippedIndex}");
        }

        RefreshAll();
    }

    void ClearSelection()
    {
        selectedEquippedIndex = -1;
        RefreshEquippedUI();
    }
}
