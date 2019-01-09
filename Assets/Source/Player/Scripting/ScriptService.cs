using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
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
                
                var assetTokens = new List<IMutableAsyncToken<ContentWidget>>();
                var finished = 0;

                var wtf = new List<ContentWidget>();
                
                for (int i = 0, len = _elementManager.All.Count; i < len; i++)
                {
                    var element = _elementManager.All[i];
                    var contentWidget = element as ContentWidget;

                    if (contentWidget != null)
                    {
                        if (contentWidget.Visible)
                        {
                            assetTokens.Add(contentWidget.OnAssetLoaded);
                            
                            wtf.Add(contentWidget);
                            contentWidget.OnAssetLoaded.OnFinally(_ => { 
                                Log.Warning(this, ++finished);
                                wtf.Remove(contentWidget);

                                if (finished == 380)
                                {
                                    for (int j = 0, jLen = wtf.Count; j < len; j++)
                                    {
                                        Log.Warning(this, wtf[j]);
                                    }
                                }
                            });
                        }
                    }
                    
                    Element_OnCreated(_elementManager.All[i]);
                }
                
                _elementManager.OnCreated += Element_OnCreated;
                _elementManager.OnDestroyed += Element_OnDestroyed;

                Log.Warning(this, assetTokens.Count);
                if (assetTokens.Count == 0)
                {
                    StartRunner();
                }
                else
                {
                    Async.All(assetTokens.ToArray()).OnFinally(_ =>
                    {
                        StartRunner();
                    });
                }
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

        private void StartRunner()
        {
            Log.Warning(this, "Starting ScriptRunner");
            _scriptRunner.ParseAll();
            _scriptRunner.StartAllScripts();
        }

        private void Element_OnCreated(Element element)
        {
            var widget = element as Widget;
            if (widget == null)
            {
                return;
            }
            
            _scriptRunner.AddWidget(widget);
        }

        private void Element_OnDestroyed(Element element)
        {
            var widget = element as Widget;
            if (widget == null)
            {
                return;
            }
            
            _scriptRunner.RemoveWidget(widget);
        }
    }
}