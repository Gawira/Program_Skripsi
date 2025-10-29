using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PickableManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject promptUI;
    public GameObject notificationUI;
    public TMP_Text notificationText;
    public Image notificationImage;

    private PlayerManager playerManager;
    private GridMaker gridMaker;
    private SacredStoneGridMaker sacredStoneGridMaker;
    private KeyItemGridMaker keyItemGridMaker;
    private DjimatLimitUI limitUI;

    private bool isNotificationActive = false;
    private bool isPickedUp = false;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        if (notificationUI != null)
            notificationUI.SetActive(false);


        playerManager = FindObjectOfType<PlayerManager>();
        gridMaker = FindObjectOfType<GridMaker>();
        sacredStoneGridMaker = FindObjectOfType<SacredStoneGridMaker>();
        keyItemGridMaker = FindObjectOfType<KeyItemGridMaker>();
        limitUI = FindObjectOfType<DjimatLimitUI>();
    }

    private void Update()
    {
        //if (isPickedUp && !isNotificationActive && Input.GetKeyDown(KeyCode.E))
        //{
        //    notificationUI.SetActive(true);
        //    isNotificationActive = true;
        //    isPickedUp = false;  
        //}

        if (isNotificationActive && Input.GetMouseButtonDown(0))
        {
            notificationUI.SetActive(false);
            isNotificationActive = false;
            Debug.Log("HELP WHY IS IT NOT DOING IT");
        }
    }

    public void TogglePrompt(bool active)
    {
        if (promptUI != null)
            promptUI.SetActive(active && !isNotificationActive);
    }

    public void HandlePickup(DjimatItem itemData)
    {
        isPickedUp =true;

        if (itemData == null) return;

        switch (itemData.itemType)
        {
            case DjimatItem.ItemType.Money:
                playerManager.money += itemData.plusMoney;
                ShowNotification($"{itemData.itemName}", itemData.icon);
                break;

            case DjimatItem.ItemType.Djimat:
                gridMaker?.AddToInventory(itemData);
                ShowNotification(itemData.itemName, itemData.icon);
                break;

            case DjimatItem.ItemType.SacredStone:
                sacredStoneGridMaker?.AddToInventory(itemData);
                ShowNotification(itemData.itemName, itemData.icon);
                break;

            case DjimatItem.ItemType.KeyItem:
                keyItemGridMaker?.AddToInventory(itemData);
                ShowNotification(itemData.itemName, itemData.icon);
                break;

            case DjimatItem.ItemType.DiamondSlot:
                playerManager.slotMax += itemData.plusslotCost;
                limitUI.GenerateSlots(playerManager.slotMax);
                ShowNotification($"+{itemData.plusslotCost} Djimat Slot", itemData.icon);
                break;
        }
    }

    private void ShowNotification(string message, Sprite icon)
    {
        if (notificationUI == null || notificationText == null)
        {
            Debug.Log(message);
            return;
        }

        notificationUI.SetActive(true);
        notificationText.text = message;

        if (notificationImage != null && icon != null)
        {
            notificationImage.sprite = icon;
            notificationImage.enabled = true;
        }
        else if (notificationImage != null)
        {
            notificationImage.enabled = false;
        }

        isNotificationActive = true;
    }
}
