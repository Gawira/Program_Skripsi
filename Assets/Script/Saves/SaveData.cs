using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    // --- Player Stats ---
    public int playerHealth;
    public int currentHealth;
    public int playerMoney;
    public int damage;
    public int lifesteal;
    public int defense;
    public int slotMax;

    // --- Checkpoint ---
    public Vector3 checkpointPosition;
    public Quaternion checkpointRotation;

    // --- Djimat System ---
    public System.Collections.Generic.List<string> equippedDjimatIDs = new();
    public System.Collections.Generic.List<string> inventoryDjimatIDs = new();

    // --- Sacred Stones ---
    public System.Collections.Generic.List<string> sacredStoneIDs = new();

    // --- Key Items ---
    public System.Collections.Generic.List<string> keyItemIDs = new();

    // --- Merchant sold state ---
    public System.Collections.Generic.List<string> soldOutItems = new();

    // --- Weapon upgrade progression ---
    public int weaponUpgradeLevel = 0;

    // --- World state ---
    public System.Collections.Generic.List<string> openedDoorIDs = new();
    public System.Collections.Generic.List<string> collectedPickupIDs = new();
}
