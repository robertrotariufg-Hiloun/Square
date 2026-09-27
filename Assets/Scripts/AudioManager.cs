using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private AudioSource musicSource;
    private AudioSource uiSource;
    [SerializeField] private GameObject SFXPrefab;

    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private float sfxVolume = 1f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        uiSource = gameObject.AddComponent<AudioSource>();
        uiSource.spatialBlend = 0f;
    }

    public void PlayMusic(AudioClip music, bool loop = true)
    {
        if (music == null || musicSource == null) return;
        musicSource.clip = music;
        musicSource.loop = loop;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlaySFX(AudioClip sfx, Vector3 position, float pitch = 1f)
    {
        if (sfx == null) return;

        if (SFXPrefab != null)
        {
            GameObject clone = Instantiate(SFXPrefab, position, Quaternion.identity);
            AudioSource source = clone.GetComponent<AudioSource>();
            if (source != null)
            {
                source.clip = sfx;
                source.pitch = pitch;
                source.volume = sfxVolume;
                source.Play();
            }
            Destroy(clone, sfx.length / Mathf.Max(0.1f, Mathf.Abs(pitch)));
        }
        else
        {
            GameObject temp = new GameObject("TempAudioSource");
            temp.transform.position = position;
            AudioSource source = temp.AddComponent<AudioSource>();
            source.clip = sfx;
            source.pitch = pitch;
            source.volume = sfxVolume;
            source.spatialBlend = 0f;
            source.Play();
            Destroy(temp, sfx.length / Mathf.Max(0.1f, Mathf.Abs(pitch)));
        }
    }

    public void PlayUISFX(AudioClip clip)
    {
        if (clip == null || uiSource == null) return;
        uiSource.PlayOneShot(clip, sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicSource != null)
            musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
    }
}
