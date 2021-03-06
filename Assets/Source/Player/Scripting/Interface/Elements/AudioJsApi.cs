﻿using System;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
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
        private readonly IAudioSource _audio;

        /// <summary>
        /// Backing Schema prop.
        /// </summary>
        private readonly ElementSchemaProp<float> _volumeProp;

        /// <summary>
        /// Tracks whether Setup has been called.
        /// </summary>
        private bool _setup;
        
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
            get { return _audio.Loop; }
            set { _audio.Loop = value; }
        }

        /// <summary>
        /// Mute.
        /// </summary>
        public bool mute
        {
            get { return _audio.Mute; }
            set { _audio.Mute = value; }
        }

        /// <summary>
        /// Whether the audio plays on awake or not.
        /// </summary>
        public bool playOnAwake
        {
            get { return _audio.PlayOnAwake; }
            set { _audio.PlayOnAwake = value; }
        }

        /// <summary>
        /// Spatial Blend. 0: 2D, 1:3D
        /// </summary>
        public float spatialBlend
        {
            get { return _audio.SpatialBlend; }
            set { _audio.SpatialBlend = value; }
        }

        /// <summary>
        /// Minimum distance.
        /// </summary>
        public float minDistance
        {
            get { return _audio.MinDistance; }
            set { _audio.MinDistance = value; }
        }

        /// <summary>
        /// Maximum distance.
        /// </summary>
        public float maxDistance
        {
            get { return _audio.MaxDistance; }
            set { _audio.MaxDistance = value; }
        }

        /// <summary>
        /// Doppler level.
        /// </summary>
        public float dopplerLevel
        {
            get { return _audio.DopplerLevel; }
            set { _audio.DopplerLevel = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="audio"></param>
        public AudioJsApi(ElementSchema schema, IAudioSource audio)
        {
            _audio = audio;

            _volumeProp = schema.GetOwn<float>("audio.volume", -1);
        }

        /// <summary>
        /// Subscribes to schema.
        /// </summary>
        public void Setup()
        {
            if (_setup)
            {
                throw new Exception("AudioJsApi already setup.");
            }
            
            if (_volumeProp.Value >= 0)
            {
                // Set initial
                Volume_OnChanged(_volumeProp, _volumeProp.Value, _volumeProp.Value);
            }
            else
            {
                // Load from prefab
                _volumeProp.Value = volume;
            }

            _volumeProp.OnChanged += Volume_OnChanged;

            _setup = true;
        }

        /// <summary>
        /// Unsubscribes from schema.
        /// </summary>
        public void Teardown()
        {
            if (!_setup)
            {
                throw new Exception("AudioJsApi not setup.");
            }
            
            _volumeProp.OnChanged -= Volume_OnChanged;

            _setup = false;
        }

        /// <summary>
        /// Invoked when the volume changes via schema.
        /// </summary>
        private void Volume_OnChanged(ElementSchemaProp<float> prop, float prev, float @new)
        {
            _audio.Volume = @new;
        }
    }
}