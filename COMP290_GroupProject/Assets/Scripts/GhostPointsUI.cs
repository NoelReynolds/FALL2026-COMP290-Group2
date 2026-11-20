using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class GhostPointsUI : MonoBehaviour
{
    [SerializeField] string format = "Ghost Points: {0} / {1}";

    TMP_Text label;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        var manager = GhostPointsManager.Instance;
        if (manager == null) return;

        manager.OnPointsChanged += Refresh;
        Refresh(manager.TotalPoints); 
    }

    void OnDisable()
    {
        if (GhostPointsManager.Instance != null)
            GhostPointsManager.Instance.OnPointsChanged -= Refresh;
    }

    void Refresh(int total)
    {
        label.text = string.Format(format, total, GhostPointsManager.MaxPoints);
    }
}
