using UnityEngine;
using UnityEngine.SceneManagement;

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
        public GameObject inventoryhelpPanel;

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
            if (inventoryhelpPanel != null) inventoryhelpPanel.SetActive(false);

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
            // 1) refresh all inventory-related UIs
            RefreshInventoryUI();

            // 2) then show panels
            inventoryPanel.SetActive(true);
            pauseMenuPanel.SetActive(false);
            HTPScreen.SetActive(false);
            if (inventoryhelpPanel != null) inventoryhelpPanel.SetActive(false);
        }

        public void OpenInventoryHelp()
        {
            if (inventoryhelpPanel != null) inventoryhelpPanel.SetActive(true);
            inventoryPanel.SetActive(false);
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
            SceneManager.LoadScene("MainMenu");
            inventoryPanel.SetActive(false);
            settingsPanel.SetActive(false);
            HTPScreen.SetActive(false);
            if (inventoryhelpPanel != null) inventoryhelpPanel.SetActive(false);
        }

        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }

        // -----------------------------
        // Refresh everything shown in the Inventory screen
        // -----------------------------
        private void RefreshInventoryUI()
        {
            // Djimat Equipped & Inventory grids (via GridMaker)
            var grid = FindObjectOfType<GridMaker>();
            if (grid != null)
            {
                // Equipped slots
                var equippedSlots = grid.equippedGridParent.GetComponentsInChildren<EquippedSlotUI>(true);
                foreach (var eq in equippedSlots)
                {
                    // Re-apply same reference to force icon/state refresh
                    eq.AssignDjimat(eq.equippedDjimat);
                }

                // Inventory slots
                var invSlots = grid.inventoryGridParent.GetComponentsInChildren<InventorySlotUI>(true);
                foreach (var inv in invSlots)
                {
                    inv.AssignDjimat(inv.assignedDjimat); // Assign triggers UpdateUI()
                }
            }

            // Sacred Stones (use its grid maker’s proper rebuild)
            var sacred = FindObjectOfType<SacredStoneGridMaker>();
            if (sacred != null)
            {
                sacred.RefreshGrid();
            }

            // Key Items (no grid maker provided here, so refresh all slots in scene)
            var keySlots = FindObjectsOfType<KeyItemSlotUI>(true);
            foreach (var key in keySlots)
            {
                key.AssignKeyItem(key.assignedKeyItem);
            }
        }
    }
}
