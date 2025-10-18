using UnityEngine;


namespace UnityStandardAssets.Cameras
{
    public class PauseSetting : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainpauseMenu;
        public GameObject pauseMenuPanel;
        public GameObject inventoryPanel;
        public GameObject settingsPanel;
        public GameObject HTPScreen;

        [SerializeField] public FreeLookCam freeLookCam;

        public bool isPaused = false;
        void Start()
        {
            if (freeLookCam == null)
                freeLookCam = FindObjectOfType<FreeLookCam>();
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                
                if (isPaused)
                {
                    Resume();
                    freeLookCam.UpdateCursorState();
                }
                else
                {
                    Pause();
                    freeLookCam.UpdateCursorState();
                } 
            }
        }

        public void Pause()
        {
            mainpauseMenu.SetActive(true);
            pauseMenuPanel.SetActive(true);
            inventoryPanel.SetActive(false);
            settingsPanel.SetActive(false);
            HTPScreen.SetActive(false);

            isPaused = true;
        }

        public void Resume()
        {
            mainpauseMenu.SetActive(false);
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
        }

        public void OpenInventory()
        {
            inventoryPanel.SetActive(true);
            pauseMenuPanel.SetActive(false);
            HTPScreen.SetActive(false);
        }

        public void OpenSettings()
        {
            settingsPanel.SetActive(true);
            pauseMenuPanel.SetActive(false);
            HTPScreen.SetActive(false);
        }

        public void OpenHTP()
        {
            HTPScreen.SetActive(true);
            settingsPanel.SetActive(false);
            pauseMenuPanel.SetActive(false);
        }

        public void BackToMainMenu()
        {
            inventoryPanel.SetActive(false);
            settingsPanel.SetActive(false);
            HTPScreen.SetActive(false);
        }

        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }
    }
}