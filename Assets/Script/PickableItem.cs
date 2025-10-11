using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickableItem : MonoBehaviour
{
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

        pickableManager.HandlePickup(itemData);
        pickableManager.TogglePrompt(false);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Collider col = GetComponent<Collider>();
        if (col != null)
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
