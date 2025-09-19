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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerManager.TakeDamage(enemyManager.damage);

            TPCharacter player = other.GetComponent<TPCharacter>();

            Vector3 knockbackDir = (player.transform.position - transform.position).normalized;
            knockbackDir.y = 0;

            player.Knockback(knockbackDir, knockbackForce, knockbackUpward);

            

            // Or you can do other logic like knockback, debug log, etc.
            Debug.Log("Enemy hit the player!");
            Debug.Log(enemyManager.damage);
        }
    }
}