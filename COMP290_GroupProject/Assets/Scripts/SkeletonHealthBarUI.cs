using UnityEngine;
using UnityEngine.UI;

public class SkeletonHealthBarUI : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private SkeletonController2D player;

    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;

    private void Start()
    {
        // Find the player automatically if necessary.
        if (player == null)
        {
            player = FindFirstObjectByType<SkeletonController2D>();
        }

        if (player == null)
        {
            Debug.LogError(
                "SkeletonHealthBarUI: Player reference is missing."
            );

            enabled = false;
            return;
        }

        if (healthSlider == null)
        {
            Debug.LogError(
                "SkeletonHealthBarUI: Health Slider is missing."
            );

            enabled = false;
            return;
        }

        // Configure the health bar.
        healthSlider.minValue = 0;
        healthSlider.maxValue = player.GetMaxHealth();
        healthSlider.wholeNumbers = true;

        UpdateHealthUI();
    }


    private void Update()
    {
        UpdateHealthUI();
    }


    private void UpdateHealthUI()
    {
        if (player == null || healthSlider == null)
            return;

        int currentHealth = player.GetCurrentHealth();
        int maximumHealth = player.GetMaxHealth();

        // Update the bar.
        healthSlider.maxValue = maximumHealth;
        healthSlider.value = currentHealth;

        // Update the text.
        if (healthText != null)
        {
            healthText.text =
                currentHealth + " / " + maximumHealth + " HP";
        }
    }
}