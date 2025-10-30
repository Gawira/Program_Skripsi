using System;
using System.Collections;
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
    public Vector3 lootDropOffset = new Vector3(0f, 1f, 0f);
    private bool lootDropped = false;

    [Header("UI Settings")]
    public GameObject bossHealthBarUI;
    public Slider bossHealthSlider;
    public Slider bossDelayedHealthSlider;   // delayed bar
    public TMPro.TMP_Text bossNameText;

    [Header("Health Bar Settings")]
    [SerializeField] private float smoothSpeed = 20f;
    [SerializeField] private float delaySpeed = 1f;

    [Header("Player")]
    public string playerTag = "Player";
    public PlayerManager playerManager;

    // === NEW: Victory banner & SFX ===
    [Header("Victory Banner")]
    [Tooltip("CanvasGroup for the fade-in/out victory banner UI.")]
    public CanvasGroup victoryCanvas;
    public TMPro.TMP_Text victoryText;
    public string victoryMessage = "BOSS DEFEATED";
    public float victoryFadeIn = 0.5f;
    public float victoryHold = 1.5f;
    public float victoryFadeOut = 0.8f;

    [Tooltip("SFX to play once when the boss dies.")]
    public AudioClip victorySfx;

    public event Action<BossManager> OnBossDied;

    private Animator anim;
    private bool deathHandled = false;

    private void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();

        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.SetActive(false);

            if (bossHealthSlider != null) bossHealthSlider.maxValue = maxHealth;
            if (bossDelayedHealthSlider != null) bossDelayedHealthSlider.maxValue = maxHealth;
            if (bossNameText != null) bossNameText.text = bossName;

            if (bossHealthSlider != null) bossHealthSlider.value = maxHealth;
            if (bossDelayedHealthSlider != null) bossDelayedHealthSlider.value = maxHealth;
        }

        if (playerManager == null)
            playerManager = FindObjectOfType<PlayerManager>();

        // Ensure victory banner starts hidden
        if (victoryCanvas != null)
        {
            victoryCanvas.alpha = 0f;
            victoryCanvas.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (bossHealthSlider == null) return;

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
        if (deathHandled) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        if (currentHealth <= 0)
        {
            deathHandled = true;

            // reward player
            playerManager?.AddMoney(moneyDrop);

            // death anim
            if (anim != null)
                anim.SetTrigger("Death");

            // run full death flow (banner + sfx + cleanup)
            StartCoroutine(HandleDeathFlow());
        }
    }

    private IEnumerator HandleDeathFlow()
    {
        // drop loot
        SpawnLootIfNeeded();

        // notify listeners
        OnBossDied?.Invoke(this);

        // hide boss UI
        DeactivateBossUI();

        // show victory banner + sfx
        yield return StartCoroutine(ShowVictoryBanner());

        // destroy boss after banner finishes
        Destroy(gameObject);
    }

    private IEnumerator ShowVictoryBanner()
    {
        if (victoryCanvas == null)
            yield break;

        if (victoryText != null)
            victoryText.text = victoryMessage;

        // Play SFX once
        if (AudioManager.Instance != null && victorySfx != null)
            AudioManager.Instance.PlaySFX(victorySfx);

        victoryCanvas.gameObject.SetActive(true);
        victoryCanvas.alpha = 0f;

        float t = 0f;
        while (t < victoryFadeIn)
        {
            t += Time.deltaTime;
            victoryCanvas.alpha = Mathf.Lerp(0f, 1f, t / victoryFadeIn);
            yield return null;
        }

        yield return new WaitForSeconds(victoryHold);

        t = 0f;
        while (t < victoryFadeOut)
        {
            t += Time.deltaTime;
            victoryCanvas.alpha = Mathf.Lerp(1f, 0f, t / victoryFadeOut);
            yield return null;
        }

        victoryCanvas.gameObject.SetActive(false);
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

    public void ForceHideUI() => DeactivateBossUI();

    public bool IsAlive() => currentHealth > 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"Boss hits player for {damage}");
            // hook player damage here if needed
        }
    }
}
