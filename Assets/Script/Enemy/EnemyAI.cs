using System.Collections;
using UnityEngine;

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
    public LayerMask groundLayer;

    private Transform goal;
    private Rigidbody rb;
    private Animator anim;

    private float decisionTimer = 0f;
    private string currentState = "Idle";
    private int strafeDirection = 1;
    private bool waitingForAttackFinish = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // Prevent tipping
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        FindPlayerOnce();
    }

    void Update()
    {
        if (goal == null)
        {
            FindPlayerOnce();
            return;
        }

        float dist = Vector3.Distance(transform.position, goal.position);

        if (dist > chaseRadius)
        {
            if (currentState != "Attack")
                SetState("Idle", "Idle");
        }
        else if (dist <= attackRadius)
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
        // Make sure enemy stays grounded
        AlignToGround();
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
        Vector3 dirAway = (transform.position - goal.position).normalized;
        MoveTowards(transform.position + dirAway, moveSpeed);
    }

    void StrafeAroundPlayer()
    {
        if (!goal) return;
        Vector3 dirToPlayer = transform.position - goal.position;
        dirToPlayer.y = 0;
        Vector3 strafeDir = Vector3.Cross(Vector3.up, dirToPlayer).normalized * strafeDirection;
        rb.MovePosition(rb.position + strafeDir * strafeSpeed * Time.deltaTime);
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
        Vector3 moveDir = (target - transform.position).normalized;
        moveDir.y = 0;
        rb.MovePosition(rb.position + moveDir * speed * Time.deltaTime);
    }

    void AlignToGround()
    {
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, groundRayLength, groundLayer))
        {
            Vector3 pos = rb.position;
            pos.y = hit.point.y;
            rb.MovePosition(pos);
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

        if (newState == "Attack")
            StartCoroutine(WaitForAttackFinish());
    }

    IEnumerator WaitForAttackFinish()
    {
        waitingForAttackFinish = true;
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);
        waitingForAttackFinish = false;
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
