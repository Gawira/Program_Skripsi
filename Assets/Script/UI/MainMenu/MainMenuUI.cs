using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu Panels")]
    public GameObject titleScreen;
    public GameObject settingScreen;
    public GameObject creditScreen;
    public GameObject saveSlotScreen;
    public GameObject HTPScreen;

    private void Start()
    {
        // Only show TitleScreen at start
        titleScreen.SetActive(true);
        settingScreen.SetActive(false);
        creditScreen.SetActive(false);
        saveSlotScreen.SetActive(false);
        HTPScreen.SetActive(false);

        // Make sure the cursor is visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenPlay()
    {
        titleScreen.SetActive(false);
        settingScreen.SetActive(false);
        creditScreen.SetActive(false);
        saveSlotScreen.SetActive(true);
        HTPScreen.SetActive(false);
    }

    public void OpenSettings()
    {
        titleScreen.SetActive(false);
        settingScreen.SetActive(true);
        creditScreen.SetActive(false);
        saveSlotScreen.SetActive(false);
        HTPScreen.SetActive(false);
    }

    public void OpenHTP()
    {
        titleScreen.SetActive(false);
        settingScreen.SetActive(false);
        creditScreen.SetActive(false);
        saveSlotScreen.SetActive(false);
        HTPScreen.SetActive(true);
    }

    public void OpenCredit()
    {
        titleScreen.SetActive(false);
        settingScreen.SetActive(false);
        creditScreen.SetActive(true);
        saveSlotScreen.SetActive(false);
        HTPScreen.SetActive(false);
    }

    public void BackToMain()
    {
        titleScreen.SetActive(true);
        settingScreen.SetActive(false);
        creditScreen.SetActive(false);
        saveSlotScreen.SetActive(false);
        HTPScreen.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();

        // If testing in the editor:
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
