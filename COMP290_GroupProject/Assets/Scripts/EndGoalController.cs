using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGoalController : MonoBehaviour
{
    public Player player; // Use the player script to ensure player is possessing a body
    [SerializeField] private GameObject getBodyUI;

    private void Start()
    {
        getBodyUI.SetActive(false);
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
                    SceneManager.LoadScene("Swamp");
                }
                if (sceneName == "Swamp")
                {
                    SceneManager.LoadScene("Church");
                }
                if (sceneName == "Church")
                {
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
}
