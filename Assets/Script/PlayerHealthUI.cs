using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Bars")]
    [SerializeField] private Slider mainHealthSlider;   // Instant health
    [SerializeField] private Slider delayedHealthSlider; // Delayed/lost health

    [Header("Player Reference")]
    [SerializeField] private PlayerManager player;  // Drag your Player here

    [Header("Settings")]
    [SerializeField] private float smoothSpeed = 10f;    // Speed for smooth transition
    [SerializeField] private float delaySpeed = 2f;      // Speed for delayed slider

    void Start()
    {
        // Initialize max values
        mainHealthSlider.maxValue = player.playerHealth;
        delayedHealthSlider.maxValue = player.playerHealth;

        mainHealthSlider.value = player.playerHealth;
        delayedHealthSlider.value = player.playerHealth;
    }

    void Update()
    {
        int targetHealth = Mathf.Clamp(player.currentHealth, 0, player.playerHealth);

        // Smoothly move main health slider
        mainHealthSlider.value = Mathf.Lerp(mainHealthSlider.value, targetHealth, Time.deltaTime * smoothSpeed);

        // Delayed slider follows after a lag
        if (delayedHealthSlider.value > targetHealth)
        {
            // Losing health = delayed bar decreases slowly
            delayedHealthSlider.value = Mathf.Lerp(delayedHealthSlider.value, targetHealth, Time.deltaTime * delaySpeed);
        }
        else
        {
            // Gaining health = snap instantly to match
            delayedHealthSlider.value = targetHealth;
        }
    }
}
