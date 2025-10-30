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
    public string gameplaySceneName = "SampleScene";
    public string introSceneName = "Intro";

    [Header("Audio")]
    public AudioClip silenceMusic; // optional: a silent track to clear menu music

    void OnEnable() => UpdateSlotText();

    void Start()
    {
        if (button != null) button.onClick.AddListener(OnSlotClicked);
        UpdateSlotText();
    }

    void UpdateSlotText()
    {
        bool exists = SaveManager.SaveExists(slotIndex);
        slotText.text = exists
            ? $"Slot {slotIndex + 1}\n[Continue]"
            : $"Slot {slotIndex + 1}\n[New Game]";
    }

    void OnSlotClicked()
    {
        SaveManager.SetActiveSlot(slotIndex);

        // Capture "brand new?" BEFORE we potentially create the save file.
        bool isNewGame = !SaveManager.SaveExists(slotIndex);

        // Optional: clear menu music
        if (AudioManager.Instance != null && silenceMusic != null)
            AudioManager.Instance.PlayAreaMusic(silenceMusic, true);

        if (isNewGame)
        {
            var newData = new SaveData
            {
                playerHealth = 100,
                currentHealth = 100,
                playerMoney = 0,
                damage = 10,
                lifesteal = 4,
                defense = 5,
                slotMax = 2,

                checkpointPosition = Vector3.zero,
                checkpointRotation = Quaternion.identity,

                equippedDjimatIDs = new System.Collections.Generic.List<string>(),
                inventoryDjimatIDs = new System.Collections.Generic.List<string>(),
                sacredStoneIDs = new System.Collections.Generic.List<string>(),
                keyItemIDs = new System.Collections.Generic.List<string>(),
                soldOutItems = new System.Collections.Generic.List<string>()
            };
            SaveManager.SaveGame(newData);
        }

        // Set desired spawn point for the gameplay scene (safe even if Intro loads first).
        SceneSpawnManager.Instance?.SetSpawnPoint("SpawnPoint");

        

        // One-time Intro when it's a New Game; otherwise go straight to gameplay.
        SceneManager.LoadScene(isNewGame ? introSceneName : gameplaySceneName);
    }
}
