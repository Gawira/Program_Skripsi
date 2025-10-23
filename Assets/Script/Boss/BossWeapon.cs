using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class BossWeapon : MonoBehaviour
{
    [Header("References")]
    public BossManager bossManager;
    public PlayerManager playerManager;

    [Header("Knockback Settings")]
    public float knockbackForce = 150f;
    public float knockbackUpward = 30f;

    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 0.3f;
    private float lastDamageTime = -Mathf.Infinity;

    private void Start()
    {
        if (playerManager == null)
            playerManager = FindObjectOfType<PlayerManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (Time.time >= lastDamageTime + damageCooldown)
        {
            // Deal damage to player
            playerManager.TakeDamage(bossManager.damage);

            // Knockback effect
            TPCharacter player = other.GetComponent<TPCharacter>();
            if (player != null)
            {
                Vector3 knockbackDir = (player.transform.position - transform.position).normalized;
                knockbackDir.y = 0;
                player.Knockback(knockbackDir, knockbackForce, knockbackUpward);
            }

            Debug.Log($"Boss hit the player for {bossManager.damage} damage!");

            lastDamageTime = Time.time;
        }
    }
}
