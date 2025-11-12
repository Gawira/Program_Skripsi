using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Bars")]
    [SerializeField] private Slider mainHealthSlider;      // instant HP bar
    [SerializeField] private Slider delayedHealthSlider;   // delayed/lost HP bar

    [Header("Player Reference")]
    [SerializeField] private PlayerManager player;

    [Header("Bar Scale Settings")]
    [SerializeField] private RectTransform barContainer;   // parent/background you want to scale on X
    [Tooltip("Lerp speed for scale changes.")]
    [SerializeField] private float scaleLerpSpeed = 10f;

    [Header("Animation Speeds")]
    [SerializeField] private float smoothSpeed = 10f;      // how fast main bar catches up
    [SerializeField] private float delaySpeed = 2f;      // how fast delayed bar shrinks

    private int baseMaxHealth;
    private float baseScaleX = 1f;                         // remember original X scale

    void Start()
    {
        baseMaxHealth = player.playerHealth;

        mainHealthSlider.maxValue = player.playerHealth;
        delayedHealthSlider.maxValue = player.playerHealth;
        mainHealthSlider.value = player.playerHealth;
        delayedHealthSlider.value = player.playerHealth;

        if (barContainer != null)
            baseScaleX = barContainer.localScale.x;        // cache starting scale
    }

    void Update()
    {
        // 1) Sync max health changes (e.g., Sacred Vest etc.)
        if (mainHealthSlider.maxValue != player.playerHealth)
        {
            mainHealthSlider.maxValue = player.playerHealth;
            delayedHealthSlider.maxValue = player.playerHealth;
        }

        // 2) Stretch by scaling on X instead of changing width
        if (barContainer != null && baseMaxHealth > 0)
        {
            float ratio = (float)player.playerHealth / (float)baseMaxHealth; // e.g., 0.5x, 1.2x, etc.
            float targetScaleX = Mathf.Max(0.001f, baseScaleX * ratio);

            Vector3 s = barContainer.localScale;
            s.x = Mathf.Lerp(s.x, targetScaleX, Time.deltaTime * scaleLerpSpeed);
            barContainer.localScale = s;
        }

        // 3) Animate slider values to current HP
        int targetHealth = Mathf.Clamp(player.currentHealth, 0, player.playerHealth);

        mainHealthSlider.value = Mathf.Lerp(
            mainHealthSlider.value, targetHealth, Time.deltaTime * smoothSpeed);

        if (delayedHealthSlider.value > targetHealth)
        {
            delayedHealthSlider.value = Mathf.Lerp(
                delayedHealthSlider.value, targetHealth, Time.deltaTime * delaySpeed);
        }
        else
        {
            delayedHealthSlider.value = targetHealth;
        }
    }
}
