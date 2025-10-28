using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SilenceTrigger : MonoBehaviour
{
    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // tell AudioManager this area = silence
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAreaMusic(null, true);
        }
    }
}
