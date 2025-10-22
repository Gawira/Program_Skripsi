using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDjimat", menuName = "Inventory/Djimat")]
public class DjimatItem : ScriptableObject
{
    public enum ItemType
    {
        Djimat,
        KeyItem,
        SacredStone,
        DiamondSlot,
        Money
    }

    [Header("Item Type")]
    public ItemType itemType = ItemType.Djimat;

    [Header("Basic")]
    public string itemName = "New Djimat";
    public Sprite icon;

    [Header("Equip")]
    public int slotCost = 1;         // how many slot diamonds it consumes
    public int plusslotCost = 0;     // buat limit slot

    public int plusMoney = 0;
    public string itemPrice = "0";

    public int healthBonus = 0;
    public int damageBonus = 0;
    public int defenseBonus = 0;
    public int lifestealBonus = 0;

    public bool TSacredStone = false;
    public bool SacredStone = false;
    public bool PSacredStone = false;
    public bool DSacredStone = false;

    public bool LiveAgain = false;

    [TextArea] public string description;
}