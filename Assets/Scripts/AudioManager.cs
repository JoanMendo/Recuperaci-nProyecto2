using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource musicAudioSource;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip[] uiClickSounds;

    [Header("Gameplay Sounds")]
    [SerializeField] private AudioClip[] ingredientMergeSounds;

    [Header("Background Music")]
    [SerializeField] private AudioClip[] menuMusic;
    [SerializeField] private AudioClip[] gameMusic;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;

    // Singleton instance
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
        PlayAmbientMusic(2);
    }

    private void InitializeAudioSources()
    {
        // Crear AudioSources si no están asignados
        if (sfxAudioSource == null)
        {
            GameObject sfxGO = new GameObject("SFX AudioSource");
            sfxGO.transform.SetParent(transform);
            sfxAudioSource = sfxGO.AddComponent<AudioSource>();
        }

        if (musicAudioSource == null)
        {
            GameObject musicGO = new GameObject("Music AudioSource");
            musicGO.transform.SetParent(transform);
            musicAudioSource = musicGO.AddComponent<AudioSource>();
        }

        // Configurar AudioSources
        sfxAudioSource.volume = sfxVolume;
        musicAudioSource.volume = musicVolume;
        musicAudioSource.loop = true;
    }

    /// <summary>
    /// Reproduce un sonido aleatorio de UI click
    /// </summary>
    public void PlayUIClick()
    {
        PlayRandomSound(uiClickSounds);
    }

    /// <summary>
    /// Reproduce un sonido aleatorio cuando se unifican ingredientes
    /// </summary>
    public void PlayIngredientMerge()
    {
        PlayRandomSound(ingredientMergeSounds);
    }

    /// <summary>
    /// Reproduce música ambiental según el número de escena
    /// </summary>
    /// <param name="sceneNumber">Número de la escena actual</param>
    public void PlayAmbientMusic(int sceneNumber)
    {
        AudioClip[] musicArray;

        // Determinar qué array usar según el número de escena
        // Escena 0 = Menú, resto = Juego
        if (sceneNumber == 0)
        {
            musicArray = menuMusic;
        }
        else
        {
            musicArray = gameMusic;
        }

        PlayRandomMusic(musicArray);
    }

    /// <summary>
    /// Reproduce música ambiental basada en la escena actual
    /// </summary>
    public void PlayAmbientMusicCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        PlayAmbientMusic(currentSceneIndex);
    }

    /// <summary>
    /// Reproduce un sonido aleatorio de un array dado
    /// </summary>
    /// <param name="soundArray">Array de AudioClips</param>
    private void PlayRandomSound(AudioClip[] soundArray)
    {
        if (soundArray == null || soundArray.Length == 0)
        {
            Debug.LogWarning("AudioManager: El array de sonidos está vacío o es null");
            return;
        }

        // Seleccionar clip aleatorio
        int randomIndex = Random.Range(0, soundArray.Length);
        AudioClip clipToPlay = soundArray[randomIndex];

        if (clipToPlay != null)
        {
            sfxAudioSource.PlayOneShot(clipToPlay);
        }
        else
        {
            Debug.LogWarning($"AudioManager: AudioClip en índice {randomIndex} es null");
        }
    }

    /// <summary>
    /// Reproduce música aleatoria de un array dado
    /// </summary>
    /// <param name="musicArray">Array de AudioClips de música</param>
    private void PlayRandomMusic(AudioClip[] musicArray)
    {
        if (musicArray == null || musicArray.Length == 0)
        {
            Debug.LogWarning("AudioManager: El array de música está vacío o es null");
            return;
        }

        // Seleccionar clip aleatorio
        int randomIndex = Random.Range(0, musicArray.Length);
        AudioClip clipToPlay = musicArray[randomIndex];

        if (clipToPlay != null)
        {
            musicAudioSource.clip = clipToPlay;
            musicAudioSource.Play();
        }
        else
        {
            Debug.LogWarning($"AudioManager: AudioClip de música en índice {randomIndex} es null");
        }
    }

    /// <summary>
    /// Detiene la música actual
    /// </summary>
    public void StopMusic()
    {
        musicAudioSource.Stop();
    }

    /// <summary>
    /// Pausa la música actual
    /// </summary>
    public void PauseMusic()
    {
        musicAudioSource.Pause();
    }

    /// <summary>
    /// Reanuda la música pausada
    /// </summary>
    public void ResumeMusic()
    {
        musicAudioSource.UnPause();
    }

    /// <summary>
    /// Cambia el volumen de los efectos de sonido
    /// </summary>
    /// <param name="volume">Volumen entre 0 y 1</param>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxAudioSource.volume = sfxVolume;
    }

    /// <summary>
    /// Cambia el volumen de la música
    /// </summary>
    /// <param name="volume">Volumen entre 0 y 1</param>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicAudioSource.volume = musicVolume;
    }
}
