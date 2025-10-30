// AudioSliderBinder.cs
using UnityEngine;
using UnityEngine.UI;

public class AudioSliderBinder : MonoBehaviour
{
    public enum SliderType { Music, SFX }
    [Header("Which volume does this control?")]
    public SliderType type = SliderType.Music;

    [Tooltip("Keep the slider handle in sync if volume changes elsewhere.")]
    public bool liveSync = true;

    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        if (slider == null)
            Debug.LogError("[AudioSliderBinder] Put me on a UI Slider.");
    }

    void OnEnable() { TryBind(); }
    void OnDisable() { Unbind(); }

    public void TryBind()
    {
        if (slider == null) return;
        var am = AudioManager.Instance;
        if (am == null) return;

        // Clean old listeners (safe on reloads)
        slider.onValueChanged.RemoveListener(OnSliderChanged);

        // Initial sync from manager -> slider
        slider.value = (type == SliderType.Music) ? am.musicVolume : am.sfxVolume;

        // Wire slider -> manager
        slider.onValueChanged.AddListener(OnSliderChanged);

        // Optional live sync manager -> slider
        if (liveSync)
        {
            am.OnMusicVolumeChanged -= HandleMusicChanged;
            am.OnSFXVolumeChanged -= HandleSFXChanged;
            am.OnMusicVolumeChanged += HandleMusicChanged;
            am.OnSFXVolumeChanged += HandleSFXChanged;
        }
    }

    private void Unbind()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);

        var am = AudioManager.Instance;
        if (am != null && liveSync)
        {
            am.OnMusicVolumeChanged -= HandleMusicChanged;
            am.OnSFXVolumeChanged -= HandleSFXChanged;
        }
    }

    private void OnSliderChanged(float v)
    {
        var am = AudioManager.Instance;
        if (am == null) return;
        if (type == SliderType.Music) am.SetMusicVolume(v);
        else am.SetSFXVolume(v);
    }

    private void HandleMusicChanged(float v)
    {
        if (type != SliderType.Music || slider == null) return;
        if (!Mathf.Approximately(slider.value, v)) slider.value = v;
    }

    private void HandleSFXChanged(float v)
    {
        if (type != SliderType.SFX || slider == null) return;
        if (!Mathf.Approximately(slider.value, v)) slider.value = v;
    }
}
