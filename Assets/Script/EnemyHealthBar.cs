using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider healthSlider;     // Slider in the world-space canvas
    [SerializeField] private Slider delayedhealthSlider;

    [SerializeField] private EnemyManager enemy;      // Your enemy script that has health

    [SerializeField] private float smoothSpeed = 20f;    // Speed for smooth transition
    [SerializeField] private float delaySpeed = 1f;      // Speed for delayed slider

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        healthSlider.maxValue = enemy.maxHealth;
        delayedhealthSlider.maxValue = enemy.maxHealth;

        healthSlider.value = enemy.currentHealth;
        delayedhealthSlider.value = enemy.currentHealth;
    }

    void Update()
    {
        if (!enemy) return;

        // Update health
        healthSlider.value = enemy.currentHealth;

        int targetHealth = Mathf.Clamp(enemy.currentHealth, 0, enemy.maxHealth);

        // Smoothly move main health slider
        healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealth, Time.deltaTime * smoothSpeed);

        // Delayed slider follows after a lag
        if (delayedhealthSlider.value > targetHealth)
        {
            // Losing health = delayed bar decreases slowly
            delayedhealthSlider.value = Mathf.Lerp(delayedhealthSlider.value, targetHealth, Time.deltaTime * delaySpeed);
        }
        else
        {
            // Gaining health = snap instantly to match
            delayedhealthSlider.value = targetHealth;
        }

        // Hide if dead
        if (enemy.currentHealth <= 0)
        {
            healthSlider.gameObject.SetActive(false);
            return;
        }


        // Always face camera
        healthSlider.transform.rotation = Quaternion.LookRotation(healthSlider.transform.position - mainCamera.transform.position);
        delayedhealthSlider.transform.rotation = Quaternion.LookRotation(healthSlider.transform.position - mainCamera.transform.position);
    }
}
