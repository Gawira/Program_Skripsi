using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class CutsceneSceneManager : MonoBehaviour
{
    public VideoPlayer player;                       // assign in the Intro/Cutscene scene
    public string fallbackNextScene = "SampleScene"; // used if bridge missing
    public float safetyExtraSeconds = 1.0f;          // timeout buffer

    bool finished;
    CutsceneBridge bridge;

    void Start()
    {
        bridge = CutsceneBridge.Instance;

        if (player == null) player = FindObjectOfType<VideoPlayer>();

        // If we have no bridge or no player/clip, bail straight to next.
        string next = (bridge != null && !string.IsNullOrEmpty(bridge.nextSceneName))
                        ? bridge.nextSceneName
                        : fallbackNextScene;

        if (bridge == null || player == null || bridge.clip == null)
        {
            LoadNext(next);
            return;
        }

        // Configure and prepare before subscribing/playing.
        player.isLooping = false;
        player.Stop();
        player.clip = bridge.clip;

        // Sub after we know the player exists, before Play().
        player.prepareCompleted += OnPrepared;
        player.loopPointReached += OnVideoFinished;

        player.Prepare();

        // Safety fallback in case loopPointReached never fires on some platforms.
        StartCoroutine(SafetyTimeout((float)bridge.clip.length + safetyExtraSeconds, next));
    }

    void Update()
    {
        if (finished || bridge == null) return;

        if (bridge.allowSkip && Input.GetKeyDown(bridge.skipKey))
        {
            OnVideoFinished(null);
        }
    }

    void OnPrepared(VideoPlayer vp)
    {
        if (finished) return;
        vp.Play();
    }

    void OnDisable() => Unhook();

    void Unhook()
    {
        if (player != null)
        {
            player.prepareCompleted -= OnPrepared;
            player.loopPointReached -= OnVideoFinished;
        }
    }

    IEnumerator SafetyTimeout(float seconds, string next)
    {
        yield return new WaitForSeconds(seconds);
        if (!finished) LoadNext(next);
    }

    void OnVideoFinished(VideoPlayer _)
    {
        if (finished) return;
        finished = true;

        string next = (bridge != null && !string.IsNullOrEmpty(bridge.nextSceneName))
                        ? bridge.nextSceneName
                        : fallbackNextScene;

        Unhook();
        bridge?.ConsumeAndDestroy(); // <- ensure a fresh run next time
        SceneManager.LoadScene(next);
    }

    void LoadNext(string next)
    {
        if (finished) return;
        finished = true;
        Unhook();
        bridge?.ConsumeAndDestroy();
        SceneManager.LoadScene(next);
    }
}