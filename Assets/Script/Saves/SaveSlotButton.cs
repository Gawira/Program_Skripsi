using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveSlotButton : MonoBehaviour
{
    [Header("Slot Settings")]
    public int slotIndex;

    [Header("UI")]
    public Button button;
    public TMPro.TMP_Text slotText;

    [Header("Game Settings")]
    public string gameSceneName = "GameScene"; // your in-game scene

    private void Start()
    {
        if (button != null)
            button.onClick.AddListener(OnSlotClicked);

        UpdateSlotText();
    }

    private void UpdateSlotText()
    {
        if (SaveManager.SaveExists(slotIndex))
        {
            slotText.text = $"Slot {slotIndex + 1}\n[Continue]";
        }
        else
        {
            slotText.text = $"Slot {slotIndex + 1}\n[New Game]";
        }
    }

    private void OnSlotClicked()
    {
        SaveManager.SetActiveSlot(slotIndex);

        if (!SaveManager.SaveExists(slotIndex))
        {
            // Create new save data
            SaveData newData = new SaveData
            {
                playerHealth = 100,
                playerMoney = 0,
                checkpointPosition = Vector3.zero
            };
            SaveManager.SaveGame(newData);
        }

        SceneManager.LoadScene(gameSceneName);
    }
}
