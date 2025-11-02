using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PitfallDeathZone : MonoBehaviour
{
    public enum Mode
    {
        KillWithDamage,   // uses TakeDamage (respects invincibility if you want)
        SetHealthToZero,  // bypasses invincibility; lets PlayerManager handle death screen
        InstantRespawn    // skips death screen; teleports back to checkpoint
    }

    [Header("Detection")]
    public string playerTag = "Player";

    [Header("Behaviour")]
    public Mode behavior = Mode.SetHealthToZero;
    [Tooltip("Only used in KillWithDamage mode.")]
    public bool respectInvincibility = true;
    [Tooltip("Only used in KillWithDamage mode.")]
    public int lethalDamage = 999999;

    [Header("Feedback (optional)")]
    public AudioClip fallSfx;
    public GameObject particlePrefab;
    public Vector3 particleOffset = Vector3.zero;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        var pm = other.GetComponent<PlayerManager>();
        if (pm == null) return;

        // SFX / VFX
        if (fallSfx) AudioManager.Instance?.PlaySFXAtPoint(fallSfx, other.transform.position);
        if (particlePrefab) Instantiate(particlePrefab, other.transform.position + particleOffset, Quaternion.identity);

        switch (behavior)
        {
            case Mode.KillWithDamage:
                if (respectInvincibility)
                {
                    pm.TakeDamage(lethalDamage); // may be ignored if invincible
                }
                else
                {
                    pm.currentHealth = 0;        // bypass invincibility, triggers death flow on next Update
                }
                break;

            case Mode.SetHealthToZero:
                pm.currentHealth = 0;            // PlayerManager.Update() will call Die()
                break;

            case Mode.InstantRespawn:
                pm.money = 0;                    // mimic your death penalty
                pm.Respawn();                    // skip YOU DIED screen
                break;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // visualize trigger bounds
        var col = GetComponent<Collider>();
        if (!col) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
#endif
}
