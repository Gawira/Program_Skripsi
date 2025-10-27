using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    [Header("Target Settings")]
    public string playerTag = "Player";     // Make sure your PlayerManager object has this tag
    private PlayerManager playerManager;

    [Header("Follow Settings")]
    public float moveSpeed = 5f;            // Movement speed of the follower
    public float stopDistance = 1f;         // How close it should get to the player

    private Transform playerTransform;

    void Start()
    {
        // Find PlayerManager by tag
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            playerManager = playerObject.GetComponent<PlayerManager>();
            playerTransform = playerManager.transform;
        }

        if (playerManager == null)
        {
            Debug.LogWarning("PlayerManager not found in scene!");
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        Vector3 playerPosition = playerTransform.position;
        float distance = Vector3.Distance(transform.position, playerPosition);

        // Only follow if not too close
        if (distance > stopDistance)
        {
            // Move toward the player smoothly
            transform.position = Vector3.MoveTowards(
                transform.position,
                playerPosition,
                moveSpeed * Time.deltaTime
            );
        }

        //// Optionally rotate to face the player
        //Vector3 lookDirection = (playerPosition - transform.position).normalized;
        //if (lookDirection != Vector3.zero)
        //{
        //    Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        //    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        //}
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }

        // Draw stop distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}