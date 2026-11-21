using TMPro;
using UnityEngine;

public class TutorialTextSpawner : MonoBehaviour
{
    [SerializeField] private GameObject tutorialBox;
    private TypingController typeText;
    [SerializeField] private TMP_Text textToType;

    private void Start()
    {
        tutorialBox.SetActive(false);
        typeText = GetComponent<TypingController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (tutorialBox != null)
            {
                tutorialBox.SetActive(true);
                typeText.StartTyping(textToType);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (tutorialBox != null)
            {
                tutorialBox.SetActive(false);
            }
        }
    }
}
