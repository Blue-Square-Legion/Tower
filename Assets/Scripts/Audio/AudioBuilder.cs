using UnityEngine;
namespace AudioSystem
{
    public class AudioBuilder
    {
        readonly AudioManager audioManager;
        AudioData audioData;
        Vector3 position = Vector3.zero;
        bool randomPitch;
        AudioEmitter audioEmitter;

        public AudioBuilder(AudioManager audioManager)
        {
            this.audioManager = audioManager;
        }

        public AudioBuilder WithAudioData(AudioData audioData)
        {
            this.audioData = audioData;
            return this;
        }

        public AudioBuilder WithPosition(Vector3 position)
        {
            this.position = position;
            return this;
        }

        public AudioBuilder WithRandomPitch(bool hasRandomPitch)
        {
            randomPitch = hasRandomPitch;
            return this;
        }

        public AudioEmitter Play()
        {
            if (!audioManager.CanPlayAudio(audioData)) return null;

            audioEmitter = audioManager.Get();
            audioEmitter.Init(audioData);
            audioEmitter.transform.position = position;
            audioEmitter.transform.parent = AudioManager.Instance.transform;

            if (randomPitch)
            {
                audioEmitter.WithRandomPitch();
            }

            if (audioData.frequentSound)
                audioManager.frequentAudioEmitters.Enqueue(audioEmitter);
            audioEmitter.Play();

            return audioEmitter;
        }
    }
}