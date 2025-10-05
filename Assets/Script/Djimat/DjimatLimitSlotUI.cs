using UnityEngine;
using UnityEngine.UI;

public class DjimatLimitSlotUI : MonoBehaviour
{
    [SerializeField] private Image bg; // darker background (capacity available)
    [SerializeField] private Image fill; // lighter fill (capacity used)

    public void SetActive(bool hasCapacity)
    {
        if (bg != null) bg.enabled = hasCapacity;
        if (fill != null) fill.enabled = false;
    }

    public void SetUsed(bool isUsed)
    {
        if (fill != null) fill.enabled = isUsed;
    }
}
