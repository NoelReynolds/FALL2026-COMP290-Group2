using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGoalController : MonoBehaviour
{
    //public Player player;

    // Scene Management on trigger with end goal
    // NOTE: Add check for physical form
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
            string sceneName = scene.name; // Checking which level is open

            // Edit these once scene names are agreed upon for each level
            if(sceneName == "Cemetary")
            {
                SceneManager.LoadScene("Swamp");
            }
            if(sceneName == "Swamp")
            {
                SceneManager.LoadScene("Church");
            }
            if(sceneName == "Church")
            {
                SceneManager.LoadScene("EndScreen");
            }
        }
    }
}
