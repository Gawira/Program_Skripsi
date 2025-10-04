using UnityEngine;
using System.Collections.Generic;

public class DjimatLimitUI : MonoBehaviour
{
    [SerializeField] private GameObject limitSlotPrefab;
    [SerializeField] private Transform limitParent;

    public List<DjimatLimitSlotUI> slots = new List<DjimatLimitSlotUI>();

    public void GenerateSlots(int capacity)
    {
        // clear existing
        foreach (Transform child in limitParent)
            Destroy(child.gameObject);

        slots.Clear();

        // create new
        for (int i = 0; i < capacity; i++)
        {
            var go = Instantiate(limitSlotPrefab, limitParent);
            var ui = go.GetComponent<DjimatLimitSlotUI>();
            if (ui != null)
            {
                ui.SetActive(true);
                slots.Add(ui);
            }
        }
    }

    public void UpdateUsage(int used)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetUsed(i < used);
        }
    }
}
