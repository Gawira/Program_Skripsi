using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDjimat", menuName = "Inventory/Djimat")]
public class DjimatItem : ScriptableObject
{
    [Header("Basic")]
    public string itemName = "New Djimat";
    public Sprite icon;

    [Header("Equip")]
    public int slotCost = 1;         // how many slot diamonds it consumes
    public int healthBonus = 0;
    public int damageBonus = 0;
    public int defenseBonus = 0;
    public int lifestealBonus = 0;

    [TextArea] public string description;
}