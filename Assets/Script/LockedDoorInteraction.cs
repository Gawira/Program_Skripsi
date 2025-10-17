using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LockedDoorInteraction : MonoBehaviour
{
    [Header("Door Settings")]
    public Animator doorAnimator;
    [Tooltip("Animator parameter controlling door open state")]
    public string openParameter = "isOpen";
    public bool startOpen = false;

    [Header("Trigger Zone")]
    public Collider doorTrigger;
    public GameObject something; // Optional: deactivate object after opening

    [Header("Interaction Settings")]
    public string playerTag = "Player";
    [Tooltip("The required key item to unlock this door")]
    public DjimatItem requiredKeyItem;   // reference the actual ScriptableObject
    public string lockedMessage = "The door is locked. You need a key.";
    public string openedMessage = "[E] Open / Close Door";

    [Header("Inventory Reference")]
    public KeyItemInventory keyItemInventory;

    private bool isOpen = false;
    private bool isInsideTrigger = false;

    private void Start()
    {
        if (doorTrigger != null)
            doorTrigger.isTrigger = true;

        // Set initial door state
        isOpen = startOpen;
        if (doorAnimator != null)
            doorAnimator.SetBool(openParameter, isOpen);
    }

    private void Update()
    {
        if (isInsideTrigger)
        {
            if (HasRequiredKey())
            {
                ToggleDoor();
                if (something != null)
                    something.SetActive(false);
            }
            else
            {
                PromptUIManagerDoorKey.Instance?.ShowPrompt(lockedMessage);
            }
        }
    }

    private bool HasRequiredKey()
    {
        if (keyItemInventory == null || requiredKeyItem == null) return false;

        foreach (var key in keyItemInventory.keyItems)
        {
            if (key == requiredKeyItem)
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
            doorAnimator.SetBool("isOpen", true);

        PromptUIManagerDoorKey.Instance?.HidePrompt();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        isInsideTrigger = true;

        if (HasRequiredKey())
            PromptUIManagerDoorKey.Instance?.ShowPrompt(openedMessage);
        else
            PromptUIManagerDoorKey.Instance?.ShowPrompt("[E] Check Door");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        isInsideTrigger = false;
        PromptUIManagerDoorKey.Instance?.HidePrompt();
    }
}
