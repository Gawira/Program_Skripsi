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
    [HideInInspector] public bool isMerchantOpen = false;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            player = playerObj.transform;

        if (pauseSetting == null)
            pauseSetting = FindObjectOfType<PauseSetting>();

        if (merchantSetting == null)
            merchantSetting = FindObjectOfType<MerchantSetting>();
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        isPlayerNear = distance <= interactDistance;

        // Show / hide prompt based on distance and state
        if (promptUI != null)
            promptUI.SetActive(isPlayerNear && !isMerchantOpen);

        // Close if player walks away
        if (!isPlayerNear && isMerchantOpen)
        {
            CloseMerchantUI();
            if (promptUI != null) promptUI.SetActive(false);
        }

        // Pause menu near merchant
        if (isPlayerNear && !isMerchantOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseSetting.isPaused) pauseSetting.Resume();
            else pauseSetting.Pause();
            pauseSetting.freeLookCam.UpdateCursorState();
        }

        // Close merchant with Escape
        if (isMerchantOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMerchantUI();
            return;
        }

        // Open merchant with E
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
            OpenMerchantUI();
    }

    private void OpenMerchantUI()
    {
        isMerchantOpen = true;

        if (pauseSetting != null)
            pauseSetting.enabled = false;

        if (merchantSetting != null)
            merchantSetting.Enter();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseMerchantUI()
    {
        isMerchantOpen = false;

        if (pauseSetting != null)
            pauseSetting.enabled = true;

        if (merchantSetting != null)
            merchantSetting.Leave();

        if (promptUI != null && isPlayerNear)
            promptUI.SetActive(true);

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
