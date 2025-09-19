using System;
using UnityEngine;
using UnityStandardAssets.Cameras;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        
        private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object
        private Transform m_Cam;                  // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_Move;
        private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.
        [SerializeField] private LockOnTarget lockOnSystem;
       
        Animator m_Animator;

        private void Start()
        {
            m_Animator = GetComponent<Animator>();
            // get the transform of the main camera
            if (Camera.main != null)
            {
                m_Cam = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
            
        }


        private void Update()
        {
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
                if (Input.GetKeyDown(KeyCode.Space))
                {
                // Only dash if we have some movement input
                    if (m_Move != Vector3.zero)
                    {
                       float dashDistance = 3f;    // How far to dash
                       float dashSpeed = 10f;      // How fast to dash

                        // Calculate dash target
                        Vector3 dashTarget = transform.position + m_Move.normalized * dashDistance;

                        Debug.Log("dashed");
                        // Instantly move or lerp over time
                        StartCoroutine(DashTo(dashTarget, dashSpeed));
                    }
                }
            }

        }

        private System.Collections.IEnumerator DashTo(Vector3 target, float speed)
        {
            // Optionally disable character movement during dash
            float distance = Vector3.Distance(transform.position, target);
            while (distance > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                distance = Vector3.Distance(transform.position, target);
                yield return null;
            }
        }

        // Fixed update is called in sync with physics
        public void FixedUpdate()
        {
            {
                float h = CrossPlatformInputManager.GetAxis("Horizontal");
                float v = CrossPlatformInputManager.GetAxis("Vertical");
                bool crouch = Input.GetKey(KeyCode.C);

                m_Animator.SetFloat("Horizontal", h);
                m_Animator.SetFloat("Vertical", v);

                // Check lock-on state
                if (lockOnSystem != null && lockOnSystem.LockOn && lockOnSystem.transform != null)
                {
                    Transform target = lockOnSystem.currentTarget;

                    
                    // Arah ke musuh
                    Vector3 directionToEnemy = (target.position - transform.position).normalized;
                    directionToEnemy.y = 0f;

                    // Tetap hadap ke musuh
                    transform.rotation = Quaternion.LookRotation(directionToEnemy);

                    // Gerak horizontal/vertical relatif terhadap musuh
                    Vector3 right = Vector3.Cross(Vector3.up, directionToEnemy);
                    Vector3 forward = Vector3.Cross(right, Vector3.up); // sama aja dengan directionToEnemy tapi lebih akurat

                    // Pergerakan strafe
                    m_Move = h * right + v * forward;

                }
                else
                {
                    if (m_Cam != null)
                    {
                        m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
                        m_Move = v * m_CamForward + h * m_Cam.right;
                    }
                    else
                    {
                        m_Move = v * Vector3.forward + h * Vector3.right;
                    }
                }

#if !MOBILE_INPUT
                if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif
                m_Character.Move(m_Move, crouch, m_Jump);
                m_Jump = false;
            }
        }
    }
}
