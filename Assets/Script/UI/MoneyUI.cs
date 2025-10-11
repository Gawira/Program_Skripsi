using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerManager playerManager;   // Drag your PlayerManager here
    [SerializeField] private TextMeshProUGUI moneyText;     // Drag TMP Text here

    void Start()
    {

        playerManager = FindObjectOfType<PlayerManager>();

        UpdateMoneyUI(); 
    }

    void Update()
    {
        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        if (playerManager != null && moneyText != null)
        {
            moneyText.text = $"{playerManager.money}";
        }
    }
}