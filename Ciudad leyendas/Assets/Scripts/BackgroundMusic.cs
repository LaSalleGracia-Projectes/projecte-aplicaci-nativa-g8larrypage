using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioClip backgroundMusicClip;
    private AudioSource audioSource;

    void Awake()
    {
        // Aseg�rate de que no haya duplicados si cambias de escena
        if (FindObjectsOfType<BackgroundMusic>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // Mantener m�sica entre escenas

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusicClip;
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = 0.5f; // Puedes ajustar el volumen aqu�
        audioSource.Play();
    }
}
