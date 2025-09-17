using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public EnemyManager enemyManager; // Drag Player (with PlayerManager) here in Inspector
    public PlayerManager playerManager;
    

    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Example: damage player if you have a PlayerHealth script
            //PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
           
            playerManager.TakeDamage(enemyManager.damage);
            
            // Or you can do other logic like knockback, debug log, etc.
            Debug.Log("Enemy hit the player!");
            Debug.Log(enemyManager.damage);
        }
    }
}