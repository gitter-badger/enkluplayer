using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an object that tracks world anchors.
    /// </summary>
    public interface IAnchorManager
    {
        /// <summary>
        /// Called to get reference to primary anchor.
        ///
        /// TODO: get rid of this
        /// </summary>
        WorldAnchorWidget Primary { get; }

        /// <summary>
        /// Manages anchor data.
        /// </summary>
        IAnchorStore Store { get; }

        /// <summary>
        /// True iff anchors are ready.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Sets up the manager.
        /// </summary>
        IAsyncToken<Void> Setup();

        /// <summary>
        /// Tears down the manager. Should be called before leaving edit mode.
        /// </summary>
        void Teardown();

        /// <summary>
        /// Called when the primary anchor is ready and located.
        /// </summary>
        /// <param name="ready">The callback to call when ready.</param>
        ICancelable OnReady(Action ready);
    }
}