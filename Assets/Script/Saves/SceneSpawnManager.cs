using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSpawnManager : MonoBehaviour
{
    public static SceneSpawnManager Instance;
    public static bool overrideSpawnThisScene = false; // New flag

    [Header("Settings")]
    public string spawnPointName = "DefaultSpawnPoint";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSpawnPoint(string pointName)
    {
        spawnPointName = pointName;
        overrideSpawnThisScene = true; // mark for override
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!overrideSpawnThisScene) return; // only run when told to
        overrideSpawnThisScene = false; // reset flag

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        GameObject spawn = GameObject.Find(spawnPointName);
        if (spawn != null)
        {
            player.transform.position = spawn.transform.position;
            player.transform.rotation = spawn.transform.rotation;
            Debug.Log($"[SceneSpawnManager] Player spawned at '{spawnPointName}'");
        }
        else
        {
            Debug.LogWarning($"[SceneSpawnManager] No spawn point named '{spawnPointName}' found!");
        }
    }
}