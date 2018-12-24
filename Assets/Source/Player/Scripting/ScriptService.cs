using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Scripting
{
    public class ScriptService : ApplicationService
    {
        private readonly IAppSceneManager _sceneManager;
        private readonly IElementManager _elementManager;

        private readonly ScriptRunner _scriptRunner;
        
        public ScriptService(
            MessageTypeBinder binder, 
            IMessageRouter messages,
            IScriptManager scriptManager,
            IScriptFactory scriptFactory,
            IScriptRequireResolver requireResolver,
            IAppSceneManager sceneManager,
            IElementManager elementManager,
            IElementJsCache elementJsCache) : base(binder, messages)
        {
            _sceneManager = sceneManager;
            _elementManager = elementManager;
            
            _scriptRunner = new ScriptRunner(
                scriptManager, scriptFactory, requireResolver, elementJsCache);
        }

        public override void Start()
        {
            base.Start();

            _sceneManager.OnInitialized += () =>
            {
                for (int i = 0, len = _elementManager.All.Count; i < len; i++)
                {
                    Element_OnCreated(_elementManager.All[i]);
                }
                
                _scriptRunner.ParseAll();
                
                _elementManager.OnCreated += Element_OnCreated;
                
                _scriptRunner.StartScripts();
            };
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            _scriptRunner.Update();
        }

        public override void Stop()
        {
            base.Stop();
            
            _elementManager.OnCreated -= Element_OnCreated;
            
            _scriptRunner.StopScripts();
        }

        private void Element_OnCreated(Element element)
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