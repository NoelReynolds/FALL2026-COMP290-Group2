using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public AudioSource audioSource;
    private float bgmVolume = 1f;
    public Slider bgmSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bgmVolume = PlayerPrefs.GetFloat("music");
        audioSource.volume = bgmVolume;
        bgmSlider.value = bgmVolume;
    }

    // Update is called once per frame
    void Update()
    {
        audioSource.volume = bgmVolume;
        PlayerPrefs.SetFloat("music", bgmVolume);
    }

    public void updateVolume(float volume)
    {
        bgmVolume = volume;
    }
}
