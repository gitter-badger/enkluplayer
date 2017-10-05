using System;
using CreateAR.SpirePlayer;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manifest of all assets needed for an app.
    /// </summary>
    [Serializable]
    public class AssetDataManifest
    {
        /// <summary>
        /// Assets.
        /// </summary>
        public AssetData[] Assets;
    }
}