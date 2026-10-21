using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGoalController : MonoBehaviour
{
    public Simple_Test_Player player;
    //public Player player; // Use the player script to ensure player is possessing a body
    [SerializeField] private GameObject getBodyUI;
    [SerializeField] private GameObject winUI;
    private float winTime = 10f;

    private void Start()
    {
        getBodyUI.SetActive(false);
        winUI.SetActive(false);
    }

    // Scene Management on trigger with end goal
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
            string sceneName = scene.name; // Checking which level is open

            if (player.isPossessed == true)
            {
                // Edit these once scene names are agreed upon for each level
                if (sceneName == "Cemetary")
                {
                    winUI.SetActive(true); // Display the win screen before loading next level
                    StartCoroutine(WinScreen(winTime));
                    SceneManager.LoadScene("Swamp");
                }
                if (sceneName == "Swamp")
                {
                    winUI.SetActive(true);
                    StartCoroutine(WinScreen(winTime));
                    SceneManager.LoadScene("Church");
                }
                if (sceneName == "Church")
                {
                    winUI.SetActive(true);
                    StartCoroutine(WinScreen(winTime));
                    SceneManager.LoadScene("EndScreen");
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

    private IEnumerator WinScreen(float time)
    {
        yield return new WaitForSeconds(time);

        winUI.SetActive(false);
    }
}
