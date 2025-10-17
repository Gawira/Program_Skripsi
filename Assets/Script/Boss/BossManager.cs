using System;
using UnityEngine;
using UnityEngine.UI;

public class BossManager : MonoBehaviour
{
    [Header("Boss Stats")]
    public string bossName = "Boss";
    public int maxHealth = 1000;
    public int currentHealth;
    public int damage = 20;
    public int moneyDrop = 1000;

    [Header("UI Settings")]
    public GameObject bossHealthBarUI;
    public Slider bossHealthSlider;
    public Slider bossDelayedHealthSlider;   // for delayed bar
    public TMPro.TMP_Text bossNameText;

    [Header("Health Bar Settings")]
    [SerializeField] private float smoothSpeed = 20f;
    [SerializeField] private float delaySpeed = 1f;

    [Header("Player")]
    public string playerTag = "Player";
    public PlayerManager playerManager;

    public event Action<BossManager> OnBossDied;
    private Animator anim;

    private void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();

        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.SetActive(false);
            if (bossHealthSlider != null)
                bossHealthSlider.maxValue = maxHealth;

            if (bossDelayedHealthSlider != null)
                bossDelayedHealthSlider.maxValue = maxHealth;

            if (bossNameText != null)
                bossNameText.text = bossName;

            bossHealthSlider.value = maxHealth;
            if (bossDelayedHealthSlider != null)
                bossDelayedHealthSlider.value = maxHealth;
        }

        if (playerManager == null)
            playerManager = FindObjectOfType<PlayerManager>();
    }

    private void Update()
    {
        if (bossHealthSlider == null) return;

        // Target health value
        int targetHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Main health bar: fast update
        bossHealthSlider.value = Mathf.Lerp(bossHealthSlider.value, targetHealth, Time.deltaTime * smoothSpeed);

        // Delayed health bar: lag behind when losing HP
        if (bossDelayedHealthSlider != null)
        {
            if (bossDelayedHealthSlider.value > targetHealth)
            {
                bossDelayedHealthSlider.value = Mathf.Lerp(bossDelayedHealthSlider.value, targetHealth, Time.deltaTime * delaySpeed);
            }
            else
            {
                bossDelayedHealthSlider.value = targetHealth; // snap when healing
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0)
        {
            playerManager?.AddMoney(moneyDrop);
            Die();
        }
    }

    private void Die()
    {
        if (anim != null)
            anim.SetTrigger("Die");

        OnBossDied?.Invoke(this);

        if (bossHealthBarUI != null)
            bossHealthBarUI.SetActive(false);

        Destroy(gameObject, 2f);
    }

    public void ActivateBossUI()
    {
        if (bossHealthBarUI != null)
            bossHealthBarUI.SetActive(true);
    }

    public void DeactivateBossUI()
    {
        if (bossHealthBarUI != null)
            bossHealthBarUI.SetActive(false);
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"Boss hits player for {damage}");
            // playerManager.TakeDamage(damage);
        }
    }
}
