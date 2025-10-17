using UnityEngine;
using UnityEngine.UI;

public class KeyItemGridMaker : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject keyItemSlotPrefab;
    public Transform keyItemGridParent;
    public KeyItemInventory keyItemInventory;   // Reference to ScriptableObject

    [Header("Grid Layout")]
    public Vector2Int gridSize = new Vector2Int(1, 4);

    private void OnEnable()
    {
        // Subscribe to inventory updates
        if (keyItemInventory != null)
            keyItemInventory.OnInventoryChanged += RefreshGrid;

        SetupGridLayout(keyItemGridParent.GetComponent<GridLayoutGroup>(), gridSize);
        RefreshGrid();
    }

    private void OnDisable()
    {
        if (keyItemInventory != null)
            keyItemInventory.OnInventoryChanged -= RefreshGrid;
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
        foreach (Transform child in keyItemGridParent)
            Destroy(child.gameObject);

        if (keyItemInventory == null) return;

        // Fill UI with current key items in inventory
        foreach (var key in keyItemInventory.keyItems)
        {
            GameObject go = Instantiate(keyItemSlotPrefab, keyItemGridParent);
            KeyItemSlotUI ui = go.GetComponent<KeyItemSlotUI>();
            ui.AssignKeyItem(key);
        }

        // Fill remaining empty slots
        int maxSlots = gridSize.x * gridSize.y;
        int emptySlots = maxSlots - keyItemInventory.keyItems.Count;
        for (int i = 0; i < emptySlots; i++)
        {
            GameObject go = Instantiate(keyItemSlotPrefab, keyItemGridParent);
            KeyItemSlotUI ui = go.GetComponent<KeyItemSlotUI>();
            ui.AssignKeyItem(null);
        }
    }

    public void AddToInventory(DjimatItem item)
    {
        if (keyItemInventory == null) return;
        keyItemInventory.AddKeyItem(item);
    }

    public void RemoveFromInventory(DjimatItem item)
    {
        if (keyItemInventory == null) return;
        keyItemInventory.RemoveKeyItem(item);
    }

    public bool HasKey(DjimatItem key)
    {
        return keyItemInventory != null && keyItemInventory.HasKeyItem(key);
    }

    public void RemoveKey(DjimatItem key)
    {
        if (keyItemInventory == null) return;
        keyItemInventory.RemoveKeyItem(key);
    }
}
