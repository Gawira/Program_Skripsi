using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class OutroTrigger : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";   // tag on your player object
    public string outroSceneName = "Outro";

    private void Start()
    {
        // make sure this collider is a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // only react to the player
        if (!other.CompareTag(playerTag)) return;

        // load the outro scene
        SceneManager.LoadScene(outroSceneName);
    }
}
