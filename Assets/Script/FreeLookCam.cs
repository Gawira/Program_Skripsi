
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


            if (lockontarget != null && lockontarget.LockOn)
            {
                if (m_Pivot != null)
                {
                    Transform pivot = m_Pivot;
                    //Debug.Log(
                    //    $"[Camera Debug] " +
                    //    $"Pos: {pivot.position}, " +
                    //    $"Rot: {pivot.rotation.eulerAngles}, " +
                    //    $"Forward: {pivot.forward}, " +
                    //    $"Right: {pivot.right}, " +
                    //    $"Up: {pivot.up}"
                    //);
                    
                }

                //disables inputs
                //Debug.Log("Lock-on active == disable mouse!");
                return;
            }

            // Read the user input
            var x = CrossPlatformInputManager.GetAxis("Mouse X");
            var y = CrossPlatformInputManager.GetAxis("Mouse Y");
            // Adjust the look angle by an amount proportional to the turn speed and horizontal input.
            m_LookAngle += x * m_TurnSpeed;

            // Rotate the rig (the root object) around Y axis only:
            m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);

            m_Cam.localPosition = new Vector3(0f, 0f, -k_LookDistance);

            if (m_VerticalAutoReturn)
            {
                // For tilt input, we need to behave differently depending on whether we're using mouse or touch input:
                // on mobile, vertical input is directly mapped to tilt value, so it springs back automatically when the look input is released
                // we have to test whether above or below zero because we want to auto-return to zero even if min and max are not symmetrical.
                m_TiltAngle = y > 0 ? Mathf.Lerp(0, -m_TiltMin, y) : Mathf.Lerp(0, m_TiltMax, -y);
            }
            else
            {
                // on platforms with a mouse, we adjust the current angle based on Y mouse input and turn speed
                m_TiltAngle -= y * m_TurnSpeed;
                // and make sure the new value is within the tilt range
                m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);
            }

            // Tilt input around X is applied to the pivot (the child of this object)
            m_PivotTargetRot = Quaternion.Euler(m_TiltAngle, m_PivotEulers.y, m_PivotEulers.z);

            if (m_TurnSmoothing > 0)
            {
                m_Pivot.localRotation = Quaternion.Slerp(m_Pivot.localRotation, m_PivotTargetRot, m_TurnSmoothing * Time.deltaTime);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, m_TransformTargetRot, m_TurnSmoothing * Time.deltaTime);
            }
            else
            {
                m_Pivot.localRotation = m_PivotTargetRot;
                transform.localRotation = m_TransformTargetRot;
            }
        }

        public void SyncFromPivot()
        {
            if (m_Pivot == null || lockontarget == null) return;

            //  Use the saved pivot rotation directly
            Quaternion spr = lockontarget.savedPivotRotation;

            // Convert to Euler for tilt/look separation
            Vector3 pivotEuler = spr.eulerAngles;

            //  Use saved rotation instead of live transform
            m_TiltAngle = pivotEuler.x;                      // X = tilt
            m_LookAngle = pivotEuler.y;                      // Y = look

            //  Apply saved rotation as targets
            m_PivotTargetRot = Quaternion.Euler(m_TiltAngle, 0f, 0f);
            m_TransformTargetRot = Quaternion.Euler(0f, m_LookAngle, 0f);

            Debug.Log($"[FreeLookCam] Synced from saved rotation: Tilt={m_TiltAngle}, Look={m_LookAngle}");
        }
    }
}
