﻿using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.SpirePlayer.IUX;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an interface for an object that listens for scene changes as
    /// well as requests scene changes from server.
    /// </summary>
    public interface IElementTxnManager
    {
        /// <summary>
        /// Ids of loaded scenes.
        /// </summary>
        string[] TrackedScenes { get; }

        /// <summary>
        /// Called when a new scene is being tracked.
        /// </summary>
        event Action<string> OnSceneAfterTracked;

        /// <summary>
        /// Called when an existing scene is no longer being tracked.
        /// </summary>
        event Action<string> OnSceneBeforeUntracked;

        /// <summary>
        /// Initializes the manager for an app.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        /// <returns></returns>
        IAsyncToken<Void> Initialize(string appId);

        /// <summary>
        /// Unintializes the manager.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Uninitialize();

        /// <summary>
        /// Retrieves the root element of a scene.
        /// </summary>
        /// <param name="sceneId">The id of a scene.</param>
        /// <returns></returns>
        Element Root(string sceneId);

        /// <summary>
        /// Watches for scene updates.
        /// </summary>
        /// <param name="sceneId">The id of the scene.</param>
        IAsyncToken<Void> TrackScene(string sceneId);

        /// <summary>
        /// Stops watching a scene.
        /// </summary>
        /// <param name="sceneId">The id of the scene.</param>
        void UntrackScene(string sceneId);

        /// <summary>
        /// Requests a scene change.
        /// </summary>
        /// <param name="txn">The transaction we're requesting.</param>
        IAsyncToken<ElementResponse> Request(ElementTxn txn);

        /// <summary>
        /// Applies a scene change.
        /// </summary>
        /// <param name="txn">The transaction we're requesting.</param>
        ElementResponse Apply(ElementTxn txn);
    }
}