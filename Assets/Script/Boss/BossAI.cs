using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public float chaseRadius = 40f;
    public float attackRadius = 15f;
    public string playerTag = "Player";
    public string bossName = "name";

    [Header("Decision Settings")]
    public float decisionInterval = 0.5f;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float strafeSpeed = 4f;
    public float rotationSpeed = 8f;

    [Tooltip("How far down we raycast to find ground height.")]
    public float groundRayLength = 1.2f;

    public float gravity = -20f;
    public LayerMask groundLayer;

    [Header("Collision / Navigation")]
    [Tooltip("Layers considered 'solid' for the boss (walls, props, etc).")]
    public LayerMask obstacleLayers;

    [Tooltip("How far to probe forward each frame for collision.")]
    public float wallCheckDistance = 0.5f;

    private float verticalVelocity = 0f;
    private Transform goal;
    private Rigidbody rb;
    private Animator anim;
    private CapsuleCollider bossCollider;

    // cached collider dims for capsule casts
    private float capsuleRadius;
    private float capsuleHalfHeight;
    private Vector3 capsuleCenterLocal;

    private float decisionTimer = 0f;
    private string currentState = "Idle";
    private bool waitingForAttackFinish = false;
    private bool bossActive = false;
    private int strafeDirection = 1;

    // --- Sudden Step (mini dash / lunge) ---
    private bool isSuddenStepping = false;
    public float suddenStepDistance = 2f;
    public float suddenStepSpeed = 10f;
    private Vector3 suddenStepTarget;

    // --- Charge Attack ---
    private bool isCharging = false;
    public float chargeSpeed = 20f;
    public float chargeDistance = 15f;
    private Vector3 chargeTarget;

    [Header("VFX")]
    public ParticleSystem explodeEffect;

    [Header("Behind Detection")]
    [Range(0f, 180f)]
    public float behindAngleThreshold = 120f; // angle where player counts as "behind me"

    private void Start()
    {
        // refs
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        bossCollider = GetComponent<CapsuleCollider>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.isKinematic = true;
        rb.useGravity = false; // we do manual gravity
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (bossCollider != null)
        {
            capsuleRadius = bossCollider.radius;
            capsuleHalfHeight = bossCollider.height * 0.5f;
            capsuleCenterLocal = bossCollider.center;
        }
    }

    private void Update()
    {
        // no brain unless active
        if (!bossActive || goal == null) return;

        // 1) always do dash step if in sudden step
        DashStepTowardTarget();

        // if still dashing, don't run normal AI for this frame
        if (isSuddenStepping)
            return;

        // 2) handle charge movement
        if (isCharging)
        {
            ChargeStep();
            return; // no other logic while charging
        }

        float dist = Vector3.Distance(transform.position, goal.position);

        // hard leash
        if (dist > chaseRadius)
        {
            SetState("Idle", "Idle");
            return;
        }

        // pause movement if mid-attack lock
        if (waitingForAttackFinish)
        {
            return;
        }

        // in attack range: decide behavior
        if (dist <= attackRadius)
        {
            decisionTimer -= Time.deltaTime;
            if (decisionTimer <= 0f && !waitingForAttackFinish)
            {
                decisionTimer = decisionInterval;
                FacePlayer();
                DecideAction();
            }

            switch (currentState)
            {
                case "Attack1":
                case "Attack2":
                case "Attack3":
                case "AreaAttack":
                case "ChargeAttack":
                    // remain still, animation handles it
                    break;

                case "Mundur":
                    RetreatFromPlayer();
                    break;

                case "Strafe":
                    StrafeAroundPlayer();
                    break;
            }
        }
        else
        {
            // chase
            SetState("Chase", "Run");
            ChasePlayer();
        }
    }

    private void FixedUpdate()
    {
        AlignToGround();
    }

    // -------------------------------------------------
    // PUBLIC from arena trigger
    // -------------------------------------------------

    public void ActivateBoss(Transform player)
    {
        goal = player;
        bossActive = true;
        SetState("Chase", "Run");
        Debug.Log("Boss activated!");
    }

    public void DeactivateBoss()
    {
        bossActive = false;
        SetState("Idle", "Idle");
    }

    // -------------------------------------------------
    // HIGH-LEVEL ACTIONS
    // -------------------------------------------------

    void ChasePlayer()
    {
        if (!goal) return;
        MoveIntent(goal.position, moveSpeed);
        FacePlayer();
    }

    void RetreatFromPlayer()
    {
        if (!goal) return;
        Vector3 dirAway = (transform.position - goal.position).normalized;
        Vector3 retreatPoint = transform.position + dirAway;
        MoveIntent(retreatPoint, moveSpeed);
        FacePlayer();
    }

    void StrafeAroundPlayer()
    {
        if (!goal) return;

        Vector3 toBoss = transform.position - goal.position;
        toBoss.y = 0f;

        Vector3 strafeDir = Vector3.Cross(Vector3.up, toBoss).normalized * strafeDirection;

        AttemptMove(strafeDir, strafeSpeed);
        FacePlayer();
    }

    void FacePlayer()
    {
        if (!goal) return;

        Vector3 dir = (goal.position - transform.position);
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // -------------------------------------------------
    // GROUNDING / GRAVITY
    // -------------------------------------------------

    void AlignToGround()
    {
        // keep boss hugging terrain, with manual gravity fallback
        if (Physics.Raycast(
            transform.position + Vector3.up,
            Vector3.down,
            out RaycastHit hit,
            groundRayLength,
            groundLayer))
        {
            verticalVelocity = 0f;
            Vector3 pos = transform.position;
            pos.y = hit.point.y;
            transform.position = pos;
        }
        else
        {
            // falling
            verticalVelocity += gravity * Time.deltaTime;
            transform.position += new Vector3(
                0f,
                verticalVelocity * Time.deltaTime,
                0f
            );
        }
    }

    // -------------------------------------------------
    // BASIC MOVEMENT *WITH* COLLISION
    // -------------------------------------------------

    void MoveIntent(Vector3 targetPos, float speed)
    {
        Vector3 dir = (targetPos - transform.position);
        dir.y = 0f;
        AttemptMove(dir, speed);
    }

    // smart move using capsule cast
    // returns true if we actually moved
    bool AttemptMove(Vector3 desiredDir, float speed)
    {
        if (desiredDir.sqrMagnitude < 0.0001f) return false;

        Vector3 step = desiredDir.normalized * speed * Time.deltaTime;

        // build capsule positions in world space
        // we sample using the collider info
        Vector3 worldCenter = transform.TransformPoint(capsuleCenterLocal);

        float half = capsuleHalfHeight;
        float rad = capsuleRadius;

        // top/bottom points of capsule
        Vector3 up = transform.up;
        Vector3 p1 = worldCenter + up * (half - rad);
        Vector3 p2 = worldCenter - up * (half - rad);

        // cast ahead to see if blocked
        if (Physics.CapsuleCast(
            p1,
            p2,
            rad,
            step.normalized,
            out RaycastHit hit,
            step.magnitude + wallCheckDistance,
            obstacleLayers,
            QueryTriggerInteraction.Ignore))
        {
            // blocked by wall/obstacle
            return false;
        }

        // not blocked → move
        transform.position += step;
        return true;
    }

    // -------------------------------------------------
    // SUDDEN STEP (short dash / lunge)
    // -------------------------------------------------

    // Animation event calls this at the dash frame
    void SuddenStep()
    {
        if (isSuddenStepping) return;

        isSuddenStepping = true;
        suddenStepTarget = transform.position + transform.forward * suddenStepDistance;
    }

    // graceful cancel (optional animation event)
    void SuddenStepStop()
    {
        isSuddenStepping = false;
    }

    // run every Update() before main AI logic
    void DashStepTowardTarget()
    {
        if (!isSuddenStepping) return;

        // figure out remaining direction
        Vector3 dashDir = suddenStepTarget - transform.position;
        dashDir.y = 0f;
        float remaining = dashDir.magnitude;

        // already basically there? stop
        if (remaining < 0.05f)
        {
            isSuddenStepping = false;
            return;
        }

        // try to move with collision
        bool moved = AttemptMove(dashDir, suddenStepSpeed);

        if (!moved)
        {
            // blocked instantly, abort dash
            isSuddenStepping = false;
            return;
        }

        // if after moving we are super close, stop
        Vector3 after = suddenStepTarget - transform.position;
        after.y = 0f;
        if (after.magnitude < 0.05f)
        {
            isSuddenStepping = false;
        }
    }

    // -------------------------------------------------
    // CHARGE ATTACK (long rush)
    // -------------------------------------------------

    // Animation event at start of Crazed Monkey's charge anim
    void ChargeAttack()
    {
        if (isCharging || bossName != "Crazed Monkey" || goal == null) return;

        isCharging = true;

        // OPTIONAL:
        // turn collider into trigger so he can plow through, Souls-boss style
        // comment this out if you WANT him to collide/stop on walls.
        if (bossCollider != null)
            bossCollider.isTrigger = true;

        // chargeTarget goes forward past the player
        Vector3 toPlayer = (goal.position - transform.position).normalized;
        chargeTarget = transform.position + toPlayer * chargeDistance;

        // face player first
        FacePlayer();

        Debug.Log("Crazed Monkey begins charging!");
    }

    // per-frame charge movement
    void ChargeStep()
    {
        Vector3 runDir = chargeTarget - transform.position;
        runDir.y = 0f;

        // if basically there, end it
        if (runDir.magnitude < 0.2f)
        {
            ChargeAttackEnd();
            return;
        }

        // try to move
        bool moved = AttemptMove(runDir, chargeSpeed);

        if (!moved)
        {
            // hit a wall early → stop the charge
            ChargeAttackEnd();
        }
    }

    void ChargeAttackEnd()
    {
        isCharging = false;

        if (bossCollider != null)
            bossCollider.isTrigger = false;

        Debug.Log("Crazed Monkey finished charging.");
    }

    // -------------------------------------------------
    // DECISION MAKING
    // -------------------------------------------------

    void DecideAction()
    {
        // If player is behind me → AreaAttack priority
        if (IsPlayerBehind())
        {
            SetState("AreaAttack", "AreaAttack");
            return;
        }

        float rand = Random.Range(0f, 100f);

        // Gate Defender Anton
        if (bossName == "The Gate Defender, Anton")
        {
            if (rand < 40f)
                SetState("Attack1", "Attack1");
            else if (rand < 70f)
                SetState("Attack2", "Attack2");
            else if (rand < 90f)
                SetState("Strafe", "Strafe");
            else
                SetState("Mundur", "Mundur");
        }
        // Crazed Monkey
        else if (bossName == "Crazed Monkey")
        {
            if (rand < 30f)
                SetState("Attack1", "Attack1");
            else if (rand < 70f)
                SetState("Attack2", "Attack2");
            else if (rand < 75f)
                SetState("Mundur", "Mundur");
            else if (rand < 95f)
                SetState("ChargeAttack", "ChargeAttack");
            else
                SetState("Strafe", "Strafe");
        }
        // Batu
        else if (bossName == "Batu")
        {
            if (rand < 30f)
                SetState("Attack1", "Attack1");
            else if (rand < 60f)
                SetState("Attack2", "Attack2");
            else if (rand < 90f)
                SetState("Attack3", "Attack3");
            else
                SetState("Strafe", "Strafe");
        }
    }

    bool IsPlayerBehind()
    {
        if (goal == null) return false;

        Vector3 toPlayer = (goal.position - transform.position).normalized;
        toPlayer.y = 0f;
        Vector3 forward = transform.forward;

        float angle = Vector3.Angle(forward, toPlayer);
        return angle > behindAngleThreshold;
    }

    // -------------------------------------------------
    // ANIMATION STATE MGMT
    // -------------------------------------------------

    void SetState(string newState, string animTrigger)
    {
        if (currentState == newState) return;

        ResetAllTriggers();
        currentState = newState;
        anim.SetTrigger(animTrigger);

        // if you want lockout movement during big attacks, uncomment:
        // if (newState == "Attack1" || newState == "Attack2" || newState == "Attack3" ||
        //     newState == "AreaAttack" || newState == "ChargeAttack")
        //     StartCoroutine(WaitForAttackFinish());
    }

    void ResetAllTriggers()
    {
        anim.ResetTrigger("Idle");
        anim.ResetTrigger("Run");
        anim.ResetTrigger("Attack1");
        anim.ResetTrigger("Attack2");
        anim.ResetTrigger("Attack3");
        anim.ResetTrigger("AreaAttack");
        anim.ResetTrigger("ChargeAttack");
        anim.ResetTrigger("Strafe");
        anim.ResetTrigger("Mundur");
    }

    IEnumerator WaitForAttackFinish()
    {
        waitingForAttackFinish = true;

        float originalSpeed = moveSpeed;
        moveSpeed = 0f;

        yield return null; // let animator enter the attack state

        AnimatorStateInfo st = anim.GetCurrentAnimatorStateInfo(0);
        float clipLen = st.length;

        yield return new WaitForSeconds(clipLen);

        moveSpeed = originalSpeed;
        waitingForAttackFinish = false;
    }

    public void PlayExplodeEffect()
    {
        if (explodeEffect != null)
            explodeEffect.Play();
    }

    // -------------------------------------------------
    // GIZMOS (debug)
    // -------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        // forward dir
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 3f);

        // ranges
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // behind cone-ish
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        DrawBehindCone();

        // if we have target
        if (goal != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, goal.position);

            if (IsPlayerBehind())
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(goal.position, 1f);
            }
        }

        // sudden step debug
        if (isSuddenStepping)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(suddenStepTarget, 0.2f);
        }

        // charge debug
        if (isCharging)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(chargeTarget, 0.3f);
        }
    }

    void DrawBehindCone()
    {
        float halfAngle = behindAngleThreshold * 0.5f;
        int segments = 20;
        float radius = 3f;

        Vector3 backDir = -transform.forward;

        Vector3 lastPoint = transform.position +
            (Quaternion.AngleAxis(-halfAngle, Vector3.up) * backDir) * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + (i * (behindAngleThreshold / segments));
            Vector3 nextPoint = transform.position +
                (Quaternion.AngleAxis(angle, Vector3.up) * backDir) * radius;

            Gizmos.DrawLine(transform.position, nextPoint);
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
}
