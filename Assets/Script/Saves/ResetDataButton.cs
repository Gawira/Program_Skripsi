using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResetDataButton : MonoBehaviour
{
    [Header("UI")]
    public Button resetButton;

    [Header("Behavior")]
    public bool reloadSceneAfterDelete = true;

    void Start()
    {
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetClicked);
    }

    public void OnResetClicked()
    {
        // You can add your own confirmation UI here before deleting
        SaveManager.DeleteSave();

        // Optional: reload scene so runtime state resets
        if (reloadSceneAfterDelete)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
