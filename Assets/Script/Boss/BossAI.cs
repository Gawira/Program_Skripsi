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
    private float verticalVelocity = 0f;
    public float gravity = -20f;
    public float rotationSpeed = 8f;
    public float groundRayLength = 1.2f;
    public LayerMask groundLayer;

    [Header("Behind Detection")]
    [Range(0f, 180f)]
    public float behindAngleThreshold = 120f;

    private Transform goal;
    private Rigidbody rb;
    private Animator anim;
    private float decisionTimer = 0f;
    private string currentState = "Idle";
    private bool waitingForAttackFinish = false;
    private bool bossActive = false;
    private int strafeDirection = 1;

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.isKinematic = true;
        rb.useGravity = false;  // we’ll handle gravity manually
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if (!bossActive || goal == null) return;

        float dist = Vector3.Distance(transform.position, goal.position);

        // Outside chase radius → idle
        if (dist > chaseRadius)
        {
            SetState("Idle", "Idle");
            return;
        }

        // During attack, stop movement
        if (waitingForAttackFinish)
        {
            return;
        }

        // Inside attack radius → make decisions
        if (dist <= attackRadius)
        {
            decisionTimer -= Time.deltaTime;
            if (decisionTimer <= 0f && !waitingForAttackFinish)
            {
                decisionTimer = decisionInterval;
                DecideAction();
                FacePlayer();
            }

            switch (currentState)
            {
                case "Attack1":
                case "Attack2":
                case "Attack3":
                case "AreaAttack":
                case "ChargeAttack":
                    // Attack — movement handled by animation or pause
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
            SetState("Chase", "Run");
            ChasePlayer();
        }
    }

    private void FixedUpdate()
    {
        AlignToGround();
    }

    public void ActivateBoss(Transform player)
    {
        goal = player;
        bossActive = true;
        SetState("Chase", "Run");
        Debug.Log("Boss activated!");
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
        Vector3 dirAway = (transform.position - goal.position).normalized;
        MoveTowards(transform.position + dirAway, moveSpeed);
        FacePlayer();
    }

    void StrafeAroundPlayer()
    {
        if (!goal) return;
        Vector3 dirToPlayer = transform.position - goal.position;
        dirToPlayer.y = 0;
        Vector3 strafeDir = Vector3.Cross(Vector3.up, dirToPlayer).normalized * strafeDirection;
        transform.position += strafeDir * strafeSpeed * Time.deltaTime;
        FacePlayer();
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
            // Not grounded - apply gravity
            verticalVelocity += gravity * Time.deltaTime;
            transform.position += new Vector3(0f, verticalVelocity * Time.deltaTime, 0f);
        }
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 moveDir = (target - transform.position).normalized;
        moveDir.y = 0;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    void DecideAction()
    {
        //  Area Attack priority if behind
        if (IsPlayerBehind())
        {
            SetState("AreaAttack", "AreaAttack");
            return;
        }

        float rand = Random.Range(0f, 100f);

        //  Gate Defender Anton
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
        //  Crazed Monkey
        else if (bossName == "Crazed Monkey")
        {
            if (rand < 30f)
                SetState("Attack1", "Attack1");
            else if (rand < 70f)
                SetState("Attack2", "Attack2");
            else if (rand < 75f)
                SetState("Mundur", "Mundur");
            else
                SetState("Strafe", "Strafe");
        }
        //  Batu
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

    void SetState(string newState, string animTrigger)
    {
        if (currentState == newState) return;

        ResetAllTriggers();
        currentState = newState;
        anim.SetTrigger(animTrigger);

        if (newState == "Attack1" || newState == "Attack2" || newState == "Attack3" ||
            newState == "AreaAttack" || newState == "ChargeAttack")
            StartCoroutine(WaitForAttackFinish());
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

    public void DeactivateBoss()
    {
        bossActive = false;
        SetState("Idle", "Idle");
    }

    // 🔹 Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 3f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        DrawBehindCone();

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
