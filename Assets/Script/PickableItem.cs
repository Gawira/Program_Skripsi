using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickableItem : MonoBehaviour
{
    [Header("Pickup Identity")]
    [Tooltip("Unique ID for persistence, e.g. 'pickup_room2_sacredstone'")]
    public string pickupID = "pickup_01";

    [Header("Pickup Data")]
    public DjimatItem itemData;
    public string playerTag = "Player";

    private bool isPlayerNearObj = false;
    private PickableManager pickableManager;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        pickableManager = FindObjectOfType<PickableManager>();

        // If already collected in this save, don't exist anymore
        if (GameManager.Instance != null &&
            GameManager.Instance.IsPickupCollected(pickupID))
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        if (isPlayerNearObj && Input.GetKeyDown(KeyCode.E))
        {
            PickUp();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNearObj = true;
            pickableManager?.TogglePrompt(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNearObj = false;
            pickableManager?.TogglePrompt(false);
        }
    }

    private void PickUp()
    {
        if (pickableManager == null) return;

        // give the item to inventory / apply effect etc.
        pickableManager.HandlePickup(itemData);

        // Remember it's collected forever
        GameManager.Instance?.MarkPickupCollected(pickupID);

        pickableManager.TogglePrompt(false);

        Destroy(gameObject);
    }

    // called after load to force sync
    public void ApplyWorldState()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsPickupCollected(pickupID))
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Collider col = GetComponent<Collider>();
        if (col != null)
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
