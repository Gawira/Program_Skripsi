using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemInfoDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text statsText; // optional

    private void Start()
    {
        ClearInfo();
    }
    public void ShowInfo(DjimatItem item)
    {
        if (item == null)
        {
            ClearInfo();
            return;
        }

        if (iconImage)
        {
            iconImage.sprite = item.icon;
            iconImage.gameObject.SetActive(true); // show the icon again
        }

        if (nameText) nameText.text = item.itemName;
        if (descriptionText) descriptionText.text = item.description;
        if (statsText)
            statsText.text = $"HP +{item.healthBonus}\nDMG +{item.damageBonus}\nSlots: {item.slotCost}";
    }

    public void ClearInfo()
    {
        if (iconImage)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
        }
        if (nameText) nameText.text = "";
        if (descriptionText) descriptionText.text = "";
        if (statsText) statsText.text = "";
    }
}
