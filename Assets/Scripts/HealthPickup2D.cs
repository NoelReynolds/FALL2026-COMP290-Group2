using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthPickup2D : MonoBehaviour
{
    [Header("Health Pickup Settings")]
    [SerializeField, Min(1)]
    private int healAmount = 30;

    [Header("Healing Sound Effect")]
    [SerializeField]
    private AudioClip healingSound;

    [SerializeField, Range(0f, 1f)]
    private float soundVolume = 1f;

    [SerializeField, Min(0f)]
    private float soundPitch = 1f;

    private bool collected;

    private void Awake()
    {
        // Ensure the pickup uses a trigger collider.
        Collider2D pickupCollider = GetComponent<Collider2D>();
        pickupCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
        {
            return;
        }

        // Find the player, including when the collider belongs
        // to a child object of the player.
        SkeletonController2D player =
            other.GetComponentInParent<SkeletonController2D>();

        if (player == null)
        {
            return;
        }

        // Prevent the pickup from being collected more than once.
        collected = true;

        // Heal using the player's existing health system.
        player.Heal(healAmount);

        // Play the healing sound independently of the pickup object.
        if (healingSound != null)
        {
            GameObject soundObject = new GameObject("HealingPickupSound");

            soundObject.transform.position = transform.position;

            AudioSource audioSource =
                soundObject.AddComponent<AudioSource>();

            audioSource.clip = healingSound;
            audioSource.volume = soundVolume;
            audioSource.pitch = soundPitch;
            audioSource.spatialBlend = 0f;
            audioSource.Play();

            Destroy(
                soundObject,
                healingSound.length / Mathf.Max(soundPitch, 0.01f)
            );
        }

        // Remove the collected pickup.
        Destroy(gameObject);
    }
}