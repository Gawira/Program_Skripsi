using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorTrigger : MonoBehaviour
{
    public enum TriggerType { OpenSide, BlockedSide }
    public TriggerType triggerType;

    private DoorInteraction parentDoor;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        parentDoor = GetComponentInParent<DoorInteraction>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || parentDoor == null) return;

        if (triggerType == TriggerType.OpenSide)
            parentDoor.OnPlayerEnterOpenSide();
        else
            parentDoor.OnPlayerEnterBlockedSide();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || parentDoor == null) return;
        parentDoor.OnPlayerExit();
    }
}
