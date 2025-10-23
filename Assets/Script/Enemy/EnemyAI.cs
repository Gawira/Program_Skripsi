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

        float dist = Vector3.Distance(transform.position, goal.position);

        // If player is too far, idle
        if (dist > chaseRadius)
        {
            SetState("Idle", "Idle");
            return;
        }

        // Stop all movement during attack
        if (waitingForAttackFinish)
        {
            return;
        }

        // Decision handling
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
        if (isSuddenStepping)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                suddenStepTarget,
                suddenStepSpeed * Time.deltaTime
            );
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
        Vector3 dirAway = (transform.position - goal.position).normalized;
        MoveTowards(transform.position + dirAway, moveSpeed);
    }

    void StrafeAroundPlayer()
    {
        if (!goal) return;
        Vector3 dirToPlayer = transform.position - goal.position;
        dirToPlayer.y = 0;
        Vector3 strafeDir = Vector3.Cross(Vector3.up, dirToPlayer).normalized * strafeDirection;
        transform.position += strafeDir * strafeSpeed * Time.deltaTime;
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
        transform.position += moveDir * moveSpeed * Time.deltaTime;
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
