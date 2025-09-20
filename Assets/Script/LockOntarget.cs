using System;
using UnityEditor;
using UnityEngine;
using UnityStandardAssets.Cameras;

namespace UnityStandardAssets.Cameras
{
    public class LockOnTarget : MonoBehaviour
    {
        [Header("Lock-On Settings")]
        [SerializeField] private string enemyTag = "Enemy";   // Tag for enemies
        [SerializeField] private float lockOnRange = 50f;     // Max lock-on distance
        [SerializeField] private float rotationSpeed = 5f;    // Smooth turning
        [SerializeField] private FreeLookCam freelookcam;

        [Header("Runtime")]
        public Transform currentTarget;                      // Currently locked enemy
        private Camera mainCam;
        public bool LockOn;

        public Vector3 savedPivot;

        public Quaternion savedPivotRotation;


        void Start()
        {
            mainCam = Camera.main;

            // Subscribe to all existing enemies
            foreach (EnemyManager enemy in FindObjectsOfType<EnemyManager>())
            {
                enemy.OnEnemyDied += HandleEnemyDeath;
            }
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
                

                LockOnToTarget();
                
                LockOn = true;

                
                
            }
            else
            {
                LockOn = false;
            }
            Debug.Log($"Saved Pivot Rotation: {savedPivotRotation.eulerAngles}");
        }
        void LateUpdate()
        {
            if (currentTarget != null)
            {
                LockOnToTarget();
            }
        }


        private void HandleEnemyDeath(EnemyManager deadEnemy)
        {
            if (currentTarget == deadEnemy.transform)
            {
                currentTarget = FindVisibleEnemy();
                LockOn = currentTarget != null;
                
                if (!LockOn && freelookcam != null)
                {
                    freelookcam.SyncFromPivot();

                }
            }
        }


        #region 🔹 Lock-On Methods
        private void UnlockTarget()
        {
            if (freelookcam != null)
            {
                freelookcam.SyncFromPivot();
            }

            currentTarget = null;
            LockOn = false;
        }

        private void LockOntoNewTarget()
        {
            currentTarget = FindVisibleEnemy();
            LockOn = currentTarget != null;
        }

        public Transform FindVisibleEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            Transform bestTarget = null;
            float closestAngle = 30f; // Only lock enemies within ~30° of view

            foreach (GameObject enemy in enemies)
            {
                EnemyManager e = enemy.GetComponent<EnemyManager>();
                if (e == null || e.currentHealth <= 0) continue; // Skip dead ones

                Vector3 viewportPos = mainCam.WorldToViewportPoint(enemy.transform.position);

                // Check if enemy is in camera view
                if (viewportPos.z > 0 &&
                    viewportPos.x > 0 && viewportPos.x < 1 &&
                    viewportPos.y > 0 && viewportPos.y < 1)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance > lockOnRange) continue;

                    Vector3 dirToEnemy = enemy.transform.position - mainCam.transform.position;
                    float angle = Vector3.Angle(mainCam.transform.forward, dirToEnemy);

                    if (angle < closestAngle)
                    {
                        closestAngle = angle;
                        bestTarget = enemy.transform;
                    }
                }
            }

            return bestTarget;
        }

        public void LockOnToTarget()
        {
            if (!currentTarget || freelookcam == null) return;

            Transform pivot = freelookcam.m_Pivot;

            // Direction from pivot to target
            Vector3 dirToTarget = currentTarget.position - pivot.position;

            // Ignore vertical difference (optional)
            dirToTarget.y = -2f;

            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);

            // Smooth rotate pivot
            pivot.rotation = Quaternion.Slerp(
                pivot.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Save the pivot’s rotation so you can reuse it later
            savedPivotRotation = pivot.rotation;

            
        }
        #endregion

        #region 🔹 Debug Gizmos
        void OnDrawGizmos()
        {
            if (!Application.isPlaying || freelookcam == null) return;

            if (freelookcam.m_Pivot != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(freelookcam.m_Pivot.position, 0.2f);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(freelookcam.m_Pivot.position, freelookcam.m_Pivot.forward * 2f);
            }

            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(currentTarget.position, 0.3f);

                Gizmos.color = Color.yellow;
                if (freelookcam.m_Pivot != null)
                {
                    Gizmos.DrawLine(freelookcam.m_Pivot.position, currentTarget.position);
                }
            }
        }
        #endregion
    }
}
