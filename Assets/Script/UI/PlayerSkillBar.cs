using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillBar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider skillSlider;

    [Header("References")]
    [SerializeField] private PlayerManager player;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private PlayerAudioController audioController; // NEW

    [Header("Skill Settings")]
    [SerializeField] private float gainPerHit = 10f;
    [SerializeField] private KeyCode activateKeyLeft = KeyCode.LeftControl;
    [SerializeField] private KeyCode activateKeyRight = KeyCode.RightControl;
    [SerializeField] private string specialTrigger = "Special";
    [SerializeField] private float uiLerpSpeed = 10f;

    private float _visualValue;
    private bool _readyAnnounced = false;

    private void Awake()
    {
        if (!player) player = FindObjectOfType<PlayerManager>();
        if (!playerAnimator && player) playerAnimator = player.GetComponent<Animator>();
        if (!audioController && player) audioController = player.GetComponent<PlayerAudioController>(); // NEW
    }

    private void Start()
    {
        if (player.skillBarMax <= 0f) player.skillBarMax = 100f;

        if (skillSlider)
        {
            skillSlider.maxValue = player.skillBarMax;
            _visualValue = Mathf.Clamp(player.skillBar, 0f, player.skillBarMax);
            skillSlider.value = _visualValue;
        }
    }

    private void Update()
    {
        if (skillSlider && skillSlider.maxValue != player.skillBarMax)
            skillSlider.maxValue = player.skillBarMax;

        float target = Mathf.Clamp(player.skillBar, 0f, player.skillBarMax);
        _visualValue = Mathf.Lerp(_visualValue, target, Time.deltaTime * uiLerpSpeed);
        if (skillSlider) skillSlider.value = _visualValue;

        bool full = target >= player.skillBarMax - 0.001f;

        if (full && !_readyAnnounced)
        {
            _readyAnnounced = true;
            audioController?.PlaySkillReadySFX(); // moved here
        }
        if (!full) _readyAnnounced = false;

        if (full && (Input.GetKeyDown(activateKeyLeft) || Input.GetKeyDown(activateKeyRight)))
            UseSkill();
    }

    public void RewardOnHit() => AddCharge(gainPerHit);

    public void AddCharge(float amount)
    {
        player.skillBar = Mathf.Clamp(player.skillBar + amount, 0f, player.skillBarMax);
    }

    private void UseSkill()
    {
        if (playerAnimator) playerAnimator.SetTrigger(specialTrigger);
        player.skillBar = 0f;
        //audioController?.PlaySkillUseSFX(); // moved here
    }
}
