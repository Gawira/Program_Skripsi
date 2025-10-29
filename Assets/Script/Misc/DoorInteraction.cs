using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    [Header("Door Identity")]
    [Tooltip("Unique ID so this door can be remembered between loads.")]
    public string doorID = "door_shortcut_01";

    [Header("Door Settings")]
    public Animator doorAnimator;
    [Tooltip("Bool parameter in the Animator that controls whether the door is open.")]
    public string openParameter = "isOpen";
    public bool startOpen = false;

    [Header("Extras")]
    [Tooltip("Optional blocker object (like a collider or wall) that should disappear once the door is opened.")]
    public GameObject something;

    [Header("Messages")]
    [Tooltip("Shown when the player comes from the correct side and the door unlocks.")]
    public string openMessage = "Door opened.";

    [Tooltip("Shown when the player tries the wrong side before it's opened.")]
    public string blockedMessage = "This door can't be opened from this side.";

    private bool isOpen = false;

    private void Start()
    {
        // Has this door already been opened in this save?
        bool alreadyUnlocked = GameManager.Instance != null &&
                               GameManager.Instance.IsDoorOpened(doorID);

        isOpen = startOpen || alreadyUnlocked;
        ApplyDoorVisualState();

        if (alreadyUnlocked && something != null)
            something.SetActive(false);
    }

    // =========================
    // Trigger side that SHOULD open the door
    // (call this from a trigger script on the "good side")
    // =========================
    public void OnPlayerEnterOpenSide()
    {
        // Door is already open? Just don't spam UI.
        if (isOpen)
        {
            PromptUIManagerDoor.Instance?.HidePrompt();
            return;
        }

        // Show message
        PromptUIManagerDoor.Instance?.ShowPrompt(openMessage);

        // Permanently open and save state
        PermanentlyOpenDoor();

        // Hide text right after opening (optional style,
        // if you want it to stay on screen for a moment, remove this line)
        PromptUIManagerDoor.Instance?.HidePrompt();
    }

    // =========================
    // Trigger side that CANNOT open the door yet
    // (call this from a trigger script on the "blocked side")
    // =========================
    public void OnPlayerEnterBlockedSide()
    {
        if (isOpen)
        {
            // If door is already open, no need to scold player
            PromptUIManagerDoor.Instance?.HidePrompt();
            return;
        }

        // Tell the player this side is locked
        PromptUIManagerDoor.Instance?.ShowPrompt(blockedMessage);
    }

    // =========================
    // Player left either trigger zone
    // (call when OnTriggerExit from either side)
    // =========================
    public void OnPlayerExit()
    {
        PromptUIManagerDoor.Instance?.HidePrompt();
    }

    // =========================
    // Internals
    // =========================
    private void PermanentlyOpenDoor()
    {
        // Remember forever in save data
        GameManager.Instance?.MarkDoorOpened(doorID);

        isOpen = true;
        ApplyDoorVisualState();

        // Disable the blocker object now that shortcut is live
        if (something != null)
            something.SetActive(false);
    }

    private void ApplyDoorVisualState()
    {
        if (doorAnimator != null && !string.IsNullOrEmpty(openParameter))
        {
            doorAnimator.SetBool(openParameter, isOpen);
        }
    }

    // Called after loading a save to sync visuals with remembered state
    public void ApplyWorldState()
    {
        bool unlocked = GameManager.Instance != null &&
                        GameManager.Instance.IsDoorOpened(doorID);

        if (unlocked)
        {
            isOpen = true;
            ApplyDoorVisualState();

            if (something != null)
                something.SetActive(false);
        }
    }
}
