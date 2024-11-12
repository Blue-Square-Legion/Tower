/*****************************************************************************
    Author: Ryan Herwig
    Date: 9/7/24
    Description: Makes easy access for creating sounds in Unity. Keeps them all in one area and easily modifiable.
    
    To Play Sound from any Script:
    AudioManager.Instance.Play([SOUND_NAME]);

    Note: [SOUND_NAME] is the name given to the Sound Object in the AudioManager inside the inspector
 *****************************************************************************/
using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(AudioManager)) as AudioManager;
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    #endregion
    [Tooltip("Can leave empty. If the sound clip does not have an Audio Mixer, it uses this one instead.")]
    public AudioMixerGroup defaultMasterMixer;

    [Tooltip("The array of all the sounds in the game")]
    public Sound[] Sounds;
    
    void Awake()
    {   
        //Gets all the sounds inside the array and creates an AudioSource out of them
        foreach (Sound s in Sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.panStereo = s.panStereo;
            s.source.spatialBlend = s.spacialBlend;
            s.source.minDistance = s.minDistance;
            s.source.maxDistance = s.maxDistance;
            s.source.rolloffMode = s.audioRollOffMode;

            s.source.outputAudioMixerGroup = s.audioMixer;
            if (s.source.outputAudioMixerGroup == null)
                s.source.outputAudioMixerGroup = defaultMasterMixer;
        }
    }

    #region Sound Controls

    /// <summary>
    /// Adds an Audio Listener with a sound file to a GameObject
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="obj"></param>
    public void AddAudioOnObject(string soundName, GameObject obj)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning(soundName + ": audio not found");
            return;
        }
        if (obj == null)
        {
            Debug.LogWarning(obj + " not found");
            return;
        }

        s.source = obj.AddComponent<AudioSource>();
        s.source.clip = s.clip;
        s.source.outputAudioMixerGroup = defaultMasterMixer;
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.loop = s.loop;
        s.source.panStereo = s.panStereo;
        s.source.spatialBlend = s.spacialBlend;
        s.source.minDistance = s.minDistance;
        s.source.maxDistance = s.maxDistance;
        s.source.rolloffMode = AudioRolloffMode.Linear;
    }

    public void PlayAudioOnObject(string soundName, GameObject obj)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning(soundName + ": audio not found");
            return;
        }
        if (obj == null)
        {
            Debug.LogWarning(obj + " not found");
            return;
        }

        s.source = obj.GetComponent<AudioSource>();
        if (!s.source.isPlaying)
        {
            s.source.Play();
        }
    }

    public void StopAddedSound(string soundName, GameObject obj)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning(soundName + ": audio not found");
            return;
        }
        if (obj == null)
        {
            Debug.LogWarning(obj + " not found");
            return;
        }
        s.source = obj.GetComponent<AudioSource>();


        s.source.Stop();

    }

    public AudioSource GetAudioOnObject(string name, GameObject obj)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": source not found");
            return null;
        }
        if (obj == null)

        {
            Debug.LogWarning(obj + " not found");
            return null;
        }

        s.source = obj.GetComponent<AudioSource>();
        return s.source;
    }

    public void RemoveAudioOnObject(string name, GameObject obj)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": source not found");
            return;
        }
        if (obj == null)
        {
            Debug.LogWarning(obj + " not found");
            return;
        }

        s.source = obj.GetComponent<AudioSource>();
        Destroy(s.source);
        //print("Removed" + name + "sound from " + obj.name);
    }

    public void Play(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        print(s.name);
        print(s.volume);
        print(s.source);
        s.source.Play();
    }

    public void PlayAtVolume(string name, float vol)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.volume = vol;
        s.source.Play();
    }

    public void UpdateVolume(string name, float vol)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.volume = vol;
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.Stop();
    }

    public void Pause(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.Pause();
    }

    public void UnPause(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.UnPause();
    }


    public float GetAudioLength(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return 0;
        }
        return s.source.clip.length;
    }

    /// <summary>
    /// Specifically for music, this function disables the volume of a clip 
    /// as soon as it plays
    /// </summary>
    /// <param name="name"></param>
    public void PlayMuted(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.Play();
        s.source.volume = 0.0f;
    }

        /// <summary>
        /// Specifically for music, this function disables the volume of a clip
        /// </summary>
        /// <param name="name"></param>
        public void Mute(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.volume = 0.0f;
    }

    /// <summary>
    /// Specifically for music, this function enables the volume of a clip
    /// </summary>
    /// <param name="name"></param>
    public void Unmute(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.volume = s.volume;
    }
    #endregion
}