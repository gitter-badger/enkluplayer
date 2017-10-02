using System;
using System.Collections.Generic;

namespace CreateAR.Spire
{
    [Serializable]
    public class AppData : StaticData
    {
        /// <summary>
        /// Scenes.
        /// </summary>
        public List<string> Scenes = new List<string>();
    }
}