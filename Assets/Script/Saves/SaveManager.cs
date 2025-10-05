using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static string SavePath => Application.persistentDataPath + "/save.json";

    public static void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Game saved at: " + SavePath);
    }

    public static SaveData LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("No save file found!");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log("Game loaded from: " + SavePath);
        return data;
    }

    

    public static bool SaveExists() => File.Exists(SavePath);

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                File.Delete(SavePath);
                Debug.Log("Save file deleted: " + SavePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to delete save file: " + ex);
            }
        }
        else
        {
            Debug.LogWarning("No save file to delete at: " + SavePath);
        }
    }
}
