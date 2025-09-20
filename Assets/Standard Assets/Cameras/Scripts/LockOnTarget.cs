using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityStandardAssets.Cameras
{
    public class LockOntarget : MonoBehaviour
    {

        [SerializeField] private string enemyTag = "Enemy";   // Tag for enemies
        [SerializeField] private float lockOnRange = 50f;     // Max lock-on distance
        [SerializeField] private float rotationSpeed = 5f;    // Smooth turning
        [SerializeField] FreeLookcam freelookcam;
        

        public Transform currentTarget;                     // Currently locked enemy
        private Camera mainCam;
        public Boolean LockOn = false;
        

        void Start()
        {
            mainCam = Camera.main;
            
        }

        void Update()
        {
            // Toggle lock-on with Right Mouse Button
            if (Input.GetMouseButtonDown(1))
            {
                if (currentTarget != null)
                {
                    UnlockTarget();
                }
                else
                {
                    LockOntoNewTarget();
                }
            }

            // If currently locked
            if (currentTarget != null)
            {
                //// Check if target is dead
                //EnemyManager enemy = currentTarget.GetComponent<EnemyManager>();
                //if (enemy != null && enemy.currentHealth <= 0)
                //{
                //    UnlockTarget();
                //    LockOntoNewTarget(); // Try to find the next enemy
                //    return;
                //}

                //// Keep locking onto target
                //LockOnToTarget();
                //LockOn = true;
            }
            else
            {
                LockOn = false;
            }
        }

        private void UnlockTarget()
        {
            currentTarget = null;
            LockOn = false;
        }

        private void LockOntoNewTarget()
        {
            currentTarget = FindVisibleEnemy();
            if (currentTarget != null)
            {
                LockOn = true;
            }
            else
            {
                LockOn = false;
            }
        }

        public Transform FindVisibleEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            Transform bestTarget = null;
            float closestAngle = 30f; // Only lock enemies within ~30° of view

            foreach (GameObject enemy in enemies)
            {
                Vector3 viewportPos = mainCam.WorldToViewportPoint(enemy.transform.position);

                // Check if enemy is in front of camera (0<viewport<1)
                if (viewportPos.z > 0 &&
                    viewportPos.x > 0 && viewportPos.x < 1 &&
                    viewportPos.y > 0 && viewportPos.y < 1)
                //Debug.Log("visible found");
                {
                    // Check distance
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance > lockOnRange) continue;

                    // Check angle (how close to center of screen)
                    Vector3 dirToEnemy = enemy.transform.position - mainCam.transform.position;
                    float angle = Vector3.Angle(mainCam.transform.forward, dirToEnemy);

                    if (angle < closestAngle)
                    {
                        closestAngle = angle;
                        bestTarget = enemy.transform;
                        //Debug.Log(bestTarget);
                    }
                }
            }

            return bestTarget;
        }

        void LockOnToTarget()
        {
            if (!currentTarget || freelookcam == null) return;

            // 1️ Get the pivot transform from FreeLookCam
            Transform pivot = freelookcam.m_Pivot;

            // 2️ Calculate the direction from pivot to the enemy
            Vector3 dirToTarget = currentTarget.position - pivot.position;

            // 3️ Get the desired rotation that looks at the target
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);

            // 4️ Smoothly rotate the pivot toward the target
            pivot.rotation = Quaternion.Slerp(
                pivot.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        void OnDrawGizmos()
        {
            // Only draw gizmos when in play mode and we have a FreeLookCam
            if (!Application.isPlaying || freelookcam == null) return;

            // Draw the camera pivot position
            if (freelookcam.m_Pivot != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(freelookcam.m_Pivot.position, 0.2f);

                // Show pivot forward direction
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(freelookcam.m_Pivot.position, freelookcam.m_Pivot.forward * 2f);
            }

            // Draw current target debug
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(currentTarget.position, 0.3f);

                // Draw a line from pivot → target
                Gizmos.color = Color.yellow;
                if (freelookcam.m_Pivot != null)
                {
                    Gizmos.DrawLine(freelookcam.m_Pivot.position, currentTarget.position);
                }
            }
        }
    }
}