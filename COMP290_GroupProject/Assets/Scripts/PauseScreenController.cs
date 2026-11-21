using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScreenController : MonoBehaviour
{
    [SerializeField] GameObject pauseScreenUI;

    private void Start()
    {
        pauseScreenUI.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(pauseScreenUI.activeInHierarchy)
            {
                Time.timeScale = 1.0f;
                pauseScreenUI.SetActive(false);
            }
            else if(!pauseScreenUI.activeInHierarchy)
            {
                Time.timeScale = 0.0f;
                pauseScreenUI.SetActive(true);
            }
        }
    }

    public void OnQuitToTitle()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("Title");
    }    
}
