using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class EnemyWeapon : MonoBehaviour
{
    public EnemyManager enemyManager; // Drag Player (with PlayerManager) here in Inspector
    public PlayerManager playerManager;
    

    public float knockbackForce = 10f;   // Strength of knockback
    public float knockbackUpward = 2f;

    void Start()
    {
        
    }

    void Update()
    {
       // Debug.Log(canDamage);
    }

    private bool canDamage = true;
    [SerializeField] private float damageCooldown = 0.2f; // seconds between hits

    private float lastDamageTime = -Mathf.Infinity;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Check cooldown
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                playerManager.TakeDamage(enemyManager.damage);

                TPCharacter player = other.GetComponent<TPCharacter>();

                Vector3 knockbackDir = (player.transform.position - transform.position).normalized;
                knockbackDir.y = 0;

                player.Knockback(knockbackDir, knockbackForce, knockbackUpward);

                Debug.Log("Enemy hit the player!");
                Debug.Log(enemyManager.damage);

                // Update last damage time
                lastDamageTime = Time.time;
            }
        }
    }
}