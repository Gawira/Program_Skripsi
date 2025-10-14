using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public float chaseRadius = 20f;      // How far enemy starts chasing
    public float attackRadius = 10f;     // How close enemy can attack/strafe
    public string playerTag = "Player";
    public float checkInterval = 0.5f;   // How often it searches for player

    [Header("Decision Settings")]
    public float decisionInterval = 0.5f;  // How often it chooses an action

    [Header("Movement")]
    public float strafeSpeed = 2f;       // How fast it circles player

    private Transform goal;
    private NavMeshAgent agent;
    private float checkTimer = 0f;
    private float decisionTimer = 1f;
    private string currentState = "Idle"; // Idle by default until player is in chaseRadius
    private int strafeDirection = 1;
    private bool canDecide = true;
    private bool waitingForAttackFinish = false;

    [SerializeField] private Animator m_Animator;


    

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // Keep agent's automatic rotation
        agent.updateRotation = true;

        // Apply offset
        float rotationOffset = 90f; // degrees
        transform.rotation = agent.transform.rotation * Quaternion.Euler(0, rotationOffset, 0);
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
            if (currentState == "Attack")
            {
                agent.isStopped = true;
                return;
            }
            SetState("Idle", "Idle");
            agent.ResetPath();
        }
        else if (dist <= attackRadius)
        {
            decisionTimer -= Time.deltaTime;
            if (decisionTimer <= 0f && !waitingForAttackFinish)
            {
                decisionTimer = decisionInterval;
                DecideAttackOrIdle();
            }

            if (currentState == "Attack")
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
                AttackPosition();
                return;
            }
        }
        else // chase mode
        {
            SetState("Chase", "Run");
            RunToPlayer();
        }
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

        if (Random.value < 0.7f) // attack bias
        {
            if (rand < 90f)
                SetState("Attack", "Attack1");
            else
                SetState("Strafe", "Strafe");
        }
        else // defensive bias
        {
            if (rand < 40f)
                SetState("Mundur", "Mundur");
            else
                SetState("Strafe", "Strafe");
        }
    }

    
    void AttackPosition()
    {
        if (!goal) return;

        // Stop NavMeshAgent movement
        agent.ResetPath();

        // Circle around the player
        Vector3 dirToPlayer = transform.position - goal.position;
        dirToPlayer.y = 0;

        Vector3 strafeDir = Vector3.Cross(Vector3.up, dirToPlayer).normalized * strafeDirection;

        transform.position += strafeDir * strafeSpeed * Time.deltaTime;

        // Always face the player
        Quaternion lookRot = Quaternion.LookRotation(goal.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
    }

    public void RunToPlayer()
    {
        if (!goal) return;

        currentState = "Chase";
        agent.isStopped = false; 
        agent.SetDestination(goal.position); 
    }

    public void StopToPlayer()
    {
        agent.ResetPath(); 
    }

    void SetState(string newState, string animationTrigger)
    {
        if (currentState == newState) return; // already in that state

        // Reset triggers
        m_Animator.ResetTrigger("Idle");
        m_Animator.ResetTrigger("Run");
        m_Animator.ResetTrigger("Attack1");
        m_Animator.ResetTrigger("Strafe");
        m_Animator.ResetTrigger("Mundur");

        // Set new state + animation
        currentState = newState;
        m_Animator.SetTrigger(animationTrigger);

        if (newState == "Attack")  // Attack1 in your animator
        {
            StartCoroutine(WaitForAttackFinish());
        }
    }


    void OnDrawGizmosSelected()
    {
        // **Attack radius (red)**
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // **Chase radius (yellow)**
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        // Draw line to goal if exists
        if (goal != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, goal.position);
        }
    }

    IEnumerator WaitForAttackFinish()
    {
        waitingForAttackFinish = true;

        // Get the current clip length
        AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
        float clipLength = stateInfo.length;

        // Wait for the animation to finish
        yield return new WaitForSeconds(clipLength);

        waitingForAttackFinish = false;
    }
}
