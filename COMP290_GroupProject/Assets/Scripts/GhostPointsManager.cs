using System;
using System.Collections.Generic;
using UnityEngine;

public class GhostPointsManager : MonoBehaviour
{
    public static GhostPointsManager Instance { get; private set; }

    public const int MaxPoints = 100;


    const bool ResetOnEditorPlay = true;

    const string TotalKey = "GhostPoints_Total";
    const string CollectedKey = "GhostPoints_CollectedIds";

    readonly HashSet<string> collectedIds = new HashSet<string>();

    public int TotalPoints { get; private set; }


    public event Action<int> OnPointsChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreate()
    {
        if (Instance != null) return;
        new GameObject("GhostPointsManager").AddComponent<GhostPointsManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
        if (ResetOnEditorPlay)
        {
            PlayerPrefs.DeleteKey(TotalKey);
            PlayerPrefs.DeleteKey(CollectedKey);
        }
#endif

        TotalPoints = PlayerPrefs.GetInt(TotalKey, 0);
        string saved = PlayerPrefs.GetString(CollectedKey, "");
        if (!string.IsNullOrEmpty(saved))
            foreach (string id in saved.Split('|'))
                collectedIds.Add(id);
    }

    public bool IsCollected(string ghostId)
    {
        return collectedIds.Contains(ghostId);
    }


    public bool AddPoints(string ghostId, int points)
    {
        if (!collectedIds.Add(ghostId)) return false;

        TotalPoints = Mathf.Min(TotalPoints + points, MaxPoints);

        PlayerPrefs.SetInt(TotalKey, TotalPoints);
        PlayerPrefs.SetString(CollectedKey, string.Join("|", collectedIds));
        PlayerPrefs.Save();

        OnPointsChanged?.Invoke(TotalPoints);
        return true;
    }

    public bool AllCollected => TotalPoints >= MaxPoints;


    [ContextMenu("Reset Ghost Progress")]
    public void ResetProgress()
    {
        collectedIds.Clear();
        PlayerPrefs.DeleteKey(TotalKey);
        PlayerPrefs.DeleteKey(CollectedKey);
        PlayerPrefs.Save();

        TotalPoints = 0;
        OnPointsChanged?.Invoke(TotalPoints);
    }
}