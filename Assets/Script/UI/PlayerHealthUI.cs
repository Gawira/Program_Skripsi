using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Bars")]
    [SerializeField] private Slider mainHealthSlider;      // instant HP bar
    [SerializeField] private Slider delayedHealthSlider;   // delayed/lost HP bar

    [Header("Player Reference")]
    [SerializeField] private PlayerManager player;         // drag PlayerManager here

    [Header("Bar Stretch Settings")]
    [SerializeField] private RectTransform barContainer;   // the RectTransform you want to stretch (e.g. parent of the sliders, or the red bar background)
    [SerializeField] private float baseWidth = 200f;        // width at base HP (tune in Inspector)

    [Header("Animation Speeds")]
    [SerializeField] private float smoothSpeed = 10f;       // how fast main bar catches up
    [SerializeField] private float delaySpeed = 2f;         // how fast delayed bar shrinks

    private int baseMaxHealth; // remembers the "normal" max HP for scaling

    void Start()
    {
        // Record what "normal health bar" looks like at start.
        // This is our baseline for Sacred Vest scaling.
        baseMaxHealth = player.playerHealth;

        // Initialize max slider values
        mainHealthSlider.maxValue = player.playerHealth;
        delayedHealthSlider.maxValue = player.playerHealth;

        mainHealthSlider.value = player.playerHealth;
        delayedHealthSlider.value = player.playerHealth;

        // Initialize width of the bar container to baseWidth
        if (barContainer != null)
        {
            Vector2 size = barContainer.sizeDelta;
            size.x = baseWidth;
            barContainer.sizeDelta = size;
        }
    }

    void Update()
    {
        // --- 1. Sync max health changes (Sacred Vest etc.) ---
        // If DjimatSystem changed player.playerHealth, reflect that in sliders and width.
        if (mainHealthSlider.maxValue != player.playerHealth)
        {
            mainHealthSlider.maxValue = player.playerHealth;
            delayedHealthSlider.maxValue = player.playerHealth;
        }

        // Stretch HP bar length based on how large max HP is compared to baseline
        if (barContainer != null && baseMaxHealth > 0)
        {
            float ratio = (float)player.playerHealth / (float)baseMaxHealth;
            float targetWidth = baseWidth * ratio;

            // Smoothly lerp width so it grows/shrinks nicely instead of popping
            Vector2 size = barContainer.sizeDelta;
            size.x = Mathf.Lerp(size.x, targetWidth, Time.deltaTime * 10f);
            barContainer.sizeDelta = size;
        }

        // --- 2. Animate the sliders based on current HP ---
        int targetHealth = Mathf.Clamp(player.currentHealth, 0, player.playerHealth);

        // main HP bar: fast / smooth
        mainHealthSlider.value = Mathf.Lerp(
            mainHealthSlider.value,
            targetHealth,
            Time.deltaTime * smoothSpeed
        );

        // delayed HP bar: lags behind when taking damage, instant when healing
        if (delayedHealthSlider.value > targetHealth)
        {
            // took damage: bleed-down slowly
            delayedHealthSlider.value = Mathf.Lerp(
                delayedHealthSlider.value,
                targetHealth,
                Time.deltaTime * delaySpeed
            );
        }
        else
        {
            // healed: snap to current HP
            delayedHealthSlider.value = targetHealth;
        }
    }
}
