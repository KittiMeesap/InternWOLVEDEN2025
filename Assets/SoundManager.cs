using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip audioClickPoints;
    public AudioClip audioClickBuildings;
    public AudioClip audioDestroyBuildings;
    public static SoundManager instance;
    void  Start()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }
    

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
