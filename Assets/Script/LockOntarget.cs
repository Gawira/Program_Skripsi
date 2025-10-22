
using System;
using UnityEngine;
using UnityStandardAssets.Cameras;

namespace UnityStandardAssets.Cameras
{
    public class LockOnTarget : MonoBehaviour
    {
        [Header("Lock-On Settings")]
        [SerializeField] private string enemyTag = "Enemy";
        [SerializeField] private string bossTag = "Boss";
        [SerializeField] private float lockOnRange = 50f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private FreeLookCam freelookcam;

        [Header("Runtime")]
        public Transform currentTarget;
        private Camera mainCam;
        public bool LockOn;

        public Quaternion savedPivotRotation;

        void Start()
        {
            mainCam = Camera.main;

            // Subscribe to existing enemies
            foreach (EnemyManager enemy in FindObjectsOfType<EnemyManager>())
            {
                enemy.OnEnemyDied += HandleEnemyDeath;
            }

            // Subscribe to existing bosses
            foreach (BossManager boss in FindObjectsOfType<BossManager>())
            {
                boss.OnBossDied += HandleBossDeath;
            }
        }

        void Update()
        {
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

            LockOn = currentTarget != null;

            if (LockOn)
                LockOnToTarget();
        }

        void LateUpdate()
        {
            if (currentTarget != null)
                LockOnToTarget();
        }

        #region 🔸 Target Death Handlers
        private void HandleEnemyDeath(EnemyManager deadEnemy)
        {
            if (currentTarget == deadEnemy.transform)
            {
                currentTarget = FindVisibleTarget();
                LockOn = currentTarget != null;
                if (!LockOn && freelookcam != null)
                    freelookcam.SyncFromPivot();
            }
        }

        private void HandleBossDeath(BossManager deadBoss)
        {
            if (currentTarget == deadBoss.transform)
            {
                currentTarget = FindVisibleTarget();
                LockOn = currentTarget != null;
                if (!LockOn && freelookcam != null)
                    freelookcam.SyncFromPivot();
            }
        }
        #endregion

        #region 🔸 Lock-On Logic
        private void UnlockTarget()
        {
            if (freelookcam != null)
                freelookcam.SyncFromPivot();

            if (freelookcam != null && freelookcam.m_Pivot != null)
            {
                // Align pivot so that camera faces the same direction again
                Vector3 camForward = -freelookcam.m_Cam.forward;
                Quaternion correctedRot = Quaternion.LookRotation(camForward, Vector3.up);
                freelookcam.m_Pivot.rotation = correctedRot;
                savedPivotRotation = correctedRot;
            }

            currentTarget = null;
            LockOn = false;
        }

        private void LockOntoNewTarget()
        {
            currentTarget = FindVisibleTarget();
            LockOn = currentTarget != null;
        }

        public Transform FindVisibleTarget()
        {
            Transform bestTarget = null;
            float closestAngle = 30f;

            // 🔹 Check Bosses first (priority)
            foreach (BossManager boss in FindObjectsOfType<BossManager>())
            {
                if (!boss.IsAlive()) continue;
                if (TryCandidate(boss.transform, ref bestTarget, ref closestAngle)) continue;
            }

            // 🔹 Check Enemies
            foreach (EnemyManager enemy in FindObjectsOfType<EnemyManager>())
            {
                if (!enemy.IsAlive()) continue;
                TryCandidate(enemy.transform, ref bestTarget, ref closestAngle);
            }

            return bestTarget;
        }

        private bool TryCandidate(Transform candidate, ref Transform bestTarget, ref float closestAngle)
        {
            Vector3 viewportPos = mainCam.WorldToViewportPoint(candidate.position);
            if (viewportPos.z <= 0 || viewportPos.x <= 0 || viewportPos.x >= 1 || viewportPos.y <= 0 || viewportPos.y >= 1)
                return false;

            float distance = Vector3.Distance(transform.position, candidate.position);
            if (distance > lockOnRange) return false;

            Vector3 dirToTarget = candidate.position - mainCam.transform.position;
            float angle = Vector3.Angle(mainCam.transform.forward, dirToTarget);

            if (angle < closestAngle)
            {
                closestAngle = angle;
                bestTarget = candidate;
            }
            return true;
        }

        public void LockOnToTarget()
        {
            if (!currentTarget || freelookcam == null) return;

            Transform pivot = freelookcam.m_Pivot;

            Vector3 dirToTarget = currentTarget.position - pivot.position;
            dirToTarget.y = -2f; // Optional offset

            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);

            pivot.rotation = Quaternion.Slerp(
                pivot.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            Vector3 camForward = freelookcam.m_Cam.forward;
            savedPivotRotation = Quaternion.LookRotation(camForward, Vector3.up);
        }
        #endregion

        #region 🔸 Gizmos
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