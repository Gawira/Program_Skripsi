using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 100;
    private int currentHealth;

    public int moneyDrop = 50;
    public int damage = 10;

    [Header("Player Tag")]
    public string playerTag = "Player";

    // Reference to animator if you want death anim
    private Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
        
    }

    private void Die()
    {
        // Optional: play death animation
        if (anim != null)
            anim.SetTrigger("Die");

        // Give player money
        //PlayerManager.Instance.AddMoney(moneyDrop);

        // Destroy after small delay (so death anim plays)
        Destroy(gameObject, 2f);
    }

    // ===============================
    // Collision with Player
    // ===============================
    private void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag(playerTag))
        //{
        //    // Example: damage player if you have a PlayerHealth script
        //    //PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        //    if (playerHealth != null)
        //    {
        //        playerHealth.TakeDamage(damage);
        //    }

        //    // Or you can do other logic like knockback, debug log, etc.
        //    Debug.Log("Enemy hit the player!");
        //}
    }
}