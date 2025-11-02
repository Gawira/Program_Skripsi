using System.Collections;
using UnityEngine;
using UnityStandardAssets.Cameras;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(TPCharacter))]
    public class TPUserControl : MonoBehaviour
    {
        private TPCharacter m_Character;
        private Transform m_Cam;
        private Vector3 m_CamForward;
        private Vector3 m_Move;
        private bool m_Jump;

        [SerializeField] private LockOnTarget lockOnSystem;

        [Header("Dash")]
        [SerializeField] private KeyCode dashKey = KeyCode.Space;
        [SerializeField] private float dashForce = 15f;
        [SerializeField] private float dashTime = 0.2f;
        [SerializeField] private float dashCooldown = 0.5f;
        [SerializeField] private float dashRayLength = 0.6f;
        [SerializeField] private LayerMask dashCollisionMask = Physics.DefaultRaycastLayers;

        [Header("Ground gate")]
        [SerializeField] private bool requireGroundedToDash = true;
        [SerializeField] private float groundedProbeRadius = 0.2f;
        [SerializeField] private float groundedProbeDistance = 0.25f;
        [SerializeField] private Vector3 groundedProbeOffset = new Vector3(0f, 0.1f, 0f);

        [Header("Debug")]
        [SerializeField] private bool drawGroundProbeGizmos = true;

        public bool isDashing = false;
        private bool canDash = true;

        private Rigidbody rb;
        private Animator m_Animator;
        public PlayerManager playermanager;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            m_Animator = GetComponent<Animator>();
            m_Cam = Camera.main ? Camera.main.transform : null;

            m_Character = GetComponent<TPCharacter>();

            // don’t hit our own layer when ray-testing during dash
            dashCollisionMask &= ~(1 << gameObject.layer);
        }

        private void Update()
        {
            if (!m_Jump)
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");

            if (Input.GetKeyDown(dashKey))
                TryStartDash();
        }

        private void TryStartDash()
        {
            if (isDashing || !canDash) return;
            if (requireGroundedToDash && !IsGroundedNow()) return;

            Vector3 dir = m_Move.sqrMagnitude > 0.001f ? m_Move : transform.forward;
            StartCoroutine(DashRoutine(dir));
        }

        private IEnumerator DashRoutine(Vector3 direction)
        {
            isDashing = true;
            canDash = false;

            if (m_Animator) m_Animator.SetTrigger("Dash");

            direction.y = 0f;
            direction.Normalize();

            float elapsed = 0f;
            while (elapsed < dashTime)
            {
                Vector3 origin = transform.position + Vector3.up * 0.2f;
                if (Physics.Raycast(origin, direction, out _, dashRayLength, dashCollisionMask, QueryTriggerInteraction.Ignore))
                    break;

                rb.velocity = direction * dashForce;
                elapsed += Time.deltaTime;
                yield return null;
            }

            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            isDashing = false;
            playermanager?.SetVulnerable();

            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }

        private bool IsGroundedNow()
        {
            Vector3 origin = transform.position + groundedProbeOffset;
            return Physics.SphereCast(origin, groundedProbeRadius, Vector3.down,
                                      out _, groundedProbeDistance, ~0, QueryTriggerInteraction.Ignore);
        }

        public void FixedUpdate()
        {
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);

            if (m_Animator)
            {
                m_Animator.SetFloat("Horizontal", h);
                m_Animator.SetFloat("Vertical", v);
            }

            if (lockOnSystem != null && lockOnSystem.LockOn && lockOnSystem.currentTarget != null)
            {
                Vector3 toEnemy = (lockOnSystem.currentTarget.position - transform.position).normalized; toEnemy.y = 0f;
                transform.rotation = Quaternion.LookRotation(toEnemy);
                Vector3 right = Vector3.Cross(Vector3.up, toEnemy);
                Vector3 fwd = Vector3.Cross(right, Vector3.up);
                m_Move = h * right + v * fwd;
            }
            else
            {
                if (m_Cam)
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
            if (Input.GetKey(KeyCode.RightShift)) m_Move *= 0.5f;
#endif
            m_Character.Move(m_Move, crouch, m_Jump);
            m_Jump = false;
        }

        // ---------- Gizmos: Ground Probe Visual ----------
        private void OnDrawGizmos()
        {
            if (!drawGroundProbeGizmos) return;

            // Use current values even in edit mode
            Vector3 origin = transform.position + groundedProbeOffset;
            float r = groundedProbeRadius;
            float d = groundedProbeDistance;

            // Cast for visualization only
            bool hitSomething = Physics.SphereCast(origin, r, Vector3.down, out RaycastHit hit, d, ~0, QueryTriggerInteraction.Ignore);

            // Origin sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin, r);

            // Line of the probe
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + Vector3.down * d);

            if (hitSomething)
            {
                // Hit point sphere (GREEN when grounded)
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(hit.point, r * 0.9f);
            }
            else
            {
                // End sphere (RED when not grounded)
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(origin + Vector3.down * d, r);
            }
        }
    }
}
