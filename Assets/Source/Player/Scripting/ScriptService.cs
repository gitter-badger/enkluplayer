using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Manages all scripting for experiences. 
    /// </summary>
    public class ScriptService : ApplicationService
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IAppSceneManager _sceneManager;
        private readonly IAppController _app;

        /// <summary>
        /// Manages loading and running scripts for given root level widgets.
        /// </summary>
        private readonly ScriptRunner _scriptRunner;
        
        private readonly List<Element> _loadedSceneRoots = new List<Element>();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ScriptService(
            MessageTypeBinder binder, 
            IMessageRouter messages,
            IScriptManager scriptManager,
            IScriptFactory scriptFactory,
            IScriptExecutorFactory scriptExecutorFactory,
            IAppSceneManager sceneManager,
            IAppController appController) : base(binder, messages)
        {
            _sceneManager = sceneManager;
            _app = appController;
            
            _scriptRunner = new ScriptRunner(scriptManager, scriptFactory, scriptExecutorFactory);
        }

        /// <inheritdoc />
        public override void Start()
        {
            base.Start();

            _app.OnReady += App_OnLoad;
            _app.OnUnloaded += App_OnUnload;
        }

        /// <inheritdoc />
        public override void Update(float dt)
        {
            base.Update(dt);

            _scriptRunner.Update();
        }

        /// <inheritdoc />
        public override void Stop()
        {
            base.Stop();
            
            _app.OnReady -= App_OnLoad;
            _app.OnUnloaded -= App_OnUnload;
            
            _scriptRunner.StopRunner();
        }

        private void App_OnLoad()
        {
            Log.Warning(this, "App_OnLoad");

            // TODO: Handle multiple scenes
            var root = _sceneManager.Root(_sceneManager.All[0]);
            _loadedSceneRoots.Add(root);
                
            _scriptRunner.AddSceneRoot(root);
            _scriptRunner.StartRunner();
        }

        private void App_OnUnload()
        {
            Log.Warning(this, "App_OnUnload");

            _scriptRunner.StopRunner();
            
            for (int i = 0, len = _loadedSceneRoots.Count; i < len; i++)
            {
                _scriptRunner.RemoveSceneRoot(_loadedSceneRoots[i]);
            }
            _loadedSceneRoots.Clear();
        }
    }
}