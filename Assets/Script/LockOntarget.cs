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
        [SerializeField] public PlayerManager playerManager;

        [Header("Runtime")]
        public Transform currentTarget;
        private Camera mainCam;
        public bool LockOn;

        public Quaternion savedPivotRotation;

        private Vector3 lastTargetDirection;

        void Start()
        {
            // Use freelook camera if available, else fallback
            mainCam = freelookcam != null && freelookcam.m_Cam != null
                ? freelookcam.m_Cam.GetComponent<Camera>()
                : Camera.main;

            // Subscribe to existing enemies
            foreach (EnemyManager enemy in FindObjectsOfType<EnemyManager>())
                enemy.OnEnemyDied += HandleEnemyDeath;

            // Subscribe to existing bosses
            foreach (BossManager boss in FindObjectsOfType<BossManager>())
                boss.OnBossDied += HandleBossDeath;

            if (playerManager == null)
                playerManager = FindObjectOfType<PlayerManager>();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (currentTarget != null)
                    UnlockTarget();
                else
                    LockOntoNewTarget();
            }

            LockOn = currentTarget != null;

            if (LockOn)
                LockOnToTarget();

            if (playerManager.isDead)
            {
                LockOn = false;
                UnlockTarget(); 
            }

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
        public void UnlockTarget()
        {
            if (freelookcam == null || freelookcam.m_Pivot == null)
                return;

            Transform rig = freelookcam.transform;
            Transform pivot = freelookcam.m_Pivot;

            // Keep facing the same direction you were during the lock
            Vector3 direction = lastTargetDirection != Vector3.zero
                ? lastTargetDirection
                : rig.forward;

            Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);
            Vector3 euler = lookRot.eulerAngles;

            // Apply same split as before
            rig.rotation = Quaternion.Euler(0f, euler.y, 0f);
            pivot.localRotation = Quaternion.Euler(euler.x, 0f, 0f);

            // Save the pivot for smooth manual control resync
            savedPivotRotation = pivot.localRotation;
            currentTarget = null;
            LockOn = false;

            // Resync FreeLookCam logic after restoring the split rotations
            freelookcam.SyncFromPivot();
        }

        public void LockOntoNewTarget()
        {
            currentTarget = FindVisibleTarget();
            LockOn = currentTarget != null;
        }

        public void LockOnToTarget()
        {
            if (!currentTarget || freelookcam == null) return;

            Transform rig = freelookcam.transform;     // Handles Y (yaw)
            Transform pivot = freelookcam.m_Pivot;     // Handles X (pitch)
            Transform cam = freelookcam.m_Cam;         // For direction reference

            // Direction from pivot to target
            Vector3 dirToTarget = currentTarget.position - pivot.position;
            lastTargetDirection = dirToTarget.normalized;

            // Compute desired rotation
            Quaternion lookRot = Quaternion.LookRotation(dirToTarget.normalized, Vector3.up);
            Vector3 euler = lookRot.eulerAngles;

            // Separate axes
            float targetYaw = euler.y;   // Horizontal rotation (Y)
            float targetPitch = euler.x; // Vertical tilt (X)

            // Smoothly rotate Y-axis on the root
            Quaternion rigTargetRot = Quaternion.Euler(0f, targetYaw, 0f);
            rig.rotation = Quaternion.Slerp(
                rig.rotation,
                rigTargetRot,
                rotationSpeed * Time.deltaTime
            );

            // Smoothly rotate X-axis on the pivot
            Quaternion pivotTargetRot = Quaternion.Euler(targetPitch, 0f, 0f);
            pivot.localRotation = Quaternion.Slerp(
                pivot.localRotation,
                pivotTargetRot,
                rotationSpeed * Time.deltaTime
            );

            // Save for unlocking stabilization
            savedPivotRotation = pivot.localRotation;
        }

        public Transform FindVisibleTarget()
        {
            Transform bestTarget = null;
            float closestAngle = 30f;

            // Prefer bosses
            foreach (BossManager boss in FindObjectsOfType<BossManager>())
            {
                if (!boss.IsAlive()) continue;
                if (TryCandidate(boss.transform, ref bestTarget, ref closestAngle)) continue;
            }

            // Then normal enemies
            foreach (EnemyManager enemy in FindObjectsOfType<EnemyManager>())
            {
                if (!enemy.IsAlive()) continue;
                TryCandidate(enemy.transform, ref bestTarget, ref closestAngle);
            }

            return bestTarget;
        }

        private bool TryCandidate(Transform candidate, ref Transform bestTarget, ref float closestAngle)
        {
            if (freelookcam == null || freelookcam.m_Cam == null)
                return false;

            Camera cam = freelookcam.m_Cam.GetComponent<Camera>();
            if (cam == null)
                return false;

            // Use freelook camera position/orientation instead of mainCam
            Vector3 viewportPos = cam.WorldToViewportPoint(candidate.position);
            if (viewportPos.z <= 0 || viewportPos.x <= 0 || viewportPos.x >= 1 || viewportPos.y <= 0 || viewportPos.y >= 1)
                return false;

            float distance = Vector3.Distance(transform.position, candidate.position);
            if (distance > lockOnRange) return false;

            Vector3 dirToTarget = candidate.position - cam.transform.position;
            float angle = Vector3.Angle(cam.transform.forward, dirToTarget);

            if (angle < closestAngle)
            {
                closestAngle = angle;
                bestTarget = candidate;
            }
            return true;
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

                if (freelookcam.m_Pivot != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(freelookcam.m_Pivot.position, currentTarget.position);
                }
            }
        }
        #endregion
    }
}
