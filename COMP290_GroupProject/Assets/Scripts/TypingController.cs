using UnityEngine;
using TMPro;
using System.Collections;

public class TypingController : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 20f;
    [SerializeField] private TextMeshProUGUI textMesh;
    private Coroutine typingCoroutine;
    private string tmpString;


    public void StartTyping(TMP_Text textToType)
    {
        if (typingCoroutine != null)
        {
            Debug.Log($"In If Statement:\n{textToType.text}");
            StopCoroutine(typingCoroutine);
        }
        tmpString = textToType.text;
        typingCoroutine = StartCoroutine(TypeTextRoutine(tmpString));
    }

    private IEnumerator TypeTextRoutine(string tmpString)
    {
        textMesh.text = tmpString;
        textMesh.maxVisibleCharacters = 0;

        textMesh.ForceMeshUpdate();
        int totalVisibleCharacters = textMesh.textInfo.characterCount;
        Debug.Log($"Number of visible characters: {totalVisibleCharacters.ToString()}");

        float delay = 1f / typingSpeed;

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            textMesh.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delay);
        }
    }
}
