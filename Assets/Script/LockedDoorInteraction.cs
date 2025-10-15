using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LockedDoorInteraction : MonoBehaviour
{
    [Header("Door Settings")]
    public Animator doorAnimator;
    [Tooltip("Animator parameter controlling door open state")]
    public string openParameter = "isOpen";
    public bool startOpen = false;

    [Header("Trigger Zones")]
    public Collider doorTrigger;
    public GameObject something; // Optional: hide object after open

    [Header("Interaction Settings")]
    public string playerTag = "Player";
    public string requiredKeyName = "Silver Key"; // the key name to check
    public string lockedMessage = "The door is locked. You need a key.";
    public string openedMessage = "[E] Open / Close Door";

    private bool isOpen = false;
    private bool isInsideTrigger = false;

    private KeyItemGridMaker playerKeyInventory;

    private void Start()
    {
        if (doorTrigger != null)
            doorTrigger.isTrigger = true;

        isOpen = startOpen;
        if (doorAnimator != null)
            doorAnimator.SetBool(openParameter, isOpen);
    }

    private void Update()
    {
        if (isInsideTrigger && Input.GetKeyDown(KeyCode.E))
        {
            if (HasRequiredKey())
            {
                ToggleDoor();
                if (something != null)
                    something.SetActive(false);
            }
            else
            {
                PromptUIManager.Instance?.ShowPrompt(lockedMessage);
            }
        }
    }

    private bool HasRequiredKey()
    {
        if (playerKeyInventory == null) return false;

        foreach (var slot in playerKeyInventory.keyItemGridParent.GetComponentsInChildren<KeyItemSlotUI>())
        {
            if (slot.assignedKeyItem != null && slot.assignedKeyItem.itemName == requiredKeyName)
            {
                return true;
            }
        }

        return false;
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;

        if (doorAnimator != null)
            doorAnimator.SetBool(openParameter, isOpen);

        PromptUIManager.Instance?.HidePrompt();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        isInsideTrigger = true;
        playerKeyInventory = other.GetComponentInChildren<KeyItemGridMaker>();

        if (HasRequiredKey())
            PromptUIManager.Instance?.ShowPrompt(openedMessage);
        else
            PromptUIManager.Instance?.ShowPrompt("[E] Check Door");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        isInsideTrigger = false;
        playerKeyInventory = null;
        PromptUIManager.Instance?.HidePrompt();
    }
}
