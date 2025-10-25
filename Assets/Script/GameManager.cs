using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;

public class GameManager : MonoBehaviour
{
    // ===== Singleton =====
    public static GameManager Instance { get; private set; }

    [Header("Systems")]
    public LockOnTarget lockOnSystem;

    // ===== Persistent World State =====
    // Doors that have been permanently opened/unlocked
    private HashSet<string> openedDoors = new HashSet<string>();

    // Pickups that have been collected and should NOT respawn
    private HashSet<string> collectedPickups = new HashSet<string>();

    private void Awake()
    {
        // singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // survive scene changes

        // You can also do other 1-time global init here if needed later
    }

    private void Start()
    {
        // Your original startup code
        QualitySettings.vSyncCount = 0; // VSync Off
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        // (empty for now, keep if you’ll add global logic later)
    }

    // ===== DOOR STATE API =====
    public bool IsDoorOpened(string doorID)
    {
        return !string.IsNullOrEmpty(doorID) && openedDoors.Contains(doorID);
    }

    public void MarkDoorOpened(string doorID)
    {
        if (!string.IsNullOrEmpty(doorID))
            openedDoors.Add(doorID);
    }

    public IEnumerable<string> GetOpenedDoors()
    {
        return openedDoors;
    }

    // ===== PICKUP STATE API =====
    public bool IsPickupCollected(string pickupID)
    {
        return !string.IsNullOrEmpty(pickupID) && collectedPickups.Contains(pickupID);
    }

    public void MarkPickupCollected(string pickupID)
    {
        if (!string.IsNullOrEmpty(pickupID))
            collectedPickups.Add(pickupID);
    }

    public IEnumerable<string> GetCollectedPickups()
    {
        return collectedPickups;
    }

    // ===== APPLY LOADED SAVE STATE =====
    // This is called after loading SaveData so we rebuild our sets
    public void ApplyLoadedState(
        List<string> loadedDoors,
        List<string> loadedPickups)
    {
        openedDoors.Clear();
        collectedPickups.Clear();

        if (loadedDoors != null)
        {
            foreach (var d in loadedDoors)
                openedDoors.Add(d);
        }

        if (loadedPickups != null)
        {
            foreach (var p in loadedPickups)
                collectedPickups.Add(p);
        }
    }
}
