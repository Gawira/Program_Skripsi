using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMaker : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject equippedSlotPrefab;
    public GameObject inventorySlotPrefab;

    [Header("Parents")]
    public Transform equippedGridParent;
    public Transform inventoryGridParent;

    [Header("Inventory Data")]
    public DjimatItem[] startingInventory;
    public int equippedSlotsCount = 4;

    void Start()
    {
        BuildEquippedGrid();
        BuildInventoryGrid();
    }

    void BuildEquippedGrid()
    {
        for (int i = 0; i < equippedSlotsCount; i++)
        {
            Instantiate(equippedSlotPrefab, equippedGridParent);
        }
    }

    void BuildInventoryGrid()
    {
        foreach (var item in startingInventory)
        {
            GameObject go = Instantiate(inventorySlotPrefab, inventoryGridParent);
            InventorySlotUI ui = go.GetComponent<InventorySlotUI>();
            ui.assignedDjimat = item;
        }
    }
}