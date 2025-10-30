// AreaNameTrigger.cs
using System.Collections;
using UnityEngine;
using TMPro;

public class AreaNameTrigger : MonoBehaviour
{
    [Header("Area")]
    public string areaName = "Beach City Mall";
    public bool oneShot = true;

    [Header("UI Banner")]
    public CanvasGroup bannerCanvas;
    public TMP_Text bannerText;
    public float fadeIn = 0.5f, hold = 1.2f, fadeOut = 0.7f;

    [Header("Audio")]
    [Tooltip("SFX to play when entering this area.")]
    public AudioClip areaEnterSFX;
    [Tooltip("If true, plays as positional 3D SFX at trigger position; otherwise as 2D UI SFX.")]
    public bool playAs3D = false;
    [Range(0f, 1f)]
    [Tooltip("Only used if playAs3D = true.")]
    public float sfxSpatialBlend = 1f;

    private bool shown;

    private void OnTriggerEnter(Collider other)
    {
        if (shown && oneShot) return;
        if (!other.CompareTag("Player")) return;

        shown = true;

        PlayAreaSFX();

        if (bannerCanvas != null)
            StartCoroutine(FadeBanner());
    }

    private void PlayAreaSFX()
    {
        if (areaEnterSFX == null || AudioManager.Instance == null) return;

        if (playAs3D)
        {
            // positional in-world, respects SFX volume slider
            AudioManager.Instance.PlaySFXAtPoint(areaEnterSFX, transform.position, Mathf.Clamp01(sfxSpatialBlend));
        }
        else
        {
            // 2D/global (good for UI-style stingers)
            AudioManager.Instance.PlaySFX(areaEnterSFX);
        }
    }

    IEnumerator FadeBanner()
    {
        if (bannerText != null) bannerText.text = areaName;
        bannerCanvas.gameObject.SetActive(true);
        bannerCanvas.alpha = 0f;

        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.deltaTime;
            bannerCanvas.alpha = Mathf.Lerp(0, 1, t / fadeIn);
            yield return null;
        }

        yield return new WaitForSeconds(hold);

        t = 0f;
        while (t < fadeOut)
        {
            t += Time.deltaTime;
            bannerCanvas.alpha = Mathf.Lerp(1, 0, t / fadeOut);
            yield return null;
        }

        bannerCanvas.gameObject.SetActive(false);
    }
}
