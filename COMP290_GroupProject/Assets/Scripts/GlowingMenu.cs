using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GlowingMenu : MonoBehaviour
{
    public Color whiteColor = Color.white;
    public Color redColor = Color.red;
    public Color greenColor = Color.green;
    public Color blueColor = Color.blue;

    public float duration = 5.0f;
    [SerializeField] private Image optionsPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        optionsPanel = GetComponent<Image>();
    }

    public void StartChangingColor()
    {
        StopAllCoroutines();
        StartCoroutine(RedColorChange(redColor, duration));
    }

    private IEnumerator RedColorChange(Color red, float duration)
    {
        Color startColor = optionsPanel.color;
        float elapsedTime = 0f;

        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float completion = Mathf.Clamp01(elapsedTime / duration);

            optionsPanel.color = Color.Lerp(startColor, red, completion);

            yield return null;
        }

        optionsPanel.color = red;
        StartCoroutine(GreenColorChange(greenColor, duration));
    }

    private IEnumerator GreenColorChange(Color green, float duration)
    {
        Color startColor = optionsPanel.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float completion = Mathf.Clamp01(elapsedTime / duration);

            optionsPanel.color = Color.Lerp(startColor, green, completion);

            yield return null;
        }

        optionsPanel.color = green;
        StartCoroutine(BlueColorChange(blueColor, duration));
    }

    private IEnumerator BlueColorChange(Color blue, float duration)
    {
        Color startColor = optionsPanel.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float completion = Mathf.Clamp01(elapsedTime / duration);

            optionsPanel.color = Color.Lerp(startColor, blue, completion);

            yield return null;
        }

        optionsPanel.color = blue;
        StartCoroutine(WhiteColorChange(whiteColor, duration));
    }

    private IEnumerator WhiteColorChange(Color white, float duration)
    {
        Color startColor = optionsPanel.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float completion = Mathf.Clamp01(elapsedTime / duration);

            optionsPanel.color = Color.Lerp(startColor, white, completion);

            yield return null;
        }

        optionsPanel.color = white;
        StartCoroutine(RedColorChange(redColor, duration));
    }


}
