using System;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Test data.
    /// </summary>
    [Serializable]
    public class ApplicationTestData
    {
        /// <summary>
        /// Asset data to be injected.
        /// </summary>
        public TextAsset Asset;

        /// <summary>
        /// Content data to be injected.
        /// </summary>
        public TextAsset Content;
    }
}