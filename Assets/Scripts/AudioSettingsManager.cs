using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsManager : MonoBehaviour
{
    [Header("UI Reference")]
    public Slider volumeSlider;

    [Header("Audio Target")]
    private AudioSource[] sourcesToControl;

    private const string VolumePrefKey = "MasterVolume";

    void Start()
    {
        if (SoundManager.instance != null)
        {
            sourcesToControl = new AudioSource[]
            {
                SoundManager.instance.gameSource,
                SoundManager.instance.menuSource
            };
        }

        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        volumeSlider.value = savedVolume;
        ApplyVolume(savedVolume);

        volumeSlider.onValueChanged.AddListener(delegate {
            ApplyVolume(volumeSlider.value);
        });
    }

    void ApplyVolume(float volume)
    {
        if (sourcesToControl != null)
        {
            foreach (var source in sourcesToControl)
            {
                if (source != null)
                    source.volume = volume;
            }
        }

        PlayerPrefs.SetFloat(VolumePrefKey, volume);
    }
}
