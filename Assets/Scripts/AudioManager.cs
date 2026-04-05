using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioClip[] musicClips;
    public AudioClip[] ambianceClips;
    public AudioClip[] sfxClips;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    private AudioSource audioSource;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    // functions to play clips from music and sfx arrays, beginning with error handling
    public void PlayMusic(int index)
    {
        if(index < 0 || index > musicClips.Length) return;

        musicSource.clip = musicClips[index];
        musicSource.Play();
    }

    public void PlaySFX(int index)
    {
        if (index < 0 || index > sfxClips.Length) return;

        sfxSource.clip = sfxClips[index];
        sfxSource.PlayOneShot(sfxSource.clip);
    }

    public void PlayRandomSFX(int min, int max)
    {
        int index = Random.Range(min, max);

        if (min < 0 || max > sfxClips.Length) return;

        sfxSource.clip = sfxClips[index];
        sfxSource.PlayOneShot(sfxSource.clip);
    }

    // check if scene is loaded/unloaded, then play appropriate clip
    private void OnEnable()
    {
        SceneManager.sceneLoaded += playSceneMusic;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= playSceneMusic;
    }
    private void playSceneMusic(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "Start")
        {
            PlayMusic(4);
        }
        if(scene.name == "WordEntry")
        {
            PlayMusic(1);
        }
        if(scene.name == "WordPlacement")
        {
            PlayMusic(0);
        }
        if (scene.name == "StoryReveal")
        {
            PlayMusic(2);
        }
    }

    // functions to play audio | used in AudioButtonController.cs
    public void PlayButtonHoverSound()
    {
        PlaySFX(0);
    }

    public void PlayButtonClickSound()
    {
        PlaySFX(1);
    }

    public void PlayRandomButtonClickSound()
    {
        PlayRandomSFX(2, 4);
    }
}
