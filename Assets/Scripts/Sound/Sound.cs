/*****************************************************************************
    Brackeys Audio Manager
    Tutorial video: https://youtu.be/6OT43pvUyfY

    Author: Caden Sheahan
    Date: 9/7/24
    Description: Creates the settings for the audio sources added to each sound
    within the AudioManager.
******************************************************************************/
using UnityEngine;

[System.Serializable]
public class Sound
{
    public AudioClip clip;

    public string name;

    [Range(0, 1)]
    public float volume;
    [Range(0.1f, 3)]
    public float pitch;
    public bool loop;

    [Range(-1, 1)]
    public float panStereo;
    [Range(0, 1)]
    public float spacialBlend;
    public int minDistance;
    public int maxDistance;

    [HideInInspector]
    public AudioSource source;
}