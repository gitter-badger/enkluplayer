using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.Assets;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    public class ExperienceUIView : MonoBehaviourIUXController
    {
        private const int PAGE_SIZE = 6;
        
        private int _assetFails;
        private int _scriptFails;

        private readonly List<Option> _options = new List<Option>();

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

        public event Action OnClose;

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

        private void RebuildFailureDisplay()
        {
            _assetFails = AssetLoader.LoadFailures.Count;
            _scriptFails = ScriptLoader.LoadFailures.Count;

            var sum = _assetFails + _scriptFails;
            var pages = sum / PAGE_SIZE;

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

        private void PopulateErrors()
        {
            var skip = PAGE_SIZE * _errorPageIndex;
            var used = 0;

            var errorOutput = "";
            if (skip < _assetFails)
            {
                for (int i = skip, len = AssetLoader.LoadFailures.Count; i < len && used < PAGE_SIZE; i++)
                {
                    var failure = AssetLoader.LoadFailures[i];
                    errorOutput += string.Format("Asset: {0} - {1}\n", failure.AssetData.AssetName, failure.Exception);
                    used++;
                }
            }

            for (int i = (skip + used) - _assetFails, len = ScriptLoader.LoadFailures.Count; i < len && used < PAGE_SIZE; i++)
            {
                var failure = ScriptLoader.LoadFailures[i];
                errorOutput += string.Format("Script: {0} - {1}\n", failure.ScriptData.Name, failure.Exception);
                used++;
            }

            TxtErrors.Label = errorOutput;
        }
    }
}