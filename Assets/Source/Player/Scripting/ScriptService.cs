using System;
using CreateAR.Commons.Unity.Logging;
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
            IElementJsCache elementJsCache,
            AppJsApi appJsApi) : base(binder, messages)
        {
            _sceneManager = sceneManager;
            _elementManager = elementManager;
            
            _scriptRunner = new ScriptRunner(
                scriptManager, scriptFactory, elementJsCache, requireResolver, appJsApi);
        }

        public override void Start()
        {
            base.Start();

            _sceneManager.OnInitialized += () =>
            {
                Log.Warning(this, "Scene OnInitialized");

                // TODO: Handle multiple scenes
                var widgetRoot = _sceneManager.Root(_sceneManager.All[0]) as Widget;

                if (widgetRoot == null)
                {
                    Log.Error(this, "Scene root isn't a Widget!");
                    return;
                }
                
                _scriptRunner.AddSceneRoot(widgetRoot);
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
            
            _scriptRunner.StopAllScripts();
        }

        private void Element_OnCreated(Element element)
        {
            Widget widget = element as Widget;
            if (widget == null)
            {
                return;
            }
            
            Log.Error(this, "Still working on it!");
        }
    }
}