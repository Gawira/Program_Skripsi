using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    [Header("Door Identity")]
    public string doorID = "door_shortcut_01";

    [Header("Door Settings")]
    public Animator doorAnimator;
    public string openParameter = "isOpen";
    public bool startOpen = false;

    [Header("Extras")]
    public GameObject something; // optional blocker object

    [Header("Messages")]
    public string openMessage = "Door opened.";
    public string blockedMessage = "This door can't be opened from this side.";

    private bool isOpen = false;

    private void Start()
    {
        bool alreadyUnlocked = GameManager.Instance != null &&
                               GameManager.Instance.IsDoorOpened(doorID);

        isOpen = startOpen || alreadyUnlocked;
        ApplyDoorVisualState();

        if (alreadyUnlocked && something != null)
            something.SetActive(false);
    }

    public void OnPlayerEnterOpenSide()
    {
        if (!isOpen)
        {
            PromptUIManager.Instance?.ShowPrompt(openMessage);
            PermanentlyOpenDoor();
            PromptUIManager.Instance?.HidePrompt();
        }
    }

    public void OnPlayerEnterBlockedSide()
    {
        if (!isOpen)
        {
            PromptUIManager.Instance?.ShowPrompt(blockedMessage);
        }
    }

    public void OnPlayerExit()
    {
        PromptUIManager.Instance?.HidePrompt();
    }

    private void PermanentlyOpenDoor()
    {
        GameManager.Instance?.MarkDoorOpened(doorID);

        isOpen = true;
        ApplyDoorVisualState();

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
