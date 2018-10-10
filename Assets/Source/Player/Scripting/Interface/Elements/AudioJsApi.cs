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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="audio"></param>
        public AudioJsApi(AudioSource audio)
        {
            _audio = audio;
        }

        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <returns></returns>
        public float getVolume()
        {
            return _audio.volume;
        }

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="volume"></param>
        public void setVolume(float volume)
        {
            _audio.volume = volume;
        }

        /// <summary>
        /// Gets looping.
        /// </summary>
        /// <returns></returns>
        public bool getLoop()
        {
            return _audio.loop;
        }

        /// <summary>
        /// Sets looping.
        /// </summary>
        /// <param name="loop"></param>
        public void setLoop(bool loop)
        {
            _audio.loop = loop;
        }

        /// <summary>
        /// Gets mute.
        /// </summary>
        /// <returns></returns>
        public bool getMute()
        {
            return _audio.mute;
        }

        /// <summary>
        /// Sets mute.
        /// </summary>
        public void setMute(bool mute)
        {
            _audio.mute = mute;
        }

        /// <summary>
        /// Gets playOnAwake.
        /// </summary>
        /// <returns></returns>
        public bool getPlayOnAwake()
        {
            return _audio.playOnAwake;
        }

        /// <summary>
        /// Sets playOnAwake.
        /// </summary>
        /// <param name="playOnAwake"></param>
        public void setPlayOnAwake(bool playOnAwake)
        {
            _audio.playOnAwake = playOnAwake;
        }

        /// <summary>
        /// Gets spatial blend.
        /// </summary>
        /// <returns></returns>
        public float getSpatialBlend()
        {
            return _audio.spatialBlend;
        }

        /// <summary>
        /// Sets spatial blend.
        /// </summary>
        /// <param name="blend"></param>
        public void setSpatialBlend(float blend)
        {
            _audio.spatialBlend = blend;
        }

        /// <summary>
        /// Gets min distance.
        /// </summary>
        /// <returns></returns>
        public float getMinDistance()
        {
            return _audio.minDistance;
        }

        /// <summary>
        /// Sets min distance.
        /// </summary>
        /// <param name="distance"></param>
        public void setMinDistance(float distance)
        {
            _audio.minDistance = distance;
        }

        /// <summary>
        /// Gets max distance.
        /// </summary>
        /// <returns></returns>
        public float getMaxDistance()
        {
            return _audio.maxDistance;
        }

        /// <summary>
        /// Sets max distance.
        /// </summary>
        /// <param name="distance"></param>
        public void setMaxDistance(float distance)
        {
            _audio.maxDistance = distance;
        }

        /// <summary>
        /// Gets the doppler level.
        /// </summary>
        /// <returns></returns>
        public float getDoplerLevel()
        {
            return _audio.dopplerLevel;
        }

        /// <summary>
        /// Sets the doppler level.
        /// </summary>
        /// <param name="doppler"></param>
        public void setDoplerLevel(float doppler)
        {
            _audio.dopplerLevel = doppler;
        }
    }
}