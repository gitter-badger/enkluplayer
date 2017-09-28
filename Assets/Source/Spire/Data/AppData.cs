using System;
using System.Collections.Generic;

namespace CreateAR.Spire
{
    [Serializable]
    public class AppData
    {
        /// <summary>
        /// UUID of this App.
        /// </summary>
        public string Id;

        /// <summary>
        /// Name.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Scenes.
        /// </summary>
        public List<string> Scenes = new List<string>();
    }
}