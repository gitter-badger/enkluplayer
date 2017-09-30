using System;
using CreateAR.SpirePlayer;

namespace CreateAR.Spire
{
    /// <summary>
    /// Manifest of all assets needed for an app.
    /// </summary>
    [Serializable]
    public class AssetInfoManifest
    {
        /// <summary>
        /// Assets.
        /// </summary>
        public AssetInfo[] Assets;
    }
}