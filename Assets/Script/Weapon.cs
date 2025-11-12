using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public PlayerManager playerManager; // Drag Player (with PlayerManager)

    [Header("Double Damage")]
    public bool isDoubleDamage = false;           // <-- toggle in Inspector / via code
    public float doubleDamageMultiplier = 2f;     // 2x by default
    public float defaultDoubleDamageDuration = 5f;

    public AudioClip doubleDamageStartSfx;        // optional
    public AudioClip doubleDamageEndSfx;          // optional

    // True if the most recent hit used double damage
    public bool LastHitWasDouble { get; private set; }

    private void Reset()
    {
        if (!playerManager) playerManager = FindObjectOfType<PlayerManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        int dmg = ComputeDamage(out bool usedDouble);

        // Prefer one or the other; most enemies shouldn’t have both
        if (other.TryGetComponent(out EnemyManager enemy))
        {
            enemy.TakeDamage(dmg);
            Debug.Log($"Hit enemy for {dmg} damage{(usedDouble ? " (DOUBLE)" : "")}!");
        }
        else if (other.TryGetComponent(out BossManager boss))
        {
            boss.TakeDamage(dmg);
            Debug.Log($"Hit boss for {dmg} damage{(usedDouble ? " (DOUBLE)" : "")}!");
        }
    }

    private int ComputeDamage(out bool usedDouble)
    {
        int dmg = playerManager != null ? playerManager.DealDamage() : 0;
        usedDouble = isDoubleDamage;
        if (usedDouble)
            dmg = Mathf.RoundToInt(dmg * doubleDamageMultiplier);

        LastHitWasDouble = usedDouble;
        return dmg;
    }

    // --- Public API ---

    /// <summary>Hard toggle.</summary>
    public void SetDoubleDamage(bool state) => isDoubleDamage = state;

    /// <summary>Enable double damage for the default duration.</summary>
    public void ActivateDoubleDamage() => StartCoroutine(DoubleDamageForSeconds(defaultDoubleDamageDuration));

    /// <summary>Enable double damage for a custom duration (seconds).</summary>
    public void ActivateDoubleDamage(float seconds) => StartCoroutine(DoubleDamageForSeconds(seconds));

    private IEnumerator DoubleDamageForSeconds(float seconds)
    {
        isDoubleDamage = true;
        if (doubleDamageStartSfx && AudioManager.Instance) AudioManager.Instance.PlaySFX(doubleDamageStartSfx);

        yield return new WaitForSeconds(seconds);

        isDoubleDamage = false;
        if (doubleDamageEndSfx && AudioManager.Instance) AudioManager.Instance.PlaySFX(doubleDamageEndSfx);
    }
}
