using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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
    public float strafeSpeed = 4f;

    [Header("Behind Detection")]
    [Range(0f, 180f)]
    public float behindAngleThreshold = 120f;

    private Transform goal;
    private NavMeshAgent agent;
    private float decisionTimer = 0f;
    private string currentState = "Idle";
    private bool waitingForAttackFinish = false;
    private bool bossActive = false;

    private Animator anim;
    private int strafeDirection = 1;

    private void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!bossActive || goal == null) return;

        float dist = Vector3.Distance(transform.position, goal.position);

        // Outside chase radius → idle
        if (dist > chaseRadius)
        {
            SetState("Idle", "Idle");
            agent.ResetPath();
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
            }

            if (currentState == "Attack1" || currentState == "Attack2" ||
                currentState == "Attack3" || currentState == "AreaAttack" ||
                currentState == "ChargeAttack")
            {
                agent.isStopped = true;
                return;
            }
            else if (currentState == "Mundur")
            {
                agent.isStopped = true;
                agent.Move(-transform.forward * agent.speed * Time.deltaTime);
                return;
            }
            else if (currentState == "Strafe")
            {
                agent.isStopped = true;
                StrafeAroundPlayer();
                return;
            }
        }
        else
        {
            SetState("Chase", "Run");
            RunToPlayer();
        }
    }

    public void ActivateBoss(Transform player)
    {
        goal = player;
        bossActive = true;
        SetState("Chase", "Run");
        RunToPlayer();
        Debug.Log("Boss activated!");
    }

    void RunToPlayer()
    {
        if (goal == null) return;
        agent.isStopped = false;
        agent.SetDestination(goal.position);
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

        //  Gate Defender Anton (unchanged)
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
            // 30% Attack1, 40% Attack2, 5% Mundur, 20% Charge, 5% Strafe (remaining)
            if (rand < 30f)
                SetState("Attack1", "Attack1");
            else if (rand < 70f)
                SetState("Attack2", "Attack2");
            else if (rand < 75f)
                SetState("Mundur", "Mundur");
            //else if (rand < 95f)
               // SetState("ChargeAttack", "ChargeAttack");
            else
                SetState("Strafe", "Strafe");
        }

        //  Batu
        else if (bossName == "Batu")
        {
            // 30% Attack1, 30% Attack2, 30% Attack3, 10% Strafe
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

    void StrafeAroundPlayer()
    {
        if (!goal) return;

        agent.ResetPath();
        Vector3 dirToPlayer = transform.position - goal.position;
        dirToPlayer.y = 0;
        Vector3 strafeDir = Vector3.Cross(Vector3.up, dirToPlayer).normalized * strafeDirection;

        transform.position += strafeDir * strafeSpeed * Time.deltaTime;

        Quaternion lookRot = Quaternion.LookRotation(goal.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
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
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);
        waitingForAttackFinish = false;
    }

    public void DeactivateBoss()
    {
        bossActive = false;
        agent.ResetPath();
        SetState("Idle", "Idle");
    }

    // 🔹 Gizmo Visualization
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
