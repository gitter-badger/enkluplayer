using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Scripting
{
    public class ScriptService : ApplicationService
    {
        private readonly IElementManager _elementManager;

        private readonly ScriptRunner _scriptRunner;
        
        public ScriptService(
            MessageTypeBinder binder, 
            IMessageRouter messages,
            IScriptManager scriptManager,
            IScriptFactory scriptFactory,
            IElementManager elementManager,
            IElementJsCache elementJsCache,
            EnkluScriptRequireResolver requireResolver) : base(binder, messages)
        {
            _elementManager = elementManager;
            
            _scriptRunner = new ScriptRunner(
                scriptManager, scriptFactory, requireResolver, elementJsCache);
        }

        public override void Start()
        {
            base.Start();

            _elementManager.OnCreated += OnElementCreated;

            for (int i = 0, len = _elementManager.All.Count; i < len; i++)
            {
                OnElementCreated(_elementManager.All[i]);
            }
            
            _scriptRunner.ParseAll();
        }

        public override void Stop()
        {
            base.Stop();
            
            _elementManager.OnCreated -= OnElementCreated;
        }

        private void OnElementCreated(Element element)
        {
            Widget widget = element as Widget;
            if (widget == null)
            {
                return;
            }
            
            _scriptRunner.AddWidget(widget);
        }
    }
}