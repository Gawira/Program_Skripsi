using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AmbientZoneToggle : MonoBehaviour
{
    [Tooltip("Assign the AmbientLoop object you want to control")]
    public AmbientLoop ambientToToggle;

    [Tooltip("If true, turns it ON when player enters. If false, turns it OFF when player enters.")]
    public bool turnOnWhenEnter = true;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (ambientToToggle == null) return;

        var src = ambientToToggle.GetComponent<AudioSource>();
        if (src == null) return;

        if (turnOnWhenEnter)
        {
            src.enabled = true;
            if (!src.isPlaying && ambientToToggle.enabled)
                src.Play();
        }
        else
        {
            src.Stop();
            src.enabled = false;
        }
    }
}
