using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

public class KeyItemGridMaker : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject keyItemSlotPrefab;
    public Transform keyItemGridParent;
    public DjimatItem[] startingKeyItems; // assign in inspector

    [Header("Grid Layout")]
    public Vector2Int gridSize = new Vector2Int(1, 4);

    void Start()
    {
        SetupGridLayout(keyItemGridParent.GetComponent<GridLayoutGroup>(), gridSize);
        BuildGrid();
    }

    void SetupGridLayout(GridLayoutGroup grid, Vector2Int size)
    {
        if (grid == null) return;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = size.y;
    }

    void BuildGrid()
    {
        int totalSlots = gridSize.x * gridSize.y;
        int itemIndex = 0;

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject go = Instantiate(keyItemSlotPrefab, keyItemGridParent);
            KeyItemSlotUI ui = go.GetComponent<KeyItemSlotUI>();

            if (startingKeyItems != null && itemIndex < startingKeyItems.Length)
            {
                ui.AssignKeyItem(startingKeyItems[itemIndex]);
                itemIndex++;
            }
            else
            {
                ui.AssignKeyItem(null);
            }
        }
    }
    public void AddToInventory(DjimatItem item)
    {
        foreach (var slot in keyItemGridParent.GetComponentsInChildren<KeyItemSlotUI>())
        {
            if (slot.assignedKeyItem == null)
            {
                slot.AssignKeyItem(item);
                return;
            }
        }

        Debug.LogWarning("No empty Key Item slot available!");
    }
}
