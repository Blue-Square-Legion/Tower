using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AudioSystem
{
    [RequireComponent (typeof (AudioSource))]
    public class AudioEmitter : MonoBehaviour
    {
        public AudioData data { get; private set; }
        AudioSource audioSource;
        Coroutine playingCoroutine;

        private void Awake()
        {
            audioSource = gameObject.GetOrAdd<AudioSource>();
        }

        public void Init(AudioData data)
        {
            this.data = data;
            audioSource.clip = data.audioFile;
            audioSource.volume = data.volume;
            audioSource.pitch = data.pitch;
            audioSource.panStereo = data.panStereo;
            audioSource.spatialBlend = data.spatialBlend;
            audioSource.reverbZoneMix = data.reverbZoneMix;
            audioSource.dopplerLevel = data.dopplerLevel;
            audioSource.priority = data.priority;
            audioSource.spread = data.spread;
            audioSource.loop = data.loop;
            audioSource.playOnAwake = data.playOnAwake;
            audioSource.mute = data.mute;
            audioSource.bypassEffects = data.bypassEffects;
            audioSource.bypassListenerEffects = data.bypassListenerEffects;
            audioSource.bypassReverbZones = data.bypassReverbZones;
            audioSource.ignoreListenerVolume = data.ignoeListenerVolume;
            audioSource.ignoreListenerPause = data.ignoreListenerPause;
            audioSource.minDistance = data.minDistance;
            audioSource.maxDistance = data.maxDistance;
            audioSource.outputAudioMixerGroup = data.audioMixer;
            audioSource.rolloffMode = data.audioRollOffMode;
        }
        
        public void Play()
        {
            if (playingCoroutine != null)
            {
                StopCoroutine(playingCoroutine);
            }

            audioSource.Play();
            playingCoroutine = StartCoroutine(WaitForAudioToEnd());
        }

        public void Stop()
        {
            if (playingCoroutine != null)
            {
                StopCoroutine(playingCoroutine);
                playingCoroutine = null;
            }

            audioSource.Stop();
            AudioManager.Instance.ReturnToPool(this);
        }

        public bool IsPlaying()
        {
            return playingCoroutine != null;
        }


        public void WithRandomPitch(float min = -0.05f, float max = 0.05f)
        {
            audioSource.pitch += Random.Range(min, max);
        }

        IEnumerator WaitForAudioToEnd()
        {
            yield return new WaitWhile(() => audioSource.isPlaying); //Yields infinitely until the audio source stops playing
            playingCoroutine = null;
            AudioManager.Instance.ReturnToPool(this);
        }
    }
}