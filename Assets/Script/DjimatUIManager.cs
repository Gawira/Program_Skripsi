using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DjimatUIManager : MonoBehaviour
{
    [Header("References")]
    public DjimatManager djimatManager;

    [Header("Prefabs & UI Parents")]
    public GameObject slotPrefab;           // prefab with SimpleSlotUI + Button
    public Transform equippedParent;        // red block container (one child per equipped slot)
    public Transform inventoryParent;       // blue block container (grid for inventory)
    public List<Image> diamondIcons = new List<Image>(); // orange diamonds to show capacity usage

    // UI internal lists
    private List<GameObject> equippedUI = new List<GameObject>();
    private List<GameObject> inventoryUI = new List<GameObject>();

    // selection state
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

        // Create UI for equipped slots
        BuildEquippedUI();

        // initial populate inventory UI
        RefreshAll();

        // subscribe to changes
        djimatManager.OnChanged += RefreshAll;
    }

    void OnDestroy()
    {
        if (djimatManager != null) djimatManager.OnChanged -= RefreshAll;
    }

    // Create fixed number of equipped slot UI elements once
    void BuildEquippedUI()
    {
        // clean
        foreach (Transform t in equippedParent) Destroy(t.gameObject);
        equippedUI.Clear();

        int slots = djimatManager.equippedSlots.Length;
        for (int i = 0; i < slots; i++)
        {
            GameObject go = Instantiate(slotPrefab, equippedParent);
            SimpleSlotUI ui = go.GetComponent<SimpleSlotUI>();
            int index = i;
            ui.button.onClick.RemoveAllListeners();
            ui.button.onClick.AddListener(() => OnEquippedSlotClicked(index));
            equippedUI.Add(go);
        }
    }

    // Refresh whole UI
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
            SimpleSlotUI ui = equippedUI[i].GetComponent<SimpleSlotUI>();
            var item = djimatManager.equippedSlots[i];
            ui.UpdateIcon(item);
            ui.SetHighlight(selectedEquippedIndex == i);
            // optionally set tooltip/label
        }
    }

    void RefreshInventoryUI()
    {
        // destroy old children
        foreach (var go in inventoryUI) Destroy(go);
        inventoryUI.Clear();

        for (int i = 0; i < djimatManager.inventory.Count; i++)
        {
            var item = djimatManager.inventory[i];
            GameObject go = Instantiate(slotPrefab, inventoryParent);
            SimpleSlotUI ui = go.GetComponent<SimpleSlotUI>();
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
                diamondIcons[i].color = (i < used) ? Color.white : new Color(1, 1, 1, 0.25f); // filled vs empty look
            }
            else
            {
                diamondIcons[i].gameObject.SetActive(false);
            }
        }
    }

    // ======= Interaction (two-click) =======
    public void OnEquippedSlotClicked(int index)
    {
        // If player clicked same selected slot -> deselect (or unequip if occupied)
        if (selectedEquippedIndex == index)
        {
            // If you want clicking same slot to unequip, uncomment next line:
            // djimatManager.UnequipSlot(index);
            ClearSelection();
            return;
        }

        // select target equipped slot
        selectedEquippedIndex = index;
        RefreshEquippedUI();
        // Optionally give hint to player: "Now click a djimat in inventory to place here."
        Debug.Log("Selected equip slot " + index + ". Now click an inventory item to place.");
    }

    public void OnInventoryItemClicked(int inventoryIndex)
    {
        if (selectedEquippedIndex == -1)
        {
            // no target selected — optionally show preview or select the item
            Debug.Log("Select an equipped slot first (click a red slot), then pick an item.");
            return;
        }

        var item = djimatManager.inventory[inventoryIndex];
        if (item == null)
        {
            Debug.Log("Invalid inventory item");
            return;
        }

        bool ok = djimatManager.EquipToSlot(selectedEquippedIndex, item);
        if (!ok)
        {
            Debug.Log("Could not equip item (not enough capacity?)");
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
