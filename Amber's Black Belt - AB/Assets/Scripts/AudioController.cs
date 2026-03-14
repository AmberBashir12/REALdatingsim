using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource musicSource2;
    public AudioSource soundSource;
    public AudioSource blipSource;
    [SerializeField] private float targetMusicVolume = 0.5f;

    private Coroutine musicSwitchCoroutine;
    private Coroutine music2SwitchCoroutine;

    public void PlayAudio(AudioClip music, AudioClip sound)
    {
        PlayAudio(music, null, sound);
    }

    public void PlayAudio(AudioClip music, AudioClip music2, AudioClip sound)
    {
        if (sound != null)
        {
            soundSource.clip = sound;
            soundSource.Play();
        }

        if (music != null)
        {
            PlayMusicOnSource(musicSource, false, music);
        }

        if (music2 != null)
        {
            if (musicSource2 == null)
            {
                Debug.LogWarning($"{nameof(AudioController)} is missing {nameof(musicSource2)} but was asked to play a second music clip '{music2.name}'.");
            }
            else
            {
                PlayMusicOnSource(musicSource2, true, music2);
            }
        }
        else if (music != null)
        {
            StopSecondaryMusic();
        }
    }

    public bool IsSoundPlaying()
    {
        return soundSource != null && soundSource.isPlaying;
    }

    public void PlayBlip(AudioClip blip)
    {
        if (blip == null)
        {
            return;
        }

        AudioSource source = blipSource != null ? blipSource : soundSource;
        if (source == null)
        {
            return;
        }

        source.PlayOneShot(blip);
    }

    private void StopSecondaryMusic()
    {
        if (musicSource2 == null)
        {
            return;
        }

        if (music2SwitchCoroutine != null)
        {
            StopCoroutine(music2SwitchCoroutine);
            music2SwitchCoroutine = null;
        }

        if (musicSource2.isPlaying)
        {
            musicSource2.Stop();
        }

        musicSource2.clip = null;
        musicSource2.volume = targetMusicVolume;
    }

    private void PlayMusicOnSource(AudioSource source, bool isSecondary, AudioClip music)
    {
        if (source == null)
        {
            return;
        }

        if (music == null)
        {
            return;
        }

        if (source.clip == music && source.isPlaying)
        {
            return;
        }

        Coroutine switchCoroutine = isSecondary ? music2SwitchCoroutine : musicSwitchCoroutine;
        if (switchCoroutine != null)
        {
            StopCoroutine(switchCoroutine);
        }

        if (source.isPlaying && source.clip != music)
        {
            source.Stop();
        }

        switchCoroutine = StartCoroutine(SwitchMusicOnSource(source, music, isSecondary));
        if (isSecondary)
        {
            music2SwitchCoroutine = switchCoroutine;
        }
        else
        {
            musicSwitchCoroutine = switchCoroutine;
        }
    }

    private IEnumerator SwitchMusicOnSource(AudioSource source, AudioClip music, bool isSecondary)
    {
        source.volume = 0f;

        source.clip = music;
        source.Play();

        while (source.volume < targetMusicVolume)
        {
            source.volume = Mathf.Min(targetMusicVolume, source.volume + 0.05f);
            yield return new WaitForSeconds(0.05f);
        }

        if (isSecondary)
        {
            music2SwitchCoroutine = null;
        }
        else
        {
            musicSwitchCoroutine = null;
        }
    }
}
