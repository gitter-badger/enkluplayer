using System;
using System.Collections;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Http.Editor;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis;
using UnityEditor;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Holds objects and builds them appropriately for edit mode..
    /// </summary>
    [InitializeOnLoad]
    public static class EditorApplication
    {
        /// <summary>
        /// Backing variable for Bootstrapper.
        /// </summary>
        private static readonly EditorBootstrapper _bootstrapper = new EditorBootstrapper();
        
        /// <summary>
        /// Dependencies.
        /// </summary>
        public static IBootstrapper Bootstrapper { get { return _bootstrapper; } }
        public static IHttpService Http { get; private set; }
        public static ApiController Api { get; private set; }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static EditorApplication()
        {
            Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter
            {
                Timestamp = false,
                Level = false
            }));
            
            UnityEditor.EditorApplication.update += _bootstrapper.Update;
            
            Http = new EditorHttpService(new JsonSerializer(), _bootstrapper);
            Api = new ApiController(Http);
        }
    }
}