using System;
using CreateAR.SpirePlayer;
using UnityEngine;

namespace CreateAR.Spire
{
    public class SpireScript
    {
        private readonly Action _unsubscribe;

        public readonly Asset Reference;
        public readonly ScriptInfo Info;
        
        /// <summary>
        /// Creates a new SpireScript.
        /// </summary>
        /// <param name="info">Information about the script.</param>
        /// <param name="reference">The AssetReference to load.</param>
        public SpireScript(
            ScriptInfo info,
            Asset reference)
        {
            Info = info;

            Reference = reference;
            _unsubscribe = Reference.WatchAsset<TextAsset>(Reference_OnAssetUpdated);
        }

        private void Reference_OnAssetUpdated(TextAsset asset)
        {
            
        }
    }
}