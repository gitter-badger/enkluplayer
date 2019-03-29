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
        private readonly IElementManager _elementManager;

        /// <summary>
        /// Manages loading and running scripts for given root level widgets.
        /// </summary>
        private readonly ScriptRunner _scriptRunner;
        
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
            IElementManager elementManager) : base(binder, messages)
        {
            _sceneManager = sceneManager;
            _elementManager = elementManager;
            
            _scriptRunner = new ScriptRunner(scriptManager, scriptFactory, scriptExecutorFactory);
        }

        /// <inheritdoc />
        public override void Start()
        {
            base.Start();

            _sceneManager.OnInitialized += () =>
            {
                Log.Warning(this, "Scene OnInitialized");

                // TODO: Handle multiple scenes
                var root = _sceneManager.Root(_sceneManager.All[0]);
                
                _scriptRunner.AddSceneRoot(root);
                _scriptRunner.StartRunner();
            };
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
            
            _elementManager.OnCreated -= Element_OnCreated;
            
            _scriptRunner.StopRunner();
        }

        /// <summary>
        /// Invoked when a new Element has been created.
        /// </summary>
        /// <param name="element"></param>
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