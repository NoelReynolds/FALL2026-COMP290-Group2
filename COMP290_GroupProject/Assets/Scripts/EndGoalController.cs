using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGoalController : MonoBehaviour
{
    //[SerializeField] public Player player; // Use the player script to ensure player is possessing a body

    public SkeletonController2D player; //We'll use this one for now
    [SerializeField] private GameObject getBodyUI;
    [SerializeField] private GameObject winUI;
    [SerializeField] private TMPro.TMP_Text levelTimerText;
    [SerializeField] private TMPro.TMP_Text bestTimeText;
    private float bestTime;
    private float timeLeft;
    private int newTime;
    private float winTime = 7.0f;
    private bool levelBeat = false;
    private string openScene;

    [SerializeField] private GameObject loseUI;

    private void Start()
    {
        getBodyUI.SetActive(false);
        winUI.SetActive(false);
        loseUI.SetActive(false);

        UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
        string openScene = scene.name; // Checking which level is open

        if(openScene == "Gothic Town v13" || openScene == "Cemetery Church Inside v13")
        {
            timeLeft = 500f; // Give more time to later levels
        }
        else
        {
            timeLeft = 300f;
        }
    }



    // Scene Management on trigger with end goal
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            if (player.isPossessed == true)
            {
                levelBeat = true;

                // Get the best time from memory
                bestTime = PlayerPrefs.GetFloat($"{openScene}_Score", 0.0f);

                winUI.SetActive(true); // Display the win screen before loading next level
                if(timeLeft > bestTime) // Check for a new score
                {
                    bestTimeText.text = $"NEW HIGH SCORE\n{timeLeft.ToString()}";
                    // Save the new time to memory
                    PlayerPrefs.SetFloat($"{openScene}_Score", timeLeft);
                }
                else
                {
                    bestTimeText.text = timeLeft.ToString();
                }

                // Edit these once scene names are agreed upon for each level
                if (openScene == "Cemetery Crypt Inside v13")
                {
                    StartCoroutine(WinScreen(winTime, "Cemetery Outside v14"));
                }
                if (openScene == "Cemetery Outside v14")
                {
                    StartCoroutine(WinScreen(winTime, "Gothic Town v13"));
                }
                if (openScene == "Gothic Town v13")
                {
                    StartCoroutine(WinScreen(winTime, "Cemetery Church Inside v13"));
                }
                if(openScene == "Cemetery Church Inside v13")
                {
                    StartCoroutine(WinScreen(winTime, "EndScreen"));
                }
            }
            else if(player.isPossessed == false)
            {
                getBodyUI.SetActive(true); // Tell Player to get a body.
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            if(getBodyUI.activeInHierarchy)
            {
                getBodyUI.SetActive(false);
            }
        }
    }

    private IEnumerator WinScreen(float time, string sceneName)
    {
        yield return new WaitForSeconds(time);

        winUI.SetActive(false);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator LoseScreen(float time, string sceneName)
    {
        yield return new WaitForSeconds(time);

        loseUI.SetActive(false);
        SceneManager.LoadScene(sceneName);
    }

    private void Update()
    {
        if(levelBeat == false)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                loseUI.SetActive(true);
                StartCoroutine(LoseScreen(winTime, "GameOver"));
            }
            newTime = (int)timeLeft;
            levelTimerText.text = newTime.ToString();
        }
    }
}
