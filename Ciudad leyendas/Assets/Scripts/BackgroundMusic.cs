using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance;

    public AudioClip backgroundMusicClip;
    private AudioSource audioSource;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Audio setup
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusicClip;
        audioSource.loop = true;
        audioSource.playOnAwake = true;

        // Cargar volumen guardado o usar volumen por defecto
        float savedVolume = PlayerPrefs.GetFloat("music_volume", 0.5f);
        audioSource.volume = savedVolume;

        audioSource.Play();
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
            PlayerPrefs.SetFloat("music_volume", volume);
            PlayerPrefs.Save();
        }
    }


    public float GetVolume()
    {
        return audioSource != null ? audioSource.volume : 0f;
    }
}
