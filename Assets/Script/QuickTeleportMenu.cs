using UnityEngine;
using UnityEngine.UI;

public class QuickTeleportMenu : MonoBehaviour
{
    [Header("Open/Close")]
    public KeyCode toggleKey = KeyCode.F4;
    public GameObject canvasRoot;                 // Assign a Canvas or Panel that contains the 4 buttons
    public bool pauseWhileOpen = false;           // Optional: pause game when menu is open
    public bool unlockCursorWhileOpen = true;     // Show/free cursor while the menu is open
    public KeyCode closeKey = KeyCode.Escape;     // Optional close key (ESC)

    [Header("Player")]
    public Transform player;                      // If null, will auto-find tag "Player"
    public bool matchRotation = true;             // Also copy rotation from destination

    [Header("Buttons")]
    public Button btnA;
    public Button btnB;
    public Button btnC;
    public Button btnD;

    [Header("Destinations")]
    public Transform destA;
    public Transform destB;
    public Transform destC;
    public Transform destD;

    private bool _isOpen;

    void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (canvasRoot != null)
            canvasRoot.SetActive(false);

        // Wire button listeners
        if (btnA) btnA.onClick.AddListener(() => TeleportTo(destA));
        if (btnB) btnB.onClick.AddListener(() => TeleportTo(destB));
        if (btnC) btnC.onClick.AddListener(() => TeleportTo(destC));
        if (btnD) btnD.onClick.AddListener(() => TeleportTo(destD));
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetOpen(!_isOpen);

        if (_isOpen && Input.GetKeyDown(closeKey))
            SetOpen(false);
    }

    private void SetOpen(bool open)
    {
        _isOpen = open;

        if (canvasRoot != null)
            canvasRoot.SetActive(open);

        if (pauseWhileOpen)
            Time.timeScale = open ? 0f : 1f;

        if (unlockCursorWhileOpen)
        {
            Cursor.visible = open;
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    public void TeleportTo(Transform target)
    {
        if (player == null || target == null) return;

        // If the player has a Rigidbody, clear velocity so they don't "slide" after teleport
        var rb = player.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // If using CharacterController, briefly disable to avoid step offset issues (optional)
        var cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        player.position = target.position;
        if (matchRotation) player.rotation = target.rotation;

        if (cc) cc.enabled = true;

        // Close menu after teleport
        if (_isOpen) SetOpen(false);
    }
}
