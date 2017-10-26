﻿using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Loads <c>AppData</c> and provides mechanisms for querying it.
    /// </summary>
    public interface IAppDataManager
    {
        /// <summary>
        /// This is called when loaded.
        /// </summary>
        event Action OnLoaded;

        /// <summary>
        /// This is called when unloaded.
        /// </summary>
        event Action OnUnloaded;

        /// <summary>
        /// Called when a piece of <c>StaticData</c> has been removed.
        /// </summary>
        event Action<StaticData> OnRemoved;

        /// <summary>
        /// Called when a piece of <c>StaticData</c> has been updated.
        /// </summary>
        event Action<StaticData> OnUpdated;

        /// <summary>
        /// Id of the currently loaded app.
        /// 
        /// Only non null between OnLoaded + OnUnloaded.
        /// </summary>
        string LoadedApp { get; }

        /// <summary>
        /// Retrieves a type of data.
        /// </summary>
        /// <typeparam name="T">The type of data to retrieve.</typeparam>
        /// <param name="id">The id of the data.</param>
        /// <returns></returns>
        T Get<T>(string id) where T : StaticData;

        /// <summary>
        /// Retrieves all of a type of data.
        /// </summary>
        /// <typeparam name="T">The type of data to retrieve.</typeparam>
        /// <returns></returns>
        T[] GetAll<T>() where T : StaticData;

        /// <summary>
        /// Retrieves the first data object with a matching name and type.
        /// </summary>
        /// <typeparam name="T">The type of data to retrieve.</typeparam>
        /// <param name="name">The name of the data.</param>
        /// <returns></returns>
        T GetByName<T>(string name) where T : StaticData;

        /// <summary>
        /// Loads all data for an app by id.
        /// </summary>
        /// <param name="id">Unique id.</param>
        /// <returns></returns>
        IAsyncToken<Void> Load(string id);

        /// <summary>
        /// Unloads currently loaded scene.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Unload();
    }
}