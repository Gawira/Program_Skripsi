using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


namespace UnityStandardAssets.Cameras
{
    public class FreeLookCam : PivotBasedCameraRig
    {
        // This script is designed to be placed on the root object of a camera rig,
        // comprising 3 gameobjects, each parented to the next:

        // 	Camera Rig
        // 		Pivot
        // 			Camera

        [SerializeField] LockOnTarget lockontarget;

        [SerializeField] private float m_MoveSpeed = 1f;                      // How fast the rig will move to keep up with the target's position.
        [Range(0f, 10f)][SerializeField] private float m_TurnSpeed = 1.5f;   // How fast the rig will rotate from user input.
        [SerializeField] private float m_TurnSmoothing = 0.0f;                // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
        [SerializeField] private float m_TiltMax = 75f;                       // The maximum value of the x axis rotation of the pivot.
        [SerializeField] private float m_TiltMin = 45f;                       // The minimum value of the x axis rotation of the pivot.
        [SerializeField] private bool m_LockCursor = false;                   // Whether the cursor should be hidden and locked.
        [SerializeField] private bool m_VerticalAutoReturn = false;           // set wether or not the vertical axis should auto return
        [SerializeField] private float k_LookDistance = 200f;
        [SerializeField] private PauseSetting pauseSetting;
        [SerializeField] private MerchantManager merchantSetting;

        private float m_LookAngle;                    // The rig's y axis rotation.
        private float m_TiltAngle;                    // The pivot's x axis rotation.
        private Vector3 m_PivotEulers;
        private Quaternion m_PivotTargetRot;
        private Quaternion m_TransformTargetRot;
        private float unlockCooldown = 0f;

        // add near top of class (inside the class, not inside methods)


        protected override void Awake()
        {
            base.Awake();


            // Lock or unlock the cursor.
            Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !m_LockCursor;
            m_PivotEulers = m_Pivot.rotation.eulerAngles;

            m_PivotTargetRot = m_Pivot.transform.localRotation;
            m_TransformTargetRot = transform.localRotation;

            if (m_Cam == null)
                m_Cam = GetComponentInChildren<Camera>().transform;

            if (merchantSetting == null)
                merchantSetting = FindObjectOfType<MerchantManager>();
        }


        protected void Update()
        {
            // Check pause state first
            if (pauseSetting != null && pauseSetting.isPaused)
            {
                // Stop camera movement
                return;
            }

            if (merchantSetting != null && merchantSetting.isMerchantOpen)
            {
                // Stop camera movement
                return;
            }

            HandleRotationMovement();
            //Debug.Log($"Tilt={m_TiltAngle}, Look={m_LookAngle}");
            //Debug.Log(
            //            $"[Camera Debug] " +
            //            $"Pos: {m_Pivot.position}, " +
            //            $"Rot: {m_Pivot.rotation.eulerAngles}, " +
            //            $"Forward: {m_Pivot.forward}, " +
            //            $"Right: {m_Pivot.right}, " +
            //            $"Up: {m_Pivot.up}"
            //        );

        }



        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        protected override void FollowTarget(float deltaTime)
        {
            if (m_Target == null) return;
            // Move the rig towards target position.
            transform.position = Vector3.Lerp(transform.position, m_Target.position, deltaTime * m_MoveSpeed);
        }

        public void UpdateCursorState()
        {
            if (pauseSetting != null && pauseSetting.isPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = m_LockCursor ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !m_LockCursor;
            }
        }
        private void HandleRotationMovement()
        {
            if (Time.timeScale < float.Epsilon)
                return;

            // Disable camera input during lock-on
            if (lockontarget != null && lockontarget.LockOn)
            {
                // When locked, camera rotation is handled by LockOnTarget
                return;
            }

            // Read mouse input
            float x = CrossPlatformInputManager.GetAxis("Mouse X");
            float y = CrossPlatformInputManager.GetAxis("Mouse Y");

            // Horizontal look (Y-axis rotation)
            m_LookAngle += x * m_TurnSpeed;

            // Vertical look (X-axis rotation)
            m_TiltAngle -= y * m_TurnSpeed;
            m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);

            // Apply rotations
            m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);
            m_PivotTargetRot = Quaternion.Euler(m_TiltAngle, m_PivotEulers.y, m_PivotEulers.z);

            

            // Adjust camera distance (backwards from pivot)
            m_Cam.localPosition = new Vector3(0f, 0f, -k_LookDistance);

            // Apply rotation smoothing (if any)
            if (m_TurnSmoothing > 0f)
            {
                m_Pivot.localRotation = Quaternion.Slerp(
                    m_Pivot.localRotation,
                    m_PivotTargetRot,
                    m_TurnSmoothing * Time.deltaTime
                );

                transform.localRotation = Quaternion.Slerp(
                    transform.localRotation,
                    m_TransformTargetRot,
                    m_TurnSmoothing * Time.deltaTime
                );
            }
            else
            {
                m_Pivot.localRotation = m_PivotTargetRot;
                transform.localRotation = m_TransformTargetRot;
            }
        }


        public void SyncFromPivot()
        {
            if (m_Pivot == null)
                return;

            // Get the current pivot (X) and rig (Y) rotations
            Vector3 pivotEuler = m_Pivot.localRotation.eulerAngles;
            Vector3 rigEuler = transform.rotation.eulerAngles;

            // Extract yaw (Y-axis) from the rig and pitch (X-axis) from the pivot
            m_LookAngle = rigEuler.y;
            m_TiltAngle = pivotEuler.x;

            // Apply those as new targets so mouse control resumes smoothly
            m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);
            m_PivotTargetRot = Quaternion.Euler(m_TiltAngle, 0f, 0f);

            // Store current Euler state to prevent flipping
            m_PivotEulers = new Vector3(m_TiltAngle, 0f, 0f);

            Debug.Log($"[FreeLookCam] Synced axis-separated — Tilt={m_TiltAngle}, Yaw={m_LookAngle}");
        }

    }
}