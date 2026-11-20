using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class GhostCollectable : MonoBehaviour
{
    [Header("Points")]
    [SerializeField] int pointsPerPickup = 1;

    [Header("Save ID (leave blank = automatic)")]
    [Tooltip("Identifies this ghost in the save data. Blank = scene name + position, " +
             "which is fine as long as you don't move the ghost after playing.")]
    [SerializeField] string ghostId = "";

    [Header("Pickup sound")]
    [SerializeField] AudioClip pickupSound;
    [Range(0f, 1f)][SerializeField] float pickupVolume = 1f;

    [Header("Player")]
    [SerializeField] string playerTag = "Player";

    string id;
    bool collected;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Start()
    {
        id = string.IsNullOrEmpty(ghostId)
            ? $"{SceneManager.GetActiveScene().name}_{transform.position.x:F1}_{transform.position.y:F1}"
            : ghostId;

        var manager = GhostPointsManager.Instance;
        if (manager != null && manager.IsCollected(id))
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag(playerTag)) return;
        Collect();
    }

    void Collect()
    {
        collected = true;

        GhostPointsManager.Instance.AddPoints(id, pointsPerPickup);
        PlaySound();

        Destroy(gameObject);
    }

    void PlaySound()
    {
        if (pickupSound == null) return;

        var temp = new GameObject("GhostPickupSound");
        var source = temp.AddComponent<AudioSource>();
        source.clip = pickupSound;
        source.volume = pickupVolume;
        source.spatialBlend = 0f;
        source.Play();
        Destroy(temp, pickupSound.length + 0.1f);
    }
}