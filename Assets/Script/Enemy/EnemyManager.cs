using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int moneyDrop = 0;
    public int damage = 10;

    [Header("Player Tag")]
    public string playerTag = "Player";
    public event Action<EnemyManager> OnEnemyDied;
    private Animator anim;

    private GameObject healthBarCanvas;
    private LockOnTarget lockOnTarget;
    private PlayerManager playerManager;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();

        if (healthBarCanvas == null)
            healthBarCanvas = GetComponentInChildren<Canvas>(true)?.gameObject;

        if (lockOnTarget == null)
            lockOnTarget = FindObjectOfType<LockOnTarget>();

        if (playerManager == null)
            playerManager = FindObjectOfType<PlayerManager>();
    }

    private void Update()
    {
        //Debug.Log(lockOnTarget.LockOn);
        if (healthBarCanvas != null && lockOnTarget.LockOn == true || currentHealth != maxHealth)
        {
            //Debug.Log("helpme");
            healthBarCanvas.SetActive(true);
        }
        else
        {  
            healthBarCanvas.SetActive(false);
        }
    }

    // Public method to get current health (for LockOnTarget system)
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Public method to get health percentage (useful for UI)
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    // Public method to check if enemy is alive
    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0); // Prevent negative health

        if (currentHealth <= 0)
        {
            // Give player money
            playerManager.AddMoney(moneyDrop);
            Die();
            
        }
    }
    public void Die()
    {
        // Optional: play death animation
        if (anim != null)
            anim.SetTrigger("Die");

        

        OnEnemyDied?.Invoke(this);

        // Destroy after small delay (so death anim plays)
        Destroy(gameObject, 1f);       
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // Example: damage player if you have a PlayerHealth script
            // PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            // if (playerHealth != null)
            // {
            //     playerHealth.TakeDamage(damage);
            // }

            Debug.Log("Enemy hit the player!");
        }
    }
}