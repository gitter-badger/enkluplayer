using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Unity implementation of IAudioSource.
    /// </summary>
    public class UnityAudioSource : IAudioSource
    {
        /// <summary>
        /// Underlying Unity AudioSource.
        /// </summary>
        private AudioSource _audioSource;

        /// <inheritdoc />
        public float Volume
        {
            get { return _audioSource.volume; }
            set { _audioSource.volume = value; }
        }

        /// <inheritdoc />
        public bool Loop
        {
            get { return _audioSource.loop; }
            set { _audioSource.loop = value; }
        }

        /// <inheritdoc />
        public bool Mute
        {
            get { return _audioSource.mute; }
            set { _audioSource.mute = value; }
        }

        /// <inheritdoc />
        public bool PlayOnAwake
        {
            get { return _audioSource.playOnAwake; }
            set { _audioSource.playOnAwake = value; }
        }
        
        /// <inheritdoc />
        public float SpatialBlend
        {
            get { return _audioSource.spatialBlend; }
            set { _audioSource.spatialBlend = value; }
        }
        
        /// <inheritdoc />
        public float MinDistance
        {
            get { return _audioSource.minDistance; }
            set { _audioSource.minDistance = value; }
        }
        
        /// <inheritdoc />
        public float MaxDistance
        {
            get { return _audioSource.maxDistance; }
            set { _audioSource.maxDistance = value; }
        }
        
        /// <inheritdoc />
        public float DopplerLevel
        {
            get { return _audioSource.dopplerLevel; }
            set { _audioSource.dopplerLevel = value; }
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="audioSource"></param>
        public UnityAudioSource(AudioSource audioSource)
        {
            _audioSource = audioSource;
        }
    }
}