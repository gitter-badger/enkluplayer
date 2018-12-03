using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// An interface to Unity's AudioSource.
    /// </summary>
    public class AudioJsApi
    {
        /// <summary>
        /// Underlying AudioSource to wrap.
        /// </summary>
        private readonly AudioSource _audio;

        private readonly ElementSchemaProp<float> _volumeProp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="audio"></param>
        public AudioJsApi(ElementSchema schema, AudioSource audio)
        {
            _audio = audio;

            _volumeProp = schema.GetOwn<float>("audio.volume", -1);
            
            if (_volumeProp.Value >= 0)
            {
                volume = _volumeProp.Value;
            }
            else
            {
                _volumeProp.Value = volume;
            }

            _volumeProp.OnChanged += OnVolumeChanged;
        }

        ~AudioJsApi()
        {
            _volumeProp.OnChanged -= OnVolumeChanged;
        }

        /// <summary>
        /// Volume.
        /// </summary>
        public float volume
        {
            get { return _volumeProp.Value; }
            set { _volumeProp.Value = value; }
        }

        /// <summary>
        /// Loop.
        /// </summary>
        public bool loop
        {
            get { return _audio.loop; }
            set { _audio.loop = value; }
        }

        /// <summary>
        /// Mute.
        /// </summary>
        public bool mute
        {
            get { return _audio.mute; }
            set { _audio.mute = value; }
        }

        /// <summary>
        /// Whether the audio plays on awake or not.
        /// </summary>
        public bool playOnAwake
        {
            get { return _audio.playOnAwake; }
            set { _audio.playOnAwake = value; }
        }

        /// <summary>
        /// Spatial Blend. 0: 2D, 1:3D
        /// </summary>
        public float spatialBlend
        {
            get { return _audio.spatialBlend; }
            set { _audio.spatialBlend = value; }
        }

        /// <summary>
        /// Minimum distance.
        /// </summary>
        public float minDistance
        {
            get { return _audio.minDistance; }
            set { _audio.minDistance = value; }
        }

        /// <summary>
        /// Maximum distance.
        /// </summary>
        public float maxDistance
        {
            get { return _audio.maxDistance; }
            set { _audio.maxDistance = value; }
        }

        /// <summary>
        /// Doppler level.
        /// </summary>
        public float dopplerLevel
        {
            get { return _audio.dopplerLevel; }
            set { _audio.dopplerLevel = value; }
        }

        /// <summary>
        /// Invoked when the volume changes via schema.
        /// </summary>
        private void OnVolumeChanged(ElementSchemaProp<float> prop, float prev, float @new)
        {
            _audio.volume = @new;
        }
    }
}