using System;

namespace CreateAR.SpirePlayer.Assets
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