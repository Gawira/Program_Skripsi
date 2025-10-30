//using UnityEngine;
//using UnityEngine.Audio;
//using System;

//public class SettingsManager : MonoBehaviour
//{
//    public static SettingsManager I { get; private set; }

//    [Header("Audio")]
//    public AudioMixer mixer;                   // optional, recommended
//    [Range(0f, 1f)][SerializeField] float music = 1f;
//    [Range(0f, 1f)][SerializeField] float sfx = 1f;

//    [Header("Video/UI")]
//    [Range(0f, 1f)][SerializeField] float brightness = 1f;  // drive a screen overlay alpha
//    [SerializeField] bool fullscreen = true;
//    [SerializeField] int vSync = 1;

//    [Header("Gameplay/Camera")]
//    [Range(0.1f, 3f)][SerializeField] float mouseSensitivity = 1f;
//    [SerializeField] bool invertY = false;

//    public event Action OnSettingsApplied;

//    // Expose read-only properties
//    public float MusicVolume => music;
//    public float SfxVolume => sfx;
//    public float Brightness => brightness;
//    public float MouseSensitivity => mouseSensitivity;
//    public bool InvertY => invertY;

//    void Awake()
//    {
//        if (I != null && I != this) { Destroy(gameObject); return; }
//        I = this;
//        DontDestroyOnLoad(gameObject);
//        LoadFromPrefs();
//        ApplyAll();
//    }

//    // ---------- Setters (UI calls these) ----------
//    public void SetMusic(float v) { music = Mathf.Clamp01(v); ApplyAudio(); Save("music", music); }
//    public void SetSfx(float v) { sfx = Mathf.Clamp01(v); ApplyAudio(); Save("sfx", sfx); }
//    public void SetBrightness(float v) { brightness = Mathf.Clamp01(v); ApplyBrightness(); Save("brightness", brightness); }
//    public void SetMouseSensitivity(float v) { mouseSensitivity = Mathf.Clamp(v, .1f, 3f); ApplyGameplay(); Save("sens", mouseSensitivity); }
//    public void SetInvertY(bool on) { invertY = on; ApplyGameplay(); PlayerPrefs.SetInt("invertY", on ? 1 : 0); }
//    public void SetFullscreen(bool on) { fullscreen = on; Screen.fullScreen = on; PlayerPrefs.SetInt("fullscreen", on ? 1 : 0); }
//    public void SetVSync(int count) { vSync = Mathf.Clamp(count, 0, 2); QualitySettings.vSyncCount = vSync; PlayerPrefs.SetInt("vsync", vSync); }

//    // ---------- Apply ----------
//    public void ApplyAll() { ApplyAudio(); ApplyBrightness(); ApplyGameplay(); OnSettingsApplied?.Invoke(); }

//    void ApplyAudio()
//    {
//        // If using AudioMixer (recommended)
//        if (mixer != null)
//        {
//            mixer.SetFloat("MusicVol", ToDb(music));
//            mixer.SetFloat("SfxVol", ToDb(sfx));
//        }
//        else
//        {
//            // Fallback: push to your AudioManager volumes
//            if (AudioManager.Instance != null)
//            {
//                AudioManager.Instance.SetMusicVolume(music);
//                AudioManager.Instance.SetSFXVolume(sfx);
//            }
//        }
//    }

//    void ApplyBrightness()
//    {
//        // Drive your global brightness overlay if you have one:
//        var overlay = FindObjectOfType<BrightnessOverlay>(includeInactive: true);
//        if (overlay != null) overlay.SetLevel(brightness);
//    }

//    void ApplyGameplay()
//    {
//        // Example: FreeLookCam sensitivity + invert
//        var cams = FindObjectsOfType<FreeLookCam>(true);
//        foreach (var cam in cams)
//        {
//            cam.SetSensitivity(mouseSensitivity);
//            cam.SetInvertY(invertY);
//        }
//    }

//    // ---------- Persistence ----------
//    void LoadFromPrefs()
//    {
//        music = PlayerPrefs.GetFloat("music", 1f);
//        sfx = PlayerPrefs.GetFloat("sfx", 1f);
//        brightness = PlayerPrefs.GetFloat("brightness", 1f);
//        mouseSensitivity = PlayerPrefs.GetFloat("sens", 1f);
//        invertY = PlayerPrefs.GetInt("invertY", 0) == 1;
//        fullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1;
//        vSync = PlayerPrefs.GetInt("vsync", 1);

//        Screen.fullScreen = fullscreen;
//        QualitySettings.vSyncCount = vSync;
//    }

//    void Save(string key, float value) { PlayerPrefs.SetFloat(key, value); }
//    static float ToDb(float linear) => (linear <= 0.0001f) ? -80f : Mathf.Log10(linear) * 20f;
//}
