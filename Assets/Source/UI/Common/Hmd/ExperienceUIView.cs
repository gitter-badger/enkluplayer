using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.Assets;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Debug UI that displays Experience related information (environment/name) as well as scene loading stats.
    /// </summary>
    public class ExperienceUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Number of errors to display per page.
        /// </summary>
        private const int ERROR_PAGE_SIZE = 6;
        
        /// <summary>
        /// Cached number of asset failures.
        /// </summary>
        private int _assetFails;
        
        /// <summary>
        /// Cached number of script failures.
        /// </summary>
        private int _scriptFails;

        /// <summary>
        /// Current Options being used in the error display.
        /// </summary>
        private readonly List<Option> _options = new List<Option>();

        /// <summary>
        /// Current index to show for the error display.
        /// </summary>
        private int _errorPageIndex;
        
        /// <summary>
        /// Dependencies.
        /// </summary>
        [Inject]
        public IAppController App { get; set; }
        
        [Inject]
        public IConnection Connection { get; set; }
        
        [Inject]
        public IAssetLoader AssetLoader { get; set; }
        
        [Inject]
        public IScriptLoader ScriptLoader { get; set; }
        
        [Inject]
        public ApplicationConfig _config { get; set; }
        
        /// <summary>
        /// Injected controls.
        /// </summary>
        [InjectElements("..btn-close")]
        public ButtonWidget BtnClose { get; set; }
        
        [InjectElements("..slt-tab")]
        public SelectWidget Select { get; set; }
        
        [InjectElements("..tab-overview")]
        public ContainerWidget TabOverview { get; set; }
        
        [InjectElements("..txt-environment")]
        public TextWidget TxtEnvironment { get; set; }
        
        [InjectElements("..txt-experience")]
        public TextWidget TxtExperience { get; set; }
        
        [InjectElements("..txt-connection")]
        public TextWidget TxtConnection { get; set; }
        
        [InjectElements("..tab-update")]
        public ContainerWidget TabUpdate { get; set; }
        
        [InjectElements("..txt-asset-queue")]
        public TextWidget TxtAssetQueue { get; set; }
        
        [InjectElements("..txt-script-queue")]
        public TextWidget TxtScriptQueue { get; set; }
        
        [InjectElements("..slt-errors")]
        public SelectWidget ErrorSelect { get; set; }
        
        [InjectElements("..txt-errors")]
        public TextWidget TxtErrors { get; set; }

        /// <summary>
        /// Invoked when the UI should close.
        /// </summary>
        public event Action OnClose;

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();
            
            TxtEnvironment.Label = _config.Network.Environment.Name;
            TxtExperience.Label = App.Name;
            TxtConnection.Label = string.Format("Live updates: {0}", Connection.IsConnected);

            BtnClose.OnActivated += _ =>
            {
                if (OnClose != null)
                {
                    OnClose();
                }
            };

            Action<SelectWidget> onTagChange = widget =>
            {
                var tab = Select.Selection.Value;
                TabOverview.Schema.Set("visible", tab == "overview");
                TabUpdate.Schema.Set("visible", tab == "update");
            };
            
            Select.OnValueChanged += onTagChange;
            onTagChange(Select);

            ErrorSelect.OnValueChanged += widget =>
            {
                _errorPageIndex = int.Parse(ErrorSelect.Selection.Value);
                PopulateErrors();
            };
        }

        /// <summary>
        /// Updates the UI.
        /// </summary>
        private void Update()
        {
            if (Select.Selection.Value == "update")
            {
                TxtAssetQueue.Label = string.Format("Assets Loading: {0}", AssetLoader.QueueLength);
                TxtScriptQueue.Label = string.Format("Scripts Loading: {0}", ScriptLoader.QueueLength);
            }
            
            if (AssetLoader.LoadFailures.Count != _assetFails || ScriptLoader.LoadFailures.Count != _scriptFails)
            {
                RebuildFailureDisplay();
            }
        }

        /// <summary>
        /// Rebuilds the error display, adding/removing options as needed.
        /// Triggers a refresh of the displayed text as well.
        /// </summary>
        private void RebuildFailureDisplay()
        {
            _assetFails = AssetLoader.LoadFailures.Count;
            _scriptFails = ScriptLoader.LoadFailures.Count;

            var sum = _assetFails + _scriptFails;
            var pages = sum / ERROR_PAGE_SIZE;

            if (pages > _options.Count)
            {
                for (var i = _options.Count; i < pages; i++)
                {
                    var option = new Option();
                    option.Label = "Errors - Page " + (i + 1);
                    option.Value = i.ToString();
                    _options.Add(option);
                    
                    ErrorSelect.AddChild(option);
                }
            } 
            else if (pages < _options.Count)
            {
                for (var i = _options.Count - 1; i >= pages; i++)
                {
                    var option = _options[i];
                    _options.RemoveAt(i);

                    ErrorSelect.RemoveChild(option);
                }
            }

            if (_errorPageIndex >= pages)
            {
                _errorPageIndex = pages - 1;
            }

            PopulateErrors();
        }

        /// <summary>
        /// Refreshes the currently displayed errors based on the currently selected page.
        /// </summary>
        private void PopulateErrors()
        {
            var skip = ERROR_PAGE_SIZE * _errorPageIndex;
            var used = 0;

            var errorOutput = "";
            if (skip < _assetFails)
            {
                for (int i = skip, len = AssetLoader.LoadFailures.Count; i < len && used < ERROR_PAGE_SIZE; i++)
                {
                    var failure = AssetLoader.LoadFailures[i];
                    errorOutput += string.Format("Asset: {0} - {1}\n", failure.AssetData.AssetName, failure.Exception);
                    used++;
                }
            }

            var start = (skip + used) - _assetFails;
            for (int i = start, len = ScriptLoader.LoadFailures.Count; i < len && used < ERROR_PAGE_SIZE; i++)
            {
                var failure = ScriptLoader.LoadFailures[i];
                errorOutput += string.Format("Script: {0} - {1}\n", failure.ScriptData.Name, failure.Exception);
                used++;
            }

            TxtErrors.Label = errorOutput;
        }
    }
}