using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LockedDoorInteraction : MonoBehaviour
{
    [Header("Door Identity")]
    [Tooltip("Unique ID for persistence, e.g. 'castle_gate_A'")]
    public string doorID = "door_01";

    [Header("Door Settings")]
    public Animator doorAnimator;
    [Tooltip("Animator bool parameter to mark door open state")]
    public string openParameter = "isOpen";
    public bool startOpen = false;

    [Header("Trigger Zone")]
    public Collider doorTrigger;
    public GameObject something; // e.g. barrier object to hide once opened

    [Header("Interaction Settings")]
    public string playerTag = "Player";
    [Tooltip("Key item required to unlock this door first time")]
    public DjimatItem requiredKeyItem;
    public string lockedMessage = "The door is locked. You need a key.";
    public string openedMessage = "[E] Open Door";

    [Header("Inventory Reference")]
    public KeyItemInventory keyItemInventory;

    private bool isOpen = false;
    private bool isInsideTrigger = false;

    private void Start()
    {
        if (doorTrigger != null)
            doorTrigger.isTrigger = true;

        // Check if this door was already opened in this save
        bool alreadyUnlocked = GameManager.Instance != null &&
                               GameManager.Instance.IsDoorOpened(doorID);

        isOpen = startOpen || alreadyUnlocked;
        ApplyVisualDoorState();

        if (alreadyUnlocked && something != null)
            something.SetActive(false);
    }

    private void Update()
    {
        if (!isInsideTrigger) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            bool canOpenNow =
                HasRequiredKey() ||
                (GameManager.Instance != null &&
                 GameManager.Instance.IsDoorOpened(doorID));

            if (canOpenNow)
            {
                UnlockAndOpenDoor();
                PromptUIManagerDoorKey.Instance?.HidePrompt();
            }
            else
            {
                PromptUIManagerDoorKey.Instance?.ShowPrompt(lockedMessage);
            }
        }
    }

    private bool HasRequiredKey()
    {
        if (requiredKeyItem == null) return false;
        if (keyItemInventory == null) return false;

        foreach (var key in keyItemInventory.keyItems)
        {
            if (key == requiredKeyItem ||
                (key != null && requiredKeyItem != null &&
                 key.itemName == requiredKeyItem.itemName))
            {
                return true;
            }
        }
        return false;
    }

    private void UnlockAndOpenDoor()
    {
        // remember it's unlocked forever
        GameManager.Instance?.MarkDoorOpened(doorID);

        isOpen = true;
        ApplyVisualDoorState();

        if (something != null)
            something.SetActive(false);
    }

    private void ApplyVisualDoorState()
    {
        if (doorAnimator != null && !string.IsNullOrEmpty(openParameter))
        {
            doorAnimator.SetBool(openParameter, isOpen);
        }
    }

    // called after load to sync to save state
    public void ApplyWorldState()
    {
        bool unlocked = GameManager.Instance != null &&
                        GameManager.Instance.IsDoorOpened(doorID);

        if (unlocked)
        {
            isOpen = true;
            ApplyVisualDoorState();

            if (something != null)
                something.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        isInsideTrigger = true;

        if (HasRequiredKey() ||
            (GameManager.Instance != null &&
             GameManager.Instance.IsDoorOpened(doorID)))
        {
            PromptUIManagerDoorKey.Instance?.ShowPrompt(openedMessage);
        }
        else
        {
            PromptUIManagerDoorKey.Instance?.ShowPrompt("[E] Check Door");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        isInsideTrigger = false;
        PromptUIManagerDoorKey.Instance?.HidePrompt();
    }
}
