using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public float chaseRadius = 20f;
    public float attackRadius = 10f;
    public string playerTag = "Player";
    public float checkInterval = 0.5f;

    [Header("Decision Settings")]
    public float decisionInterval = 0.5f;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float strafeSpeed = 2f;
    public float rotationSpeed = 8f;
    public float groundRayLength = 1.2f;
    private float verticalVelocity = 0f;
    public float gravity = -20f;
    public LayerMask groundLayer;

    private Transform goal;
    private Rigidbody rb;
    private Animator anim;

    [Header("Collision / Obstacle Avoidance")]
    [Tooltip("Which layers count as solid walls / props / level geometry.")]
    public LayerMask obstacleMask;

    [Tooltip("Radius of this enemy's body for collision checks (match your CapsuleCollider radius).")]
    public float capsuleRadius = 0.4f;

    [Tooltip("Height of the body cast (match your CapsuleCollider height).")]
    public float capsuleHeight = 1.8f;

    [Tooltip("How high off the ground to start the cast (usually half of radius).")]
    public float capsuleBottomOffset = 0.5f;


    private float decisionTimer = 0f;
    private string currentState = "Idle";
    private int strafeDirection = 1;
    private bool waitingForAttackFinish = false;

    private bool isSuddenStepping = false;
    public float suddenStepDistance = 2f; // tweak for how far the step moves
    public float suddenStepSpeed = 10f;   // tweak for how fast the step happens
    private Vector3 suddenStepTarget;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;  // we’ll handle gravity manually
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        FindPlayerOnce();
    }

    void Update()
    {
        if (goal == null)
        {
            FindPlayerOnce();
            return;
        }

        PlayerManager playerManager = goal.GetComponent<PlayerManager>();
        if (playerManager == null)
            return;

        // Stop AI logic if player is dead
        if (playerManager.currentHealth <= 0)
        {
            SetState("Idle", "Idle");
            return;
        }

        float dist = Vector3.Distance(transform.position, goal.position);

        // Run dash step first so movement for this frame includes dash
        DashStepTowardTarget();

        // If still mid-dash, we don't do normal AI movement/decisions this frame
        if (isSuddenStepping)
            return;

        // If player is super far: idle
        if (dist > chaseRadius)
        {
            SetState("Idle", "Idle");
            return;
        }

        // Don't walk/strafe while we're "locked in" an attack anim
        if (waitingForAttackFinish)
            return;

        if (dist <= attackRadius)
        {
            decisionTimer -= Time.deltaTime;
            if (decisionTimer <= 0f && !waitingForAttackFinish)
            {
                decisionTimer = decisionInterval;
                DecideAttackOrIdle();
            }

            switch (currentState)
            {
                case "Attack":
                    FacePlayer();
                    break;

                case "Mundur":
                    RetreatFromPlayer();
                    FacePlayer();
                    break;

                case "Strafe":
                    StrafeAroundPlayer();
                    FacePlayer();
                    break;
            }
        }
        else
        {
            SetState("Chase", "Run");
            ChasePlayer();
        }
    }

    void FixedUpdate()
    {
        AlignToGround();
    }

    void SuddenStep()
    {
        if (isSuddenStepping) return;

        isSuddenStepping = true;
        suddenStepTarget = transform.position + transform.forward * suddenStepDistance;
    }

    void SuddenStepStop()
    {
        isSuddenStepping = false;


    }

    void FindPlayerOnce()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
            goal = player.transform;
    }

    void DecideAttackOrIdle()
    {
        float rand = Random.Range(0f, 100f);

        if (Random.value < 0.7f)
        {
            if (rand < 90f)
                SetState("Attack", "Attack1");
            else
                SetState("Strafe", "Strafe");
        }
        else
        {
            if (rand < 40f)
                SetState("Mundur", "Mundur");
            else
                SetState("Strafe", "Strafe");
        }
    }

    void ChasePlayer()
    {
        if (!goal) return;
        MoveTowards(goal.position, moveSpeed);
        FacePlayer();
    }

    void RetreatFromPlayer()
    {
        if (!goal) return;
        Vector3 dirAway = (transform.position - goal.position);
        dirAway.y = 0f;
        AttemptMove(dirAway, moveSpeed);
    }

    void StrafeAroundPlayer()
    {
        if (!goal) return;

        Vector3 dirToPlayer = transform.position - goal.position;
        dirToPlayer.y = 0f;

        // perpendicular to player-facing vector
        Vector3 strafeDir = Vector3.Cross(Vector3.up, dirToPlayer).normalized * strafeDirection;

        AttemptMove(strafeDir, strafeSpeed);
    }

    void FacePlayer()
    {
        if (!goal) return;

        Vector3 dir = (goal.position - transform.position);
        dir.y = 0;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 moveDir = target - transform.position;
        moveDir.y = 0f;
        AttemptMove(moveDir, speed);
    }

    // Try to move in desiredDir (world space, horizontal), respecting obstacles.
    // Returns true if we actually moved.
    private bool AttemptMove(Vector3 desiredDir, float speed)
    {
        desiredDir.y = 0f;
        if (desiredDir.sqrMagnitude < 0.0001f)
            return false;

        desiredDir.Normalize();

        float stepDist = speed * Time.deltaTime;

        // Build a capsule that matches the enemy's collider volume
        Vector3 bottom = transform.position + Vector3.up * capsuleBottomOffset;
        Vector3 top = bottom + Vector3.up * (capsuleHeight - capsuleBottomOffset);

        // 1. Check straight ahead
        if (!Physics.CapsuleCast(
                bottom,
                top,
                capsuleRadius,
                desiredDir,
                out RaycastHit hit,
                stepDist,
                obstacleMask,
                QueryTriggerInteraction.Ignore))
        {
            // path is clear, just move
            transform.position += desiredDir * stepDist;
            return true;
        }

        // 2. Blocked → try to slide along the surface normal
        Vector3 slideDir = Vector3.ProjectOnPlane(desiredDir, hit.normal);
        slideDir.y = 0f;

        if (slideDir.sqrMagnitude > 0.0001f)
        {
            slideDir.Normalize();

            if (!Physics.CapsuleCast(
                    bottom,
                    top,
                    capsuleRadius,
                    slideDir,
                    stepDist,
                    obstacleMask,
                    QueryTriggerInteraction.Ignore))
            {
                transform.position += slideDir * stepDist;
                return true;
            }
        }

        // 3. Totally stuck this frame
        return false;
    }

    private void DashStepTowardTarget()
    {
        // If we're not currently dashing, do nothing.
        if (!isSuddenStepping) return;

        // Figure out the flat direction to the suddenStepTarget
        Vector3 dashDir = suddenStepTarget - transform.position;
        dashDir.y = 0f;
        float remainingDist = dashDir.magnitude;

        // Safety: if we're basically already there, stop the dash
        if (remainingDist < 0.05f)
        {
            isSuddenStepping = false;
            return;
        }

        // Reuse the same collision logic, but with higher speed
        bool moved = AttemptMove(dashDir, suddenStepSpeed);

        if (!moved)
        {
            // We tried to dash but hit a wall immediately.
            // End the dash so AI doesn't get stuck in 'isSuddenStepping = true'.
            isSuddenStepping = false;
            return;
        }

        // If we DID move, check if we basically reached the target this frame.
        // We only care about horizontal distance because suddenStepTarget is horizontal.
        Vector3 postMoveDelta = suddenStepTarget - transform.position;
        postMoveDelta.y = 0f;
        if (postMoveDelta.magnitude < 0.05f)
        {
            isSuddenStepping = false;
        }
    }

    void AlignToGround()
    {
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, groundRayLength, groundLayer))
        {
            // Grounded
            verticalVelocity = 0f;
            Vector3 pos = transform.position;
            pos.y = hit.point.y;
            transform.position = pos;
        }
        else
        {
            // 🪂 Not grounded - apply gravity
            verticalVelocity += gravity * Time.deltaTime;
            transform.position += new Vector3(0f, verticalVelocity * Time.deltaTime, 0f);
        }
    }

    void SetState(string newState, string animationTrigger)
    {
        if (currentState == newState) return;

        anim.ResetTrigger("Idle");
        anim.ResetTrigger("Run");
        anim.ResetTrigger("Attack1");
        anim.ResetTrigger("Strafe");
        anim.ResetTrigger("Mundur");

        currentState = newState;
        anim.SetTrigger(animationTrigger);

        //if (newState == "Attack")
        //    StartCoroutine(WaitForAttackFinish());
    }

    IEnumerator SuddenStepCoroutine()
    {
        float originalSpeed = moveSpeed;
        float boostedSpeed = moveSpeed * 2.5f; //  You can tweak this multiplier
        float duration = 0.2f;                 //  Duration of the step

        moveSpeed = boostedSpeed;

        // Move slightly forward in facing direction
        float timer = 0f;
        while (timer < duration)
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        moveSpeed = originalSpeed;
    }

    IEnumerator WaitForAttackFinish()
    {
        if (isSuddenStepping)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                suddenStepTarget,
                suddenStepSpeed * Time.deltaTime
            );
        }
        else
        {

            waitingForAttackFinish = true;

            //  Store the current movement speed
            float originalMoveSpeed = moveSpeed;

            //  Stop movement during attack
            moveSpeed = 0f;

            // Wait one frame to ensure animator updates to the attack state
            yield return null;

            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            float clipLength = stateInfo.length;

            //  Wait for the attack animation to finish
            yield return new WaitForSeconds(clipLength);

            //  Restore the movement speed
            moveSpeed = originalMoveSpeed;

            waitingForAttackFinish = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        if (goal != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, goal.position);
        }
    }
}
