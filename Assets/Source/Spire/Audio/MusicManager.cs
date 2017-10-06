using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages a set of songs and their playback.
    /// </summary>
    [Obsolete("This class is temporary!")]
    public class MusicManager : InjectableMonoBehaviour
    {
        [Serializable]
        public class Clip
        {
            /// <summary>
            /// Audio Source
            /// </summary>
            public AudioSource AudioSource;

            /// <summary>
            /// Target Volume
            /// </summary>
            public float TargetVolume = 1;

            /// <summary>
            /// Active Volume
            /// </summary>
            public float ActiveVolume;
        }

        /// <summary>
        /// Action for unsubscribing from events.
        /// </summary>
        private Action _unsub;

        /// <summary>
        /// Listen for messages.
        /// </summary>
        [Inject]
        public IMessageRouter Messages { get; set; }

        /// <summary>
        /// Audio Source Clips
        /// </summary>
        public List<Clip> Clips;

        /// <summary>
        /// Active clip name
        /// </summary>
        public string ActiveClipName;

        /// <summary>
        /// How long does it take music to fade in or out
        /// </summary>
        public float FadeDuration = 4.0f;

        /// <summary>
        /// If true, music fades out
        /// </summary>
        public bool IsMuted;

        /// <inheritdoc cref="MonoBehaviour"/>
        public void Start()
        {
            _unsub = Messages.Subscribe(
                MessageTypes.MUSIC,
                message => Play((string) message));
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        public void OnDestroy()
        {
            _unsub();
        }

        /// <summary>
        /// Transitions to the specified clip
        /// </summary>
        /// <param name="clipName"></param>
        public void Play(string clipName)
        {
            Log.Info(this, "Play {0}.", clipName);

            ActiveClipName = clipName;
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        public void Update()
        {
            var deltaTime = Time.deltaTime;

            for (int i = 0, count = Clips.Count; i < count; ++i)
            {
                var clip = Clips[i];
                if (clip != null && clip.AudioSource != null)
                {
                    var isActiveClip = !IsMuted
                        && clip.AudioSource.name == ActiveClipName;
                    var deltaVolumePct = FadeDuration > Mathf.Epsilon
                        ? deltaTime / FadeDuration
                        : 1.0f;
                    if (isActiveClip)
                    {
                        clip.ActiveVolume = Mathf.Clamp01(clip.ActiveVolume + deltaVolumePct);
                        clip.AudioSource.volume = clip.ActiveVolume * clip.TargetVolume;

                        if (!clip.AudioSource.isPlaying)
                        {
                            clip.AudioSource.Play();
                        }
                    }
                    else
                    {
                        clip.ActiveVolume = Mathf.Clamp01(clip.ActiveVolume - deltaVolumePct);
                        clip.AudioSource.volume = clip.ActiveVolume * clip.TargetVolume;

                        if (Mathf.Approximately(clip.AudioSource.volume, 0))
                        {
                            if (clip.AudioSource.isPlaying)
                            {
                                if (IsMuted)
                                {
                                    clip.AudioSource.Pause();
                                }
                                else
                                {
                                    clip.AudioSource.Stop();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
