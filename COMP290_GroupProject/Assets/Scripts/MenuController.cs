using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject optionsScreen;

    private void Start()
    {
        optionsScreen.SetActive(false);
    }

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

    public void OnOpenOptions()
    {
        optionsScreen.SetActive(true);
    }

    public void OnCloseOptions()
    {
        optionsScreen.SetActive(false);
    }

    public void Level1Click()
    {
        SceneManager.LoadScene("Cemetery Crypt Inside v13");
    }

    public void Level2Click()
    {
        SceneManager.LoadScene("Cemetery Outside v14");
    }

    public void Level3Click()
    {
        SceneManager.LoadScene("Gothic Town v13");
    }

    public void Level4Click()
    {
        SceneManager.LoadScene("Cemetery Church Inside v13");
    }
}
