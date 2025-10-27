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
            SaveData newData = new SaveData
            {
                // --- Player base stats for a NEW GAME ---
                playerHealth = 100,
                currentHealth = 100,
                playerMoney = 0,
                damage = 10,
                lifesteal = 4,
                defense = 5,
                slotMax = 2,          // starting diamond slots / capacity

                // --- Spawn / checkpoint ---
                checkpointPosition = Vector3.zero,
                checkpointRotation = Quaternion.identity,

                // --- Empty inventories ---
                equippedDjimatIDs = new System.Collections.Generic.List<string>(),
                inventoryDjimatIDs = new System.Collections.Generic.List<string>(),
                sacredStoneIDs = new System.Collections.Generic.List<string>(),
                keyItemIDs = new System.Collections.Generic.List<string>(),
                soldOutItems = new System.Collections.Generic.List<string>()
            };

            SaveManager.SaveGame(newData);
        }


        // Set desired spawn point
        if (SceneSpawnManager.Instance != null)
            SceneSpawnManager.Instance.SetSpawnPoint("SpawnPoint");


        SceneSpawnManager.Instance?.SetSpawnPoint("SpawnPoint");
        SceneManager.LoadScene("SampleScene");
    }
}
