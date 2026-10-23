using UnityEngine;
using UnityEngine.EventSystems;

public class SelectScreenController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject stageText;

    void Start()
    {
        stageText.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        stageText.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        stageText.SetActive(false);
    }
}
