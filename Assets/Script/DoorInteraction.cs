using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorInteraction : MonoBehaviour
{
    [Header("Door Settings")]
    public Animator doorAnimator;
    [Tooltip("Animator parameter name controlling the door open state")]
    public string openParameter = "isOpen";
    public bool startOpen = false;

    [Header("Trigger Zones")]
    public Collider openTrigger;       // Trigger to open/close door
    public Collider blockedTrigger;    // Trigger to show message
    public GameObject something;

    [Header("Interaction Settings")]
    public string playerTag = "Player";
    public string blockedMessage = "This door can't be opened from this side.";

    private bool isOpen;
    private bool isInsideOpenTrigger = false;
    private bool isInsideBlockedTrigger = false;

    void Start()
    {
        // Make sure triggers are set as triggers
        if (openTrigger != null) openTrigger.isTrigger = true;
        if (blockedTrigger != null) blockedTrigger.isTrigger = true;

        // Set initial door state
        isOpen = startOpen;
        if (doorAnimator != null)
            doorAnimator.SetBool("isOpen", false);
    }

    void Update()
    {
        // Handle open trigger interaction
        if (isInsideOpenTrigger)
        {
            ToggleDoor();
            something.SetActive(false);
        }

        // Handle blocked trigger interaction
        if (isInsideBlockedTrigger)
        {
            PromptUIManager.Instance?.ShowPrompt(blockedMessage);
        }

        
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;
        if (doorAnimator != null)
            doorAnimator.SetBool("isOpen", true);
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        if (!other.CompareTag(playerTag)) return;

        // Check which trigger the player entered
        if (openTrigger)
        {
            isInsideOpenTrigger = true;
            Debug.Log("isInsideOpenTrigger" + isInsideOpenTrigger);
            PromptUIManager.Instance?.ShowPrompt("[E] Open / Close Door");
        }
        else if (blockedTrigger)
        {
            isInsideBlockedTrigger = true;
            Debug.Log("isInsideBlockedTrigger" + isInsideBlockedTrigger);
            PromptUIManager.Instance?.ShowPrompt("[E] Interact");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (other == openTrigger)
            isInsideOpenTrigger = false;
        else if (other == blockedTrigger)
            isInsideBlockedTrigger = false;

        // Always hide prompt on exit
        PromptUIManager.Instance?.HidePrompt();
    }
}
