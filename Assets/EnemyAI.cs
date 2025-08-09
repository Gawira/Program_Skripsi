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
    public float attackChance = 100f;     // % chance to AttackPosition (only in attack radius)

    [Header("Movement")]
    public float strafeSpeed = 2f;       // How fast it circles player

    private Transform goal;
    private NavMeshAgent agent;
    private float checkTimer = 0f;
    private float decisionTimer = 0f;
    private string currentState = "Idle"; // Idle by default until player is in chaseRadius
    private int strafeDirection = 1;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // nge-check setiap beberapa detik
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f)
        {
            checkTimer = checkInterval;
            if (goal == null)
                FindPlayerOnce();
        }

        if (goal == null) return;

        float dist = Vector3.Distance(transform.position, goal.position);

        // player di luar chase radius, enemy idle
        if (dist > chaseRadius)
        {
            currentState = "Idle";
            agent.ResetPath();
        }
        else if (dist <= attackRadius) // player di dalam attack radius
        {
            
            decisionTimer -= Time.deltaTime;
            if (decisionTimer <= 0f)
            {
                decisionTimer = decisionInterval;
                DecideAttackOrIdle();
            }

            if (currentState == "Attack")
            {
                AttackPosition(); // override movement
                return; // Don't run to player
            }
        }
        else
        {
            // Player in chase range but outside attack range
            currentState = "Chase";
        }

        // Only chase if not attacking
        if (currentState == "Chase")
        {
            RunToPlayer();
        }

        //Debug.Log($"State: {currentState} | Dist: {dist:F1}");
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

        if (rand < attackChance)
        {
            currentState = "Attack";
            strafeDirection = Random.value < 0.5f ? -1 : 1;
        }
        else
            currentState = "Idle";
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

    void RunToPlayer()
    {
        if (!goal) return;

        currentState = "Chase";
        agent.destination = goal.position;
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
}
