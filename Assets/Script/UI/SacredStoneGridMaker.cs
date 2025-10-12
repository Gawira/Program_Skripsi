using UnityEngine;
using UnityEngine.UI;

public class SacredStoneGridMaker : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject stoneSlotPrefab;
    public Transform stoneGridParent;
    public SacredStoneInventory stoneInventory; // reference ScriptableObject

    [Header("Grid Layout")]
    public Vector2Int gridSize = new Vector2Int(1, 4);

    private void OnEnable()
    {
        // Subscribe to inventory updates
        if (stoneInventory != null)
            stoneInventory.OnInventoryChanged += RefreshGrid;

        SetupGridLayout(stoneGridParent.GetComponent<GridLayoutGroup>(), gridSize);
        RefreshGrid();
    }

    private void OnDisable()
    {
        if (stoneInventory != null)
            stoneInventory.OnInventoryChanged -= RefreshGrid;
    }

    private void SetupGridLayout(GridLayoutGroup grid, Vector2Int size)
    {
        if (grid == null) return;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = size.y;
    }

    public void RefreshGrid()
    {
        // Clear existing slots
        foreach (Transform child in stoneGridParent)
            Destroy(child.gameObject);

        if (stoneInventory == null) return;

        // Fill UI with current stones in the inventory
        foreach (var stone in stoneInventory.stones)
        {
            GameObject go = Instantiate(stoneSlotPrefab, stoneGridParent);
            SacredStoneSlotUI ui = go.GetComponent<SacredStoneSlotUI>();
            ui.AssignStone(stone);
        }

        // Fill remaining empty slots
        int maxSlots = gridSize.x * gridSize.y;
        int emptySlots = maxSlots - stoneInventory.stones.Count;
        for (int i = 0; i < emptySlots; i++)
        {
            GameObject go = Instantiate(stoneSlotPrefab, stoneGridParent);
            SacredStoneSlotUI ui = go.GetComponent<SacredStoneSlotUI>();
            ui.AssignStone(null);
        }
    }

    public void AddToInventory(DjimatItem item)
    {
        if (stoneInventory == null) return;

        stoneInventory.AddStone(item);  // Update ScriptableObject instead of direct UI
    }

    public void RemoveFromInventory(DjimatItem item)
    {
        if (stoneInventory == null) return;

        stoneInventory.RemoveStone(item);  // Inventory updates will auto-refresh UI
    }

    public bool HasStone(DjimatItem stone)
    {
        foreach (var slot in stoneGridParent.GetComponentsInChildren<SacredStoneSlotUI>())
        {
            if (slot.assignedStone == stone)
                return true;
        }
        return false;
    }

    public void RemoveStone(DjimatItem stone)
    {
        foreach (var slot in stoneGridParent.GetComponentsInChildren<SacredStoneSlotUI>())
        {
            if (slot.assignedStone == stone)
            {
                slot.AssignStone(null);
                return;
            }
        }
    }
}
