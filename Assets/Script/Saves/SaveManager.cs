using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static int activeSlot = -1;
    private static string SaveFolder => Application.persistentDataPath + "/saves";

    public static void SetActiveSlot(int slotIndex)
    {
        activeSlot = slotIndex;
        Debug.Log($"Active Save Slot set to {slotIndex}");
    }

    private static string GetSavePath(int slot)
    {
        return Path.Combine(SaveFolder, $"save_slot{slot + 1}.json");
    }

    private static string ActiveSavePath => GetSavePath(activeSlot);

    public static void SaveGame(SaveData data)
    {
        if (activeSlot < 0)
        {
            Debug.LogError("No active slot set!");
            return;
        }

        if (!Directory.Exists(SaveFolder))
            Directory.CreateDirectory(SaveFolder);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(ActiveSavePath, json);
        Debug.Log($"Game saved at: {ActiveSavePath}");
    }

    public static SaveData LoadGame()
    {
        if (activeSlot < 0)
        {
            Debug.LogError("No active slot set!");
            return null;
        }

        if (!File.Exists(ActiveSavePath))
        {
            Debug.LogWarning($"No save file found for slot {activeSlot}");
            return null;
        }

        string json = File.ReadAllText(ActiveSavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log($"Game loaded from: {ActiveSavePath}");
        return data;
    }

    public static bool SaveExists(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }
    public static bool SaveExistsForActiveSlot()
    {
        return activeSlot >= 0 && File.Exists(ActiveSavePath);
    }

    public static void DeleteSave(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Save file deleted: {path}");
        }
    }

    public static void DeleteSaveForActiveSlot()
    {
        if (activeSlot < 0)
        {
            Debug.LogWarning("No active save slot selected.");
            return;
        }

        string path = ActiveSavePath;
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
            Debug.Log($"Deleted save file at slot {activeSlot}: {path}");
        }
        else
        {
            Debug.LogWarning($"No save file to delete at slot {activeSlot}.");
        }
    }
}
