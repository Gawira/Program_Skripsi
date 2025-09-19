using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public LockOnTarget lockOnSystem;
    public EnemyManager enemyManager;
   

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyManager.currentHealth <= 0)
        {
            lockOnSystem.LockOn = false;
            lockOnSystem.currentTarget = null;
            enemyManager.Die();
        }
    }
}
