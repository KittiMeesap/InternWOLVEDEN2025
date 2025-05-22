using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
    public AudioSource menuSource;
    public AudioSource gameSource;

    [Header("Menu Clips")]
    public AudioClip menuClick;
    public AudioClip menuHover;
    public AudioClip menuBGM;

    [Header("Gameplay Clips")]
    public AudioClip buildingClick;
    public AudioClip tileClick;
    public AudioClip destroyBuilding;
    public AudioClip gameBGM;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            PlayMenuBGM();
        }
        else if (scene.name == "Gameplay")
        {
            PlayGameBGM();
        }
    }

    // === Menu ===
    public void PlayMenuHover() => menuSource?.PlayOneShot(menuHover);
    public void PlayMenuClick() => menuSource?.PlayOneShot(menuClick);

    public void PlayMenuBGM()
    {
        if (menuSource != null && menuSource.clip != menuBGM)
        {
            menuSource.Stop();
            menuSource.clip = menuBGM;
            menuSource.loop = true;
            menuSource.Play();
        }

        if (gameSource != null && gameSource.isPlaying)
        {
            gameSource.Stop();
        }
    }

    // === Gameplay ===
    public void PlayBuildingClick() => gameSource?.PlayOneShot(buildingClick);
    public void PlayTileClick() => gameSource?.PlayOneShot(tileClick);
    public void PlayDestroyBuilding() => gameSource?.PlayOneShot(destroyBuilding);

    public void PlayGameBGM()
    {
        if (gameSource != null && gameSource.clip != gameBGM)
        {
            gameSource.Stop();
            gameSource.clip = gameBGM;
            gameSource.loop = true;
            gameSource.Play();
        }

        if (menuSource != null && menuSource.isPlaying)
        {
            menuSource.Stop();
        }
    }
}
