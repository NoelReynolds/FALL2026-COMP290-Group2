using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] public static int levelSoulsCollected = 0;
    [SerializeField] public static int levelSoulsTarget = 0;
    private int soulPrior = 0;
    private int level = 0;
    private int[] levelSoulList = { }; //Souls per level in order
    [SerializeField] private GameObject soulTextObject;
    private TextMeshProUGUI soulText = null;

    private void Awake()
    {
        if (level <= 0) levelSoulsTarget = 1;
        soulText = soulTextObject.GetComponent<TextMeshProUGUI>();
        gameManager = gameObject.GetComponent<GameManager>();
    }

    public static void AddSouls(int soulsAdded = 1)
    {
        if (soulsAdded <= 0) return;
        levelSoulsCollected += soulsAdded;
    }
    public static int GetSoulCount() { return levelSoulsCollected; }
    public static void ResetSoulCount() { levelSoulsCollected = 0; }

    private void FixedUpdate()
    {
        if (soulPrior != levelSoulsCollected)
        {
            Debug.Log("Soul Update");
            soulText.SetText($"Souls collected: {levelSoulsCollected}/{levelSoulsTarget}");
            soulPrior = levelSoulsCollected;
        }
    }
}
