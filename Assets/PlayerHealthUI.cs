using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for UI

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;   // Drag your HealthBar (Slider) here
    [SerializeField] private PlayerManager player;  // Drag your Player here

    void Start()
    {
        // Initialize the max value of the slider
        healthSlider.maxValue = player.playerHealth;
        healthSlider.value = player.playerHealth;
    }

    void Update()
    {
        // Keep the slider updated to current health
        healthSlider.value = playerHealth();
    }

    private int playerHealth()
    {
        // Expose current health safely
        return Mathf.Clamp(player.currentHealth, 0, player.playerHealth);
    }
}