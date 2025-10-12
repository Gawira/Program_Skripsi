using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Sacred Stone Inventory")]
public class SacredStoneInventory : ScriptableObject
{
    public List<DjimatItem> stones = new List<DjimatItem>();

    public event System.Action OnInventoryChanged;

    public void AddStone(DjimatItem stone)
    {
        stones.Add(stone);
        OnInventoryChanged?.Invoke();
    }

    public void RemoveStone(DjimatItem stone)
    {
        stones.Remove(stone);
        OnInventoryChanged?.Invoke();
    }
}
