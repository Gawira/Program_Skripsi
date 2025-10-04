using UnityEngine;
using UnityEngine.UI;

public class GridMaker : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject equippedSlotPrefab;
    public GameObject inventorySlotPrefab;

    [Header("Parents (must have GridLayoutGroup)")]
    public Transform equippedGridParent;
    public Transform inventoryGridParent;

    [Header("Grid Settings")]
    public Vector2Int equippedGridSize = new Vector2Int(1, 4); // rows, columns
    public Vector2Int inventoryGridSize = new Vector2Int(3, 4); // rows, columns

    [Header("Inventory Data")]
    public DjimatItem[] startingInventory;

    private InventorySlotUI[] inventorySlots;

    void Start()
    {
        SetupGridLayout(equippedGridParent.GetComponent<GridLayoutGroup>(), equippedGridSize);
        SetupGridLayout(inventoryGridParent.GetComponent<GridLayoutGroup>(), inventoryGridSize);

        BuildEquippedGrid();
        BuildInventoryGrid();
    }

    void SetupGridLayout(GridLayoutGroup grid, Vector2Int size)
    {
        if (grid == null) return;

        // Force columns layout
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = size.y; // columns
    }

    void BuildEquippedGrid()
    {
        int totalSlots = equippedGridSize.x * equippedGridSize.y;

        for (int i = 0; i < totalSlots; i++)
        {
            Instantiate(equippedSlotPrefab, equippedGridParent);
        }
    }

    void BuildInventoryGrid()
    {
        int totalSlots = inventoryGridSize.x * inventoryGridSize.y;
        int itemIndex = 0;

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject go = Instantiate(inventorySlotPrefab, inventoryGridParent);
            InventorySlotUI ui = go.GetComponent<InventorySlotUI>();

            if (startingInventory != null && itemIndex < startingInventory.Length)
            {
                ui.assignedDjimat = startingInventory[itemIndex];
                itemIndex++;
            }
            else
            {
                ui.assignedDjimat = null; // empty slot
            }
        }
    }

    public void AddToInventory(DjimatItem item)
    {
        // Find first empty slot
        foreach (var slot in inventoryGridParent.GetComponentsInChildren<InventorySlotUI>())
        {
            if (slot.assignedDjimat == null)
            {
                slot.assignedDjimat = item;
                slot.UpdateUI();
                return;
            }
        }

        Debug.LogWarning("Inventory full! Could not add Djimat back.");
    }
}
