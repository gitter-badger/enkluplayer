#if !UNITY_EDITOR && UNITY_WSA

using System;
using System.Collections.Generic;
using CreateAR.EnkluPlayer.AR;
using UnityEngine.XR.WSA;
using Enklu.Data;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// <c>IArService</c> implementation for HoloLens.
    /// </summary>
    public class HoloLensArService : IArService
    {
        /// <inheritdoc />
        public event Action OnTrackingOffline;
        
        /// <inheritdoc />
        public event Action OnTrackingOnline;

        /// <inheritdoc />
        public List<ArAnchor> Anchors { get; private set; }

        /// <inheritdoc />
        public ArServiceConfiguration Config { get; private set; }
    
        /// <inheritdoc />
        public bool IsSetup { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public HoloLensArService()
        {
            Anchors = new List<ArAnchor>
            {
                new ArAnchor("floor")
                {
                    Extents = new Vec3(1000, 1000, 1000),
                    Position = new Vec3(0, 0, 0),
                    Rotation = Quat.Euler(0, 0, 0)
                },
            };
        }

        /// <inheritdoc />
        public void Setup(ArServiceConfiguration config)
        {
            WorldManager.OnPositionalLocatorStateChanged += WorldManager_OnPositionalLocatorStateChanged;
    
            IsSetup = true;
        }

        /// <inheritdoc />
        public void Teardown()
        {
            IsSetup = false;
    
            WorldManager.OnPositionalLocatorStateChanged -= WorldManager_OnPositionalLocatorStateChanged;
        }
        
        /// <summary>
        /// Called by MS API.
        /// </summary>
        /// <param name="oldState">Previous state.</param>
        /// <param name="newState">New state.</param>
        private void WorldManager_OnPositionalLocatorStateChanged(PositionalLocatorState oldState, PositionalLocatorState newState)
        {
            if (newState == PositionalLocatorState.Active)
            {
                if (null != OnTrackingOnline)
                {
                    OnTrackingOnline();
                }
            }
            else
            {
                if (null != OnTrackingOffline)
                {
                    OnTrackingOffline();
                }
            }
        }
    }
}

#endif
