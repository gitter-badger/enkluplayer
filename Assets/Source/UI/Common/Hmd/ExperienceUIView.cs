using System;
using CreateAR.EnkluPlayer.Assets;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    public class ExperienceUIView : MonoBehaviourIUXController
    {
        [Inject]
        public IAppController App { get; set; }
        
        [Inject]
        public IConnection Connection { get; set; }
        
        [Inject]
        public IAssetLoader AssetLoader { get; set; }
        
        [Inject]
        public ApplicationConfig _config { get; set; }
        
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
        
        [InjectElements("..txt-queue")]
        public TextWidget TxtQueue { get; set; }

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

            Select.OnValueChanged += widget =>
            {
                var tab = Select.Selection.Value;
                TabOverview.Schema.Set("visible", tab == "overview");
                TabUpdate.Schema.Set("visible", tab == "update");
            };
        }

        private void Update()
        {
            if (Select.Selection.Value == "update")
            {
                TxtQueue.Label = string.Format("Assets Loading: {0}", AssetLoader.QueueLength);
            }
        }
    }
}