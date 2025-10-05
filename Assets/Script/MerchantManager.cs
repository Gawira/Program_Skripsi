using UnityEngine;
using UnityStandardAssets.Cameras;

public class MerchantManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player tag to detect")]
    public string playerTag = "Player";

    [Tooltip("Main merchant interaction prompt (e.g. 'Press E to Trade')")]
    public GameObject promptUI;

    [Tooltip("Distance in which the player can interact with the merchant")]
    public float interactDistance = 3f;

    [Header("Linked Systems")]
    public PauseSetting pauseSetting;
    public MerchantSetting merchantSetting;

    private Transform player;
    private bool isPlayerNear = false;
    private bool isMerchantOpen = false;

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        // Find player automatically by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("MerchantManager: No Player found with tag '" + playerTag + "'");

        // Auto-find PauseSetting and MerchantSetting if not set
        if (pauseSetting == null)
            pauseSetting = FindObjectOfType<PauseSetting>();

        if (merchantSetting == null)
            merchantSetting = FindObjectOfType<MerchantSetting>();
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);
        isPlayerNear = distance <= interactDistance;

        // Show “Press E” prompt if player nearby and merchant not open
        if (promptUI != null)
            promptUI.SetActive(isPlayerNear && !isMerchantOpen);

        // If player walks out of range, close merchant UI
        if (!isPlayerNear && isMerchantOpen)
        {
            CloseMerchantUI();
        }

        // Block ESC pause when merchant open
        if (isMerchantOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseMerchantUI();
            }
            return; // skip input while merchant open
        }

        // Open merchant with E
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            OpenMerchantUI();
        }
    }

    void OpenMerchantUI()
    {
        isMerchantOpen = true;

        // Disable pause input
        if (pauseSetting != null)
            pauseSetting.enabled = false;

        // Call the MerchantSetting to open UI
        if (merchantSetting != null)
            merchantSetting.Enter();

        // Hide prompt
        if (promptUI != null)
            promptUI.SetActive(false);

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseMerchantUI()
    {
        isMerchantOpen = false;

        // Re-enable pause system
        if (pauseSetting != null)
            pauseSetting.enabled = true;

        // Call the MerchantSetting to close UI
        if (merchantSetting != null)
            merchantSetting.Leave();

        // Show prompt if still near
        if (promptUI != null && isPlayerNear)
            promptUI.SetActive(true);

        // Lock cursor back
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);

#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, "Merchant Interact Range");
#endif
    }
}
