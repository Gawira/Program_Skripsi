using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PromptTrigger : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";
    [TextArea]
    public string promptMessage = "[E] Interact";

    private void Start()
    {
        // Make sure the collider is a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            PromptUIManagerDoor.Instance?.ShowPrompt(promptMessage);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            PromptUIManagerDoor.Instance?.HidePrompt();
        }
    }
}
