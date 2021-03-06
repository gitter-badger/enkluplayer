
using System;
using System.Collections.Generic;

namespace CreateAR.EnkluPlayer.AR
{
    /// <summary>
    /// Provides an interface for working with Ar.
    /// </summary>
    public interface IArService
    {
        /// <summary>
        /// Called when AR tracking has been interrupted.
        /// </summary>
        event Action OnTrackingOffline;
        
        /// <summary>
        /// Called when AR tracking is back online.
        /// </summary>
        event Action OnTrackingOnline;
        
        /// <summary>
        /// Set of anchors we have found.
        /// </summary>
        List<ArAnchor> Anchors { get; }
        
        /// <summary>
        /// Configuration.
        /// </summary>
        ArServiceConfiguration Config { get; }
        
        /// <summary>
        /// True iff ar service has been setup.
        /// </summary>
        bool IsSetup { get; }
        
        /// <summary>
        /// Sets up the AR provider for use.
        /// </summary>
        /// <param name="config">Configuration to use.</param>
        void Setup(ArServiceConfiguration config);
        
        /// <summary>
        /// Tears down the AR provider.
        /// </summary>
        void Teardown();
    }
}
