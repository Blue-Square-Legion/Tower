/*****************************************************************************
    Brackeys Audio Manager
    Tutorial video: https://youtu.be/6OT43pvUyfY

    Author: Caden Sheahan
    Date: 9/7/24
    Description: Creates an array of all sound effects defined by "Sound" script
    and adds an audio source to each of them in the AudioManager game object.
    
    Use this line to play any sound anywhere, if you have the name set in 
    the inspector:
    
    FindObjectOfType<AudioManager>().Play("[INSERT_NAME_FROM_INSPECTOR]");
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

    public Sound[] Sounds; 
    public AudioMixerGroup masterMixer;
    public float musicVolume;
    
    void Awake()
    {   
        foreach (Sound s in Sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = masterMixer;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.panStereo = s.panStereo;
            s.source.spatialBlend = s.spacialBlend;
            s.source.minDistance = s.minDistance;
            s.source.maxDistance = s.maxDistance;
            s.source.rolloffMode = AudioRolloffMode.Linear;
        }
    }

    #region Sound Controls

    public AudioSource SoundSourceOnObject(string name, GameObject obj)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": source not found");
            return null;
        }
        s.source = obj.GetComponent<AudioSource>();
        return s.source;
    }

    public void RemoveSound(string name, GameObject obj)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": source not found");
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
        s.source.Play();
        s.source.volume = vol;
        //print(s.name + " is playing at volume level " + s.source.volume);
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
        //print(s.name + " is playing at volume level " + s.source.volume);
    }

    public void AddSound(string soundName, GameObject obj)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning(soundName + ": audio not found");
            return;
        }

        s.source = obj.AddComponent<AudioSource>();
        s.source.clip = s.clip;
        s.source.outputAudioMixerGroup = masterMixer;
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.loop = s.loop;
        s.source.panStereo = s.panStereo;
        s.source.spatialBlend = s.spacialBlend;
        s.source.minDistance = s.minDistance;
        s.source.maxDistance = s.maxDistance;
        s.source.rolloffMode = AudioRolloffMode.Linear;

        //print("sound '" + soundName + "' added to " + obj.name);
    }

    public void PlayAddedSound(string soundName, GameObject obj)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == soundName);
        if (s == null || obj == null)
        {
            Debug.LogWarning(soundName + ": audio not found");
            return;
        }
        s.source = obj.GetComponent<AudioSource>();
        //if (!s.source.isPlaying)
        //{
            s.source.Play();
        //}
    }

    public void StopAddedSound(string soundName, GameObject obj)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == soundName);
        if (s == null || obj == null)
        {
            Debug.LogWarning(soundName + ": audio not found");
            return;
        }
        s.source = obj.GetComponent<AudioSource>();
       
        
        s.source.Stop();
        
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


    public float ClipLength(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return 0;
        }
        //print(s.source.clip.length);
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
        s.source.volume = musicVolume;
    }

    #endregion

    #region Song Functions



    #endregion
}