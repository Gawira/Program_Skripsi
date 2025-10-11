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
        [SerializeField] private MerchantManager merchantManager;

        void Start()
        {
            if (merchantManager == null)
                merchantManager = FindObjectOfType<MerchantManager>();
        }

        void Update()
        {
            // Optional: any special logic can now just check merchantManager.isMerchantOpen
            if (merchantManager != null && merchantManager.isMerchantOpen)
            {
                // Example: handle cursor state or other merchant-specific behavior here
            }
        }

        public void Enter()
        {
            merchantMenu.SetActive(true);
            merchantMenuPanel.SetActive(true);
            BuyItemPanel.SetActive(false);
            UpgradePanel.SetActive(false);
            merchantManager.isMerchantOpen = true;
        }

        public void Leave()
        {
            merchantMenu.SetActive(false);
            merchantMenuPanel.SetActive(false);
            merchantManager.isMerchantOpen =false;
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
