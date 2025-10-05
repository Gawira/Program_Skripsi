using UnityEngine;


namespace UnityStandardAssets.Cameras
{
    public class MerchantSetting : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject merchantMenu;
        public GameObject merchantMenuPanel;
        public GameObject BuyItemPanel;
        public GameObject UpgradePanel;

        [SerializeField] private FreeLookCam freeLookCam;

        public bool isMerchant = false;

        void Update()
        {
        }

        public void Enter()
        {
            merchantMenu.SetActive(true);
            merchantMenuPanel.SetActive(true);
            BuyItemPanel.SetActive(false);
            UpgradePanel.SetActive(false);

            isMerchant = true;
        }
        public void Leave()
        {
            merchantMenu.SetActive(false);
            merchantMenuPanel.SetActive(false);
            Time.timeScale = 1f;
            isMerchant = false;
        }

        public void OpenBuy()
        {
            BuyItemPanel.SetActive(true);
            merchantMenuPanel.SetActive(false);
        }

        public void OpenUpgrade()
        {
            UpgradePanel.SetActive(true);
            merchantMenuPanel.SetActive(false);
        }

        public void BackToMainMenu()
        {
            BuyItemPanel.SetActive(false);
            UpgradePanel.SetActive(false);
        }

        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }
    }
}