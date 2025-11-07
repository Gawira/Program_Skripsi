using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class HowToPlayUIManager : MonoBehaviour
{
    [System.Serializable]
    public class PanelGroup
    {
        public string name;
        public GameObject panel;
        [Tooltip("Assign the two VideoPlayers for this panel. If left empty, the script will auto-find all VideoPlayers under the panel.")]
        public VideoPlayer[] videos;
    }

    [Header("Groups")]
    [SerializeField] private PanelGroup movement;
    [SerializeField] private PanelGroup lockOn;
    [SerializeField] private PanelGroup evade;
    [SerializeField] private PanelGroup combat;

    [Header("Options")]
    [Tooltip("If ON, when you open a panel the videos restart from 0 instead of resuming.")]
    [SerializeField] private bool restartOnShow = false;

    // track what’s currently shown so we can pause only that group
    private PanelGroup _current;

    private void Awake()
    {
        // Fill missing arrays by auto-finding VideoPlayers under each panel.
        AutoFill(movement);
        AutoFill(lockOn);
        AutoFill(evade);
        AutoFill(combat);
    }

    private void Start()
    {
        // default: show movement panel
        ShowPanel(movement);
    }

    // ---- Public UI hooks ----
    public void ShowMovement() => ShowPanel(movement);
    public void ShowLockOn() => ShowPanel(lockOn);
    public void ShowEvade() => ShowPanel(evade);
    public void ShowCombat() => ShowPanel(combat);

    public void BackToMenu()
    {
        PauseGroup(_current);
        HideAllPanels();
        _current = null;
    }

    // ---- Core logic ----
    private void ShowPanel(PanelGroup group)
    {
        if (group == null || group.panel == null) return;

        // Pause the one we’re leaving
        if (_current != null) PauseGroup(_current);

        // Hide all, then show this one
        HideAllPanels();
        group.panel.SetActive(true);

        // Play this group’s videos (start both at the same time)
        StartCoroutine(PlayGroup(group));

        _current = group;
    }

    private IEnumerator PlayGroup(PanelGroup group)
    {
        if (group.videos == null) yield break;

        // Optional restart
        if (restartOnShow)
        {
            foreach (var vp in group.videos)
            {
                if (vp == null) continue;
                if (vp.isPrepared) vp.time = 0;
            }
        }

        // Ensure all are prepared first to reduce desync
        foreach (var vp in group.videos)
        {
            if (vp == null) continue;
            if (!vp.isPrepared) vp.Prepare();
        }

        // Wait until all are prepared (or already were)
        bool allPrepared;
        do
        {
            allPrepared = true;
            foreach (var vp in group.videos)
            {
                if (vp == null) continue;
                if (!vp.isPrepared) { allPrepared = false; break; }
            }
            if (!allPrepared) yield return null;
        } while (!allPrepared);

        // Fire them together
        foreach (var vp in group.videos)
        {
            if (vp == null) continue;
            vp.Play();
        }
    }

    private void PauseGroup(PanelGroup group)
    {
        if (group == null || group.videos == null) return;
        foreach (var vp in group.videos)
        {
            if (vp != null && vp.isPlaying) vp.Pause();
        }
    }

    private void HideAllPanels()
    {
        if (movement.panel) movement.panel.SetActive(false);
        if (lockOn.panel) lockOn.panel.SetActive(false);
        if (evade.panel) evade.panel.SetActive(false);
        if (combat.panel) combat.panel.SetActive(false);
    }

    private void AutoFill(PanelGroup g)
    {
        if (g == null || g.panel == null) return;
        if (g.videos == null || g.videos.Length == 0)
            g.videos = g.panel.GetComponentsInChildren<VideoPlayer>(true);
    }
}
