// CutsceneBridge.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CutsceneBridge : MonoBehaviour
{
    public static CutsceneBridge Instance { get; private set; }

    [Header("Payload")]
    public VideoClip clip;
    public string nextSceneName;
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Space;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ConsumeAndDestroy()
    {
        if (Instance == this) Instance = null;
        Destroy(gameObject);
    }

    /// <summary>Convenience: spawn bridge and jump to your cutscene scene.</summary>
    public static void Play(VideoClip clip, string nextScene, string cutsceneSceneName = "CutsceneScene",
                            bool allowSkip = true, KeyCode skipKey = KeyCode.Space)
    {
        var go = new GameObject("CutsceneBridge");
        var bridge = go.AddComponent<CutsceneBridge>();
        bridge.clip = clip;
        bridge.nextSceneName = nextScene;
        bridge.allowSkip = allowSkip;
        bridge.skipKey = skipKey;

        SceneManager.LoadScene(cutsceneSceneName);
    }
}
