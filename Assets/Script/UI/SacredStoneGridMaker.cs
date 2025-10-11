using UnityEngine;
using UnityEngine.UI;

public class SacredStoneGridMaker : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject stoneSlotPrefab;
    public Transform stoneGridParent;
    public DjimatItem[] startingStones; // assign in inspector

    [Header("Grid Layout")]
    public Vector2Int gridSize = new Vector2Int(1, 4);

    void Start()
    {
        SetupGridLayout(stoneGridParent.GetComponent<GridLayoutGroup>(), gridSize);
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
            GameObject go = Instantiate(stoneSlotPrefab, stoneGridParent);
            SacredStoneSlotUI ui = go.GetComponent<SacredStoneSlotUI>();

            if (startingStones != null && itemIndex < startingStones.Length)
            {
                ui.AssignStone(startingStones[itemIndex]);
                itemIndex++;
            }
            else
            {
                ui.AssignStone(null);
            }
        }
    }

    public void AddToInventory(DjimatItem item)
    {
        foreach (var slot in stoneGridParent.GetComponentsInChildren<SacredStoneSlotUI>())
        {
            if (slot.assignedStone == null)
            {
                slot.AssignStone(item);
                return;
            }
        }

        Debug.LogWarning("No empty Sacred Stone slot available!");
    }
}
