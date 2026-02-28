using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource soundSource;
    [SerializeField] private float targetMusicVolume = 0.5f;

    private Coroutine musicSwitchCoroutine;

    public void PlayAudio(AudioClip music, AudioClip sound)
    {
        if (sound != null)
        {
            soundSource.clip = sound;
            soundSource.Play();
        }

        if (music != null && (musicSource.clip != music || !musicSource.isPlaying))
        {
            if (musicSwitchCoroutine != null)
            {
                StopCoroutine(musicSwitchCoroutine);
            }

            if (musicSource.isPlaying && musicSource.clip != music)
            {
                musicSource.Stop();
            }

            musicSwitchCoroutine = StartCoroutine(SwitchMusic(music));
        }
    }

    public bool IsSoundPlaying()
    {
        return soundSource != null && soundSource.isPlaying;
    }

    private IEnumerator SwitchMusic(AudioClip music)
    {
        musicSource.volume = 0f;

        musicSource.clip = music;
        musicSource.Play();

        while (musicSource.volume < targetMusicVolume)
        {
            musicSource.volume = Mathf.Min(targetMusicVolume, musicSource.volume + 0.05f);
            yield return new WaitForSeconds(0.05f);
        }

        musicSwitchCoroutine = null;
    }
}
