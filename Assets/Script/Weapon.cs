using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public PlayerManager playerManager; // Drag Player (with PlayerManager) here in Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyManager enemy = other.GetComponent<EnemyManager>();
            if (enemy != null)
            {
                int dmg = playerManager.DealDamage();
                enemy.TakeDamage(dmg);
                Debug.Log("Hit enemy for " + dmg + " damage!");
            }
        }
    }
}