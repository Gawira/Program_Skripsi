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

    [Header("Loot Drop")]
    [Tooltip("Prefab dropped when boss dies (e.g. harta). Will be spawned once.")]
    public GameObject lootPrefab;
    public Vector3 lootDropOffset = new Vector3(0f, 1f, 0f); // spawn a bit above ground
    private bool lootDropped = false;

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

            if (bossHealthSlider != null)
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

        // clamp target
        int targetHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // fast bar
        bossHealthSlider.value = Mathf.Lerp(
            bossHealthSlider.value,
            targetHealth,
            Time.deltaTime * smoothSpeed
        );

        // delayed bar
        if (bossDelayedHealthSlider != null)
        {
            if (bossDelayedHealthSlider.value > targetHealth)
            {
                bossDelayedHealthSlider.value = Mathf.Lerp(
                    bossDelayedHealthSlider.value,
                    targetHealth,
                    Time.deltaTime * delaySpeed
                );
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
            // reward player
            playerManager?.AddMoney(moneyDrop);

            // play death anim
            if (anim != null)
                anim.SetTrigger("Death");

            Die();
        }
    }

    private void SpawnLootIfNeeded()
    {
        if (lootDropped) return;
        lootDropped = true;

        if (lootPrefab != null)
        {
            Vector3 dropPos = transform.position + lootDropOffset;
            Instantiate(lootPrefab, dropPos, Quaternion.identity);
        }
    }

    private void Die()
    {
        // trigger anim again just in case
        if (anim != null)
            anim.SetTrigger("Death");

        // drop harta / loot
        SpawnLootIfNeeded();

        // notify listeners
        OnBossDied?.Invoke(this);

        // hide UI
        DeactivateBossUI();

        // cleanup boss after a delay
        Destroy(gameObject, 3f);
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

    // this is a public version so other scripts can hide the UI
    public void ForceHideUI()
    {
        DeactivateBossUI();
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
            // playerManager.TakeDamage(damage); // if you hook player damage later
        }
    }
}
