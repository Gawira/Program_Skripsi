using UnityEngine;
using UnityEngine.SceneManagement;

public class TimelineSceneLoader : MonoBehaviour
{
    [Header("Scene to load at activation")]
    public string sceneName = "Outro";

    [Header("Only run once?")]
    public bool onlyOnce = true;

    private bool alreadyTriggered = false;

    // This gets called the moment the GameObject is activated by Timeline
    private void OnEnable()
    {
        // prevent double-trigger if Timeline toggles it again
        if (onlyOnce && alreadyTriggered)
            return;

        alreadyTriggered = true;

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("[TimelineSceneLoader] sceneName is empty, not loading.");
        }
    }
}
