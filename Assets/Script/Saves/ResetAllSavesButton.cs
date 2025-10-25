using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResetAllSavesButton : MonoBehaviour
{
    [Header("UI")]
    public Button resetAllButton;

    [Header("Settings")]
    [Tooltip("How many save slots exist in the game.")]
    public int totalSlots = 3;

    [Tooltip("Reload the scene after clearing saves.")]
    public bool reloadSceneAfterDelete = true;

    private void Start()
    {
        if (resetAllButton != null)
            resetAllButton.onClick.AddListener(OnResetAllClicked);
    }

    public void OnResetAllClicked()
    {
        // Confirm in console
        Debug.Log("Reset All Saves button pressed — deleting all slots...");

        // Delete all slots
        for (int i = 0; i < totalSlots; i++)
        {
            SaveManager.DeleteSave(i);
        }

        Debug.Log("All save slots cleared!");

        // Optionally reload main menu or current scene
        if (reloadSceneAfterDelete)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
