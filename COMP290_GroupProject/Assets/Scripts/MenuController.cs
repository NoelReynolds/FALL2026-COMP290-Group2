using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void OnStartClick()
    {
        SceneManager.LoadScene("SelectScreen");
    }

    public void OnExitClick()
    {
        // When clicking exit, if you are in the Unity Editor, it shuts down the test, if you are in a built game, it shuts off the game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void OnContinueClick()
    {
        SceneManager.LoadScene("Title");
    }

    public void Level1Click()
    {
        SceneManager.LoadScene("Cemetery Crypt Inside v12");
    }

    public void Level2Click()
    {
        SceneManager.LoadScene("Cemetery Outside v13");
    }
}
