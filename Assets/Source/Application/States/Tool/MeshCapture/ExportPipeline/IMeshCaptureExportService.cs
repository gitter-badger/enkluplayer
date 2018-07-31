using System;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an asynchronous pipeline that exports captures.
    /// </summary>
    public interface IMeshCaptureExportService
    {
        /// <summary>
        /// Configuration object.
        /// </summary>
        MeshCaptureExportServiceConfiguration Configuration { get; }

        /// <summary>
        /// Called when file is created.
        /// </summary>
        event Action<string> OnFileCreated;

        /// <summary>
        /// Called when file url was changed.
        /// </summary>
        event Action<string> OnFileUrlChanged;

        /// <summary>
        /// Starts processing. Calls to <c>Export</c> must follow a Start and 
        /// preceede a Stop. Start/Stop may be called many times.
        /// </summary>
        /// <param name="fileId">The id of the file to export to. If left null,
        /// OnFileUrlChanged will be called when a file is initially created.</param>
        void Start(string fileId = null);

        /// <summary>
        /// Stops processing.
        /// </summary>
        void Stop();

        /// <summary>
        /// Saves snapshot of objects passed in.
        /// </summary>
        /// <param name="triangles">Number of triangles to export.</param>
        /// <param name="gameObjects">The gameobjects for which to take a snapshot.</param>
        /// <returns></returns>
        bool Export(out int triangles, params GameObject[] gameObjects);
    }
}