using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
    public AudioSource audioSource; 

    [Header("Menu Clips")]
    public AudioClip  soundMenuClick;
    public AudioClip  soundMenuHover;
    public AudioClip  soundMenuBGM;

    [Header("Gameplay Clips")]
    public AudioClip soundBuildingClick; 
    public AudioClip soundTileClickPoint;
    public AudioClip soundDestroyBuilding;
    public AudioClip  soundGameBGM;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); 
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("SoundManager requires an AudioSource component on the same GameObject!");
            }
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("SoundManager: Tried to play a null clip or audioSource is null.");
        }
    }
    
    private void PlayBGM(AudioClip newBGM)
    {
        if (audioSource == null) return;

        // ถ้า BGM ปัจจุบันไม่ใช่ BGM ใหม่ หรือไม่ได้กำลังเล่นอยู่
        if (audioSource.clip != newBGM || !audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = newBGM;
            audioSource.loop = true; // BGM ควรอ่านซ้ำ
            audioSource.Play();
        }
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
        // หากมี Scene อื่นๆ สามารถเพิ่ม else if ได้
    }

    // === Menu BGM ===
    public void PlayMenuBGM()
    {
        PlayBGM(soundMenuBGM);
    }
    
    public void PlayGameBGM()
    {
        PlayBGM(soundGameBGM);
    }
}