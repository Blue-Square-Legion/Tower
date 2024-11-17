using System;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    [Serializable]
    public class AudioData
    {
        [Tooltip("The Audio File that will be played when called")]
        public AudioClip audioFile;

        [Range(0, 1)]
        [Tooltip("Percentage oh how loud the volume should play.\nExample: 0.5 will play the AudioClip at half volume")]
        public float volume = 1;

        [Range(0.1f, 3)]
        [Tooltip("Modifies the pitch to sound higher or lower.\nDefault Pitch Value: 1")]
        public float pitch = 1;

        [Tooltip("If the clip should play again immediately after the audio clip is finished")]
        public bool loop = false;

        [Range(-1, 1)]
        [Tooltip("Adjusts if the Audio should be played on the left or right speaker. Most effective when wearing headphones\nIf Audio should be played equally on both sides, value = 0")]
        public float panStereo = 0;

        [Range(0, 1)]
        [Tooltip("Sets how much the audio is affected by 3D spacial calculations (Example: Doppler Effect)." +
            "\nA value of 0 makes the sound completely 2D.\nA value of 1 makes the sound completely 3D")]
        public float spatialBlend = 0;

        [Tooltip("Determines how close the sound can feel.\nIf distance is less than the minimum distance, the sound will stop becoming louder as the listener gets closer to the source")]
        public float minDistance = 0;

        [Tooltip("Determines how far the sound can feel.\nIf distance is more than the maximym distance, the sound will stop becoming quieter as the listener gets further from the source")]
        public float maxDistance = 0;

        [Tooltip("If the audio clip should play immediately")]
        public bool playOnAwake;

        [Tooltip("If the sound will be frequently played, used for optimization purposes (will not break game if set incorrectly. It may use more resources though)")]
        public bool frequentSound;

        [Tooltip("Mutes the audio clip")]
        public bool mute;

        [Tooltip("Toggle to Show Advanced Options")]
        public bool showHiddenValues;

        //ADVANCED OPTIONS

        [Tooltip("The audio mixer used for this sound clip.\nIf empty, uses the default Audio Mixer inside the AudioManager (which can be empty too).")]
        public AudioMixerGroup audioMixer;

        [Tooltip("Bypasses and/or ignores any applied effects on the Audio Source")]
        public bool bypassEffects;

        [Tooltip("")]
        public bool bypassListenerEffects;

        [Tooltip("")]
        public bool bypassReverbZones;

        [Tooltip("")]
        public int priority = 128;

        [Tooltip("")]
        public float reverbZoneMix = 1f;

        [Tooltip("")]
        public float dopplerLevel = 1f;

        [Tooltip("")]
        public int spread;

        [Tooltip("")]
        public bool ignoeListenerVolume;

        [Tooltip("")]
        public bool ignoreListenerPause;

        [Tooltip("Defines how the audio is affected by Sound. Default: Linear" +
            "\nLogarithmic - Use this mode when you want real-world rolloff" +
            "\nLinear - Use this mode when you want to lower sound based on distance from the source" +
            "\nCustom - Use this when you want to have Custom rolloff")]
        public AudioRolloffMode audioRollOffMode = AudioRolloffMode.Linear;
    }
}