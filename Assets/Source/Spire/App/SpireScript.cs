using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Jint.Parser.Ast;
using UnityEngine;

namespace CreateAR.Spire
{
    /// <summary>
    /// A dynamically executable script. 
    /// </summary>
    public class SpireScript
    {
        /// <summary>
        /// For parsing scripts.
        /// </summary>
        private readonly IScriptParser _parser;

        /// <summary>
        /// To unsubscribe from asset updates.
        /// </summary>
        private readonly Action _unsubscribe;

        /// <summary>
        /// Backing variable for OnReady.
        /// </summary>
        private readonly AsyncToken<SpireScript> _onReady = new AsyncToken<SpireScript>();

        /// <summary>
        /// Underlying asset we load from.
        /// </summary>
        public readonly Asset Asset;

        /// <summary>
        /// Data about the script.
        /// </summary>
        public readonly ScriptData Data;

        /// <summary>
        /// Token when script is first available to execute.
        /// </summary>
        public IAsyncToken<SpireScript> OnReady { get { return _onReady; } }

        /// <summary>
        /// Program that can be executed.
        /// </summary>
        public Program Program { get; private set; }

        /// <summary>
        /// Creates a new SpireScript.
        /// </summary>
        /// <param name="parser">Parses JS.</param>
        /// <param name="data">Information about the script.</param>
        /// <param name="asset">The AssetReference to load.</param>
        public SpireScript(
            IScriptParser parser,
            ScriptData data,
            Asset asset)
        {
            _parser = parser;
            Data = data;
            Asset = asset;

            // watch for updates to the underlying asset
            _unsubscribe = Asset.WatchAsset<TextAsset>(Asset_OnAssetUpdated);

            // set to true!
            Asset.AutoReload = true;
        }
        
        /// <summary>
        /// Should not be called directly. Use <c>IScriptManager</c>::Release().
        /// </summary>
        public void Release()
        {
            _unsubscribe();
        }

        /// <summary>
        /// Called when the underlying asset has been updated.
        /// </summary>
        /// <param name="asset">The asset.</param>
        private void Asset_OnAssetUpdated(TextAsset asset)
        {
            Log.Info(this, "Script updated, parsing asset.");

            // parse!
            _parser
                .Parse(asset.text)
                .OnSuccess(program =>
                {
                    Log.Info(this, "Script parsed and ready.");

                    Program = program;

                    _onReady.Succeed(this);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not parse {0} : {1}.",
                        Data,
                        exception);

                    _onReady.Fail(exception);
                });
        }
    }
}