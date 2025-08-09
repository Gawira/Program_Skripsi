using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityStandardAssets.Cameras
{
    internal class LockOnTarget : MonoBehaviour
    {
        [SerializeField] private string enemyTag = "Enemy";   // Tag for enemies
        [SerializeField] private float lockOnRange = 50f;     // Max lock-on distance
        [SerializeField] private float rotationSpeed = 5f;    // Smooth turning

        private Transform currentTarget;                     // Currently locked enemy
        private Camera mainCam;
        private FreeLookCam freelookcam;
        public Boolean LockOn;


        void Start()
        {
            mainCam = Camera.main;
            freelookcam = GetComponent<FreeLookCam>();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(2)) // Mouse scroll click
            {
                // If already locked, unlock
                if (currentTarget != null)
                {
                    currentTarget = null;
                    LockOn = false;
                    Debug.Log(LockOn);
                }
                else
                {
                    currentTarget = FindVisibleEnemy();

                }
            }

            if (currentTarget != null)
            {
                LockOnToTarget();
                LockOn = true;
                Debug.Log(LockOn);
            }
        }

        Transform FindVisibleEnemy()
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
                        Debug.Log(bestTarget);
                    }
                }
            }

            return bestTarget;
        }

        void LockOnToTarget()
        {
            if (!currentTarget) return;

            // Calculate direction to enemy
            Vector3 dirToTarget = currentTarget.position - transform.position;
            dirToTarget.y = 0; // keep camera level
            //Debug.Log("lock on found");

            // Smoothly rotate the camera rig toward target
            Quaternion lookRot = Quaternion.LookRotation(dirToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);

            // OPTIONAL: Also tilt pivot vertically toward enemy
            Vector3 pivotDir = currentTarget.position - freelookcam.m_Pivot.position;
            Quaternion pivotLookRot = Quaternion.LookRotation(pivotDir);
            freelookcam.m_Pivot.rotation = Quaternion.Slerp(freelookcam.m_Pivot.rotation, pivotLookRot, rotationSpeed * Time.deltaTime);
        }
    }
}