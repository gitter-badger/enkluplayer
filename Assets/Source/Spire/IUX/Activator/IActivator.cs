using System;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Fundemenal primitive of user engagement.
    /// </summary>
    public interface IActivator : IElement, IInteractive
    {
        /// <summary>
        /// Bounding radius of the activator.
        /// TODO: Spherical bounds are an implementation detail
        ///       Refractor out of interface
        /// </summary>
        float Radius { get; } 

        /// <summary>
        /// (IUX PATENT)
        /// A scalar percentage [0..1] representing targeting clarity.
        /// 0 = low clarity - may be aiming at the edge of this.
        /// 1 = high clarity - definitely targeting at center of this.
        /// </summary>
        float Aim { get; }

        /// <summary>
        /// (IUX PATENT)
        /// A scalar percentage [0..1] representing targeting steadiness.
        /// 0 = low steadiness -> may be moving over on way to something else.
        /// 1 - high steadiness -> definitely stationary over this.
        /// </summary>
        float Stability { get; }

        /// <summary>
        /// (IUX PATENT)
        /// A scalar percentage [0..1] representing activation completion.
        /// </summary>
        float Activation { get; }

        /// <summary>
        /// Invoked when the activator is activated.
        /// </summary>
        event Action<IActivator> OnActivated;

        /// <summary>
        /// Forced activation.
        /// </summary>
        void Activate();
    }
}
