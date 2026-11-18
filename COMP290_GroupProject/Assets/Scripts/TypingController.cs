using UnityEngine;
using TMPro;
using System.Collections;

public class TypingController : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 20f;
    [SerializeField] private TextMeshProUGUI textMesh;
    private Coroutine typingCoroutine;
    [SerializeField] private TMP_Text textToType;
    private string tmpString;
    [SerializeField] private GameObject tutorialBox;

    private void StartTyping(TMP_Text textToType)
    {
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        tmpString = textToType.text.ToString();
        typingCoroutine = StartCoroutine(TypeTextRoutine(tmpString));
    }

    private IEnumerator TypeTextRoutine(string textToType)
    {
        textMesh.text = textToType;
        textMesh.maxVisibleCharacters = 0;

        textMesh.ForceMeshUpdate();
        int totalVisibleCharacters = textMesh.textInfo.characterCount;

        float delay = 1f / typingSpeed;

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            textMesh.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delay);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            StartTyping(textToType);
        }
    }
}
