using System;
using System.Collections.Generic;
using Enklu.Data;

namespace CreateAR.EnkluPlayer.AR
{
    /// <summary>
    /// Passthrough implementation of <c>IArService</c> that provides a floor.
    /// </summary>
    public class PassthroughArService : IArService
    {
        /// <inheritdoc />
        public event Action OnTrackingOffline;
        
        /// <inheritdoc />
        public event Action OnTrackingOnline;
        
        /// <inheritdoc />
        public List<ArAnchor> Anchors
        {
            get
            {
                return new List<ArAnchor>
                {
                    new ArAnchor("floor")
                    {
                        Extents = new Vec3(1, 0, 1),
                        Position = new Vec3(0f, 0f, 0f),
                        Rotation = Quat.Euler(0, 0, 0)
                    },
                };
            }
        }

        /// <inheritdoc />
        public ArServiceConfiguration Config { get; private set; }

        /// <inheritdoc />
        public bool IsSetup { get; private set; }

        /// <inheritdoc />
        public void Setup(ArServiceConfiguration config)
        {
            Config = config;

            IsSetup = true;
        }

        /// <inheritdoc />
        public void Teardown()
        {
            IsSetup = false;
        }

        /// <summary>
        /// Forces trackingt offline.
        /// </summary>
        public void ForceOffline()
        {
            if (null != OnTrackingOffline)
            {
                OnTrackingOffline();
            }
        }

        /// <summary>
        /// Forces tracking online.
        /// </summary>
        public void ForceOnline()
        {
            if (null != OnTrackingOnline)
            {
                OnTrackingOnline();
            }
        }
    }
}
