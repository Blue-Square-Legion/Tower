/*****************************************************************************
    Author: Ryan Herwig
    Date: 9/7/24
    Description: Makes easy access for creating sounds in Unity. Keeps them all in one area and easily modifiable.
    
    To Play Sound from any Script:
    AudioManager.Instance.Play([SOUND_NAME], ~OPTIONAL~ [GAME_OBJECT]).
    If Game Object is given, it will play the sound that is on the game object.
    If it is not given, it will play the sound that is attached to the AudioManager itself (Spatial Blend 2D (Set it to 0) is heavily recommended)

    Note: [SOUND_NAME] is the name given to the Sound Object in the AudioManager inside the inspector
 *****************************************************************************/
using System;
using System.Collections.Generic;
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
    public Audio[] audioFiles;

    private List<(GameObject, Audio)> inGameAudio;
    
    void Awake()
    {   
        //Gets all the sounds inside the array and creates an AudioSource out of them
        foreach (Audio s in audioFiles)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.audioFile;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.panStereo = s.panStereo;
            s.source.spatialBlend = s.spatialBlend;
            s.source.minDistance = s.minDistance;
            s.source.maxDistance = s.maxDistance;
            s.source.rolloffMode = s.audioRollOffMode;

            s.source.outputAudioMixerGroup = s.audioMixer;
            if (s.source.outputAudioMixerGroup == null)
                s.source.outputAudioMixerGroup = defaultMasterMixer;
        }
        inGameAudio = new();
    }

    #region Sound Controls
    #region Audio on GameObjects
    /// <summary>
    /// Adds an Audio Listener with a sound file to a GameObject
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="obj"></param>
    public void Add(string soundName, GameObject obj)
    {
        Audio tempAudio = Array.Find(audioFiles, sound => sound.name == soundName);
        if (tempAudio == null)
        {
            Debug.LogWarning(soundName + ": audio not found");
            return;
        }
        if (obj == null)
        {
            Debug.LogWarning(obj + " not found");
            return;
        }

        tempAudio.source = obj.AddComponent<AudioSource>();
        tempAudio.source.clip = tempAudio.audioFile;
        tempAudio.source.outputAudioMixerGroup = defaultMasterMixer;
        tempAudio.source.volume = tempAudio.volume;
        tempAudio.source.pitch = tempAudio.pitch;
        tempAudio.source.loop = tempAudio.loop;
        tempAudio.source.panStereo = tempAudio.panStereo;
        tempAudio.source.spatialBlend = tempAudio.spatialBlend;
        tempAudio.source.minDistance = tempAudio.minDistance;
        tempAudio.source.maxDistance = tempAudio.maxDistance;
        tempAudio.source.rolloffMode = tempAudio.audioRollOffMode;

        inGameAudio.Add((obj, tempAudio));
    }

    public void Play(string soundName, GameObject obj)
    {
        Audio tempAudio = Array.Find(audioFiles, sound => sound.name == soundName);

        //Error Check - If audio file was created with this script
        if (tempAudio == null)
        {
            Debug.LogWarning(soundName + ": audio not found");
            return;
        }

        //Error Check - if GameObject exists
        if (obj == null)
        {
            Debug.LogWarning(obj + " not found");
            return;
        }
        /**
        //Searches List for the Audio inside the GameObject
        int audioCount = inGameAudio.Count;
        for (int i = 0; i < audioCount; i++)
        {
            if (obj == inGameAudio[i].Item1 && soundName.Equals(inGameAudio[i].Item2.name))
            {
                //Found source
            }
        }
        */

        //Failed to find source
        Component[] audioSourceComponents = obj.GetComponents(typeof(AudioSource));
        List<AudioSource> test = new();
        for (int i = 0; i < audioSourceComponents.Length; i++)
        {
            test.Add((AudioSource)audioSourceComponents[i]);
        }
        for (int i = 0; i < test.Count; i++)
        {
            print(test[i].clip);
        }

        tempAudio.source = obj.GetComponent<AudioSource>();
        tempAudio.source.Play();
    }

    public void PlayAtVolume(string name, float vol)
    {
        Audio s = Array.Find(audioFiles, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.volume = vol;
        s.source.Play();
    }

    public void Pause(string name, GameObject obj) //TODO
    {
        Audio s = Array.Find(audioFiles, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.Pause();
    }

    public void UnPause(string name, GameObject obj) //TODO
    {
        Audio s = Array.Find(audioFiles, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.UnPause();
    }

    public void Stop(string soundName, GameObject obj)
    {
        Audio s = Array.Find(audioFiles, sound => sound.name == soundName);
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

    public void Remove(string name, GameObject obj)
    {
        Audio s = Array.Find(audioFiles, sound => sound.name == name);
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
    }

    public AudioSource Get(string name, GameObject obj)
    {
        Audio s = Array.Find(audioFiles, sound => sound.name == name);
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

    public void UpdateVolume(string name, float vol, GameObject gameObject) //TODO
    {
        Audio s = Array.Find(audioFiles, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.volume = vol;
    }

    public float GetLength(string name)
    {
        Audio s = Array.Find(audioFiles, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return 0;
        }
        return s.source.clip.length;
    }
    #endregion

    #region AudioManager Sounds
    /// <summary>
    /// Mutes the audio on the AudioManager
    /// Recommended for the Spatial Blend to be at 0 (2D)
    /// </summary>
    /// <param name="name"></param>
    public void Mute(string name)
    {
        Audio s = Array.Find(audioFiles, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.volume = 0.0f;
    }

    /// <summary>
    /// Unmutes the audio on the AudioManager
    /// Recommended for the Spatial Blend to be at 0 (2D)
    /// </summary>
    /// <param name="name"></param>
    public void Unmute(string name)
    {
        Audio s = Array.Find(audioFiles, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning(name + ": audio not found");
            return;
        }
        s.source.volume = s.volume;
    }

    /// <summary>
    /// Starts playing the Audio that is on the AudioManager
    /// Recommended for the Spatial Blend to be at 0 (2D)
    /// </summary>
    /// <param name="soundName"></param>
    public void Play(string soundName)
    {
        Audio tempAudio = Array.Find(audioFiles, sound => sound.name == soundName);
        if (tempAudio == null)
        {
            Debug.LogWarning(soundName + ": audio not found");
            return;
        }

        tempAudio.source.Play();
    }

    /// <summary>
    /// Stops playing the Audio that is not on the AudioManager
    /// Recommended for the Spatial Blend to be at 0 (2D)
    /// </summary>
    /// <param name="soundName"></param>
    public void Stop(string soundName)
    {
        Audio s = Array.Find(audioFiles, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning(soundName + ": audio not found");
            return;
        }

        s.source.Stop();
    }

    #endregion
    #endregion
}