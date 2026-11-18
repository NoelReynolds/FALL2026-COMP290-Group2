using UnityEngine;

public class TutorialTextSpawner : MonoBehaviour
{
    [SerializeField] private GameObject tutorialBox;

    private void Start()
    {
        tutorialBox.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            tutorialBox.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            tutorialBox.SetActive(false);
        }
    }
}
