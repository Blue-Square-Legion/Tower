using System.Collections.Generic;
using UnityEngine;
using UnityUtils;
using UnityEngine.Pool;

namespace AudioSystem
{
    public class AudioManager : PersistentSingleton<AudioManager>
    {
        IObjectPool<AudioEmitter> audioEmiterPool;
        readonly List<AudioEmitter> activeAudioEmitters = new();
        public readonly Queue<AudioEmitter> frequentAudioEmitters = new();

        [Tooltip("Prefab of the Sound Emitter")]
        [SerializeField] AudioEmitter audioEmitterPrefab;

        [Tooltip("")]
        [SerializeField] bool collectionCheck = true;

        [Tooltip("")]
        [SerializeField] int defaultCapacity = 10;
        [SerializeField] int maxPoolSize = 100;

        [Tooltip("Most of any particular sound that can be played at once")]
        [SerializeField] int maxAudioInstances = 30;
        
        void Start()
        {
            InitializePool();
        }

        void InitializePool()
        {
            audioEmiterPool = new ObjectPool<AudioEmitter>(CreateAudioEmitter, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, collectionCheck, defaultCapacity, maxPoolSize);
        }

        #region Constructor Methods
        AudioEmitter CreateAudioEmitter()
        {
            AudioEmitter audioEmitter = Instantiate(audioEmitterPrefab);
            audioEmitter.gameObject.SetActive(false);
            return audioEmitter;
        }

        /// <summary>
        /// Removes an audio emitter from the pool and adds it to the game
        /// </summary>
        void OnTakeFromPool(AudioEmitter audioEmitter)
        {
            audioEmitter.gameObject.SetActive(true);
            activeAudioEmitters.Add(audioEmitter);
        }

        /// <summary>
        /// Removes an audio emitter from the game and returns it back to the pool
        /// </summary>
        void OnReturnedToPool(AudioEmitter audioEmmiter)
        {
            audioEmmiter.gameObject.SetActive(false);
            activeAudioEmitters.Remove(audioEmmiter);
        }

        /// <summary>
        /// Destroys an audio emitter. Does not get returned to the pool
        /// </summary>
        void OnDestroyPoolObject(AudioEmitter audioEmitter)
        {
            Destroy(audioEmitter.gameObject);
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets an audio emitter from the pool. If none are available, attempts to create one instead
        /// </summary>
        public AudioBuilder CreateAudio() => new AudioBuilder(this);

        /// <summary>
        /// Returns an audio emitter
        /// </summary>
        public AudioEmitter Get()
        {
            return audioEmiterPool.Get();
        }

        /// <summary>
        /// Returns the audio emitter back to the pool
        /// </summary>
        public void ReturnToPool(AudioEmitter audioEmitter)
        {
            audioEmiterPool.Release(audioEmitter);
        }

        public bool CanPlayAudio(AudioData data)
        {
            //If it is not a frequent sound, allow it to run regardless
            if (!data.frequentSound) return true;

            //If it is a frequent sound, and the maximum amount of audioInstances have been reached, try to remove the oldest audio emitter
            if (frequentAudioEmitters.Count >= maxAudioInstances && frequentAudioEmitters.TryDequeue(out AudioEmitter audioEmitter))
            {
                frequentAudioEmitters.Dequeue();
                try
                {
                    //If audio emitter can be removed
                    audioEmitter.Stop();
                    return true;
                }
                catch
                {
                    //If audio emitter cannot be removed, it most likely has already been released
                    Debug.Log("AudioEmitter is already released!");
                }
                return false;
            }
            return true;
        }
        #endregion
    }
}