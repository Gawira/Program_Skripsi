using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    // Core player info
    public int playerHealth;
    public int playerMoney;
    public Vector3 checkpointPosition;

    // Djimat info
    public List<string> equippedDjimatIDs = new List<string>();
    public List<string> inventoryDjimatIDs = new List<string>();

    // Optional: add other data later (like boss states, unlocked areas, etc.)
}
