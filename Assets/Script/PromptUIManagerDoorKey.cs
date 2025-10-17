using UnityEngine;
using TMPro;

public class PromptUIManagerDoorKey: MonoBehaviour
{
    public static PromptUIManagerDoorKey Instance;

    [Header("UI References")]
    public GameObject promptUI;
    public TMP_Text promptText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    public void ShowPrompt(string message)
    {
        if (promptUI == null) return;
        if (promptText != null) promptText.text = message;
        promptUI.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptUI == null) return;
        promptUI.SetActive(false);
    }
}
