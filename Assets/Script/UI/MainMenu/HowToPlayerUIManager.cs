using UnityEngine;

public class HowToPlayUIManager : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject movementPanel;
    [SerializeField] private GameObject lockOnPanel;
    [SerializeField] private GameObject evadePanel;
    [SerializeField] private GameObject combatPanel;

    private void Awake()
    {
        // If not manually assigned in Inspector, try to find them by name
        if (movementPanel == null) movementPanel = transform.Find("Movement")?.gameObject;
        if (lockOnPanel == null) lockOnPanel = transform.Find("Lock-On")?.gameObject;
        if (evadePanel == null) evadePanel = transform.Find("Evade")?.gameObject;
        if (combatPanel == null) combatPanel = transform.Find("Combat")?.gameObject;
    }

    private void Start()
    {
        // Show movement panel by default
        ShowPanel(movementPanel);
    }

    public void ShowMovement()
    {
        ShowPanel(movementPanel);
    }

    public void ShowLockOn()
    {
        ShowPanel(lockOnPanel);
    }

    public void ShowEvade()
    {
        ShowPanel(evadePanel);
    }

    public void ShowCombat()
    {
        ShowPanel(combatPanel);
    }

    public void BackToMenu()
    {
        // Hide everything when back is pressed
        movementPanel.SetActive(false);
        lockOnPanel.SetActive(false);
        evadePanel.SetActive(false);
        combatPanel.SetActive(false);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        // Hide all
        movementPanel.SetActive(false);
        lockOnPanel.SetActive(false);
        evadePanel.SetActive(false);
        combatPanel.SetActive(false);

        // Show chosen panel
        if (panelToShow != null)
            panelToShow.SetActive(true);
    }
}
