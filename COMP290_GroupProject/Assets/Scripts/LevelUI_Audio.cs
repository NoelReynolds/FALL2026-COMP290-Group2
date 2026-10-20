using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUI_Audio : MonoBehaviour
{
    [SerializeField] private GameObject levelUi; // We can select the correct UI from the editor
    [SerializeField] private GameObject tipsText; // This will replace the levelUI when disabled
    private float uiUpTime = 10f;

    //// Audio Setup
    //[SerializeField] private AudioClip titleBGM;
    //[SerializeField] private AudioClip level1BGM;
    //[SerializeField] private AudioClip level2BGM;
    //[SerializeField] private AudioClip level3BGM;
    //[SerializeField] private AudioClip endingBGM;
    //[SerializeField] private AudioClip gameOverBGM;

    //// Audio Sources
    //[SerializeField] private AudioSource bgmSource;

    // Level Soundtrack
    void Start()
    {
        levelUi.SetActive(false);
        tipsText.SetActive(false);

        Scene scene = SceneManager.GetActiveScene();
        string sceneName = scene.name;
        LoadLevel(sceneName);


        // Set up UI objective UI to turn off after some time
        //StartCoroutine(DisableUI(uiUpTime));
        // I moved this to each level scene instead. Since the other scenes wouldn't need the level/tips UI
        // For those ones, just put an empty object in the editor if needed
    }

    private IEnumerator DisableUI(float time)
    {
        yield return new WaitForSeconds(time);

        levelUi.SetActive(false);
        tipsText.SetActive(true);
    }

    void Update()
    {
        if(tipsText.activeInHierarchy)
        {
            if(Input.GetKeyDown(KeyCode.T)) // Remove the Tip and view the objective again
            {
                tipsText.SetActive(false);
                levelUi.SetActive(true);
                StartCoroutine(DisableUI(uiUpTime));
            }
        }
    }

    private void LoadLevel(string sceneName)
    {
        // This will load the Audio based on which scene is open
        if (sceneName == "Title" || sceneName == "Opening")
        {
            //bgmSource.clip = titleBGM;
            //bgmSource.Play();
        }
        else if (sceneName == "Cemetary")
        {
            //bgmSource.clip = level1BGM;
            //bgmSource.Play();
            levelUi.SetActive(true);
            StartCoroutine(DisableUI(uiUpTime));
        }
        else if (sceneName == "Swamp")
        {
            //bgmSource.clip = level2BGM;
            //bgmSource.Play();
            levelUi.SetActive(true);
            StartCoroutine(DisableUI(uiUpTime));
        }
        else if (sceneName == "Church")
        {
            //bgmSource.clip = level3BGM;
            //bgmSource.Play();
            levelUi.SetActive(true);
            StartCoroutine(DisableUI(uiUpTime));
        }
        else if (sceneName == "EndScreen")
        {
            //bgmSource.clip = endingBGM;
            //bgmSource.Play();
        }
        else if (sceneName == "GameOver")
        {
            //bgmSource.clip = gameOverBGM;
            //bgmSource.Play();
        }
        else if(sceneName == "UI_Test_Scene")
        {
            // For testing purposes
            levelUi.SetActive(true);
            StartCoroutine(DisableUI(uiUpTime));
        }

    }
}
