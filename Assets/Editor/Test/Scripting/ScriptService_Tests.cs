using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Vine;
using Jint.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class ScriptService_Tests
    {
        private TestScriptManager _scriptManager;
        private ScriptService _scriptService;
        private IElementManager _elementManager;
        
        private TestScriptFactory _scriptFactory;
        private EnkluScript[] _behaviors = new EnkluScript[3];
        private EnkluScript[] _vines = new EnkluScript[3];

        private IAppSceneManager _sceneManager;
        
        [SetUp]
        public void Setup()
        {
            _sceneManager = new TestSceneManager();
            _scriptManager = new TestScriptManager();
            _scriptFactory = new TestScriptFactory();
            _elementManager = new TestElementManager();
            _scriptService = new ScriptService(
                null, 
                null, 
                _scriptManager, 
                _scriptFactory,
                null,
                _sceneManager,
                _elementManager, 
                new ElementJsCache(null));
            
            var parser = new DefaultScriptParser(
                null, new JsVinePreProcessor(), new JavaScriptParser());
            
            // For testing, just load the script IDs as the source program.
            for (var i = 0; i < _behaviors.Length; i++)
            {
                var behaviorData = new ScriptData
                {
                    Id = "script-behavior-" + i
                };
                _behaviors[i] = new EnkluScript(
                    parser, new TestScriptLoader(behaviorData.Id), behaviorData);
            }

            for (var i = 0; i < _vines.Length; i++)
            {
                var vineData = new ScriptData
                {
                    Id = "script-vine-" + i,
                    TagString = "vine"
                };
                _vines[i] = new EnkluScript(
                    parser, new TestScriptLoader(vineData.Id), vineData);
            }
        }

        /// Tests a scene loading from scratch.
        [Test]
        public void InitialScene()
        {
            var behaviorWidget = CreateWidget(_behaviors[0]);
            _elementManager.Add(behaviorWidget);
            
            var vineWidget = CreateWidget(_vines[0]);
            _elementManager.Add(vineWidget);
            
            _scriptService.Start();
            _sceneManager.Initialize("test", null);

            var vineComponent = _scriptFactory.GetVine(_vines[0]);
            var behaviorComponent = _scriptFactory.GetBehavior(_behaviors[0]);
            
            Assert.AreEqual(0, vineComponent.EnterInvoked);
            Assert.AreEqual(0, behaviorComponent.EnterInvoked);
            
            vineComponent.FinishConfigure();
            
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            Assert.AreEqual(1, behaviorComponent.EnterInvoked);
        }

        /// Tests elements being added to an already added scene.
        [Test]
        public void NewElements()
        {
            _scriptService.Start();
            _sceneManager.Initialize("test", null);
            
            // Late add Vine
            var vineWidget = CreateWidget(_vines[0]);
            _elementManager.Add(vineWidget);
            
            var vineComponent = _scriptFactory.GetVine(_vines[0]);
            
            Assert.AreEqual(0, vineComponent.EnterInvoked);
            vineComponent.FinishConfigure();
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            
            // Late add Behavior
            var behaviorWidget = CreateWidget(_behaviors[0]);
            _elementManager.Add(behaviorWidget);
            
            var behaviorComponent = _scriptFactory.GetBehavior(_behaviors[0]);
            
            Assert.AreEqual(1, behaviorComponent.EnterInvoked);
            
            // Late add Combined
            var mixedWidget = CreateWidget(_behaviors[1], _vines[1]);
            _elementManager.Add(mixedWidget);

            vineComponent = _scriptFactory.GetVine(_vines[1]);
            behaviorComponent = _scriptFactory.GetBehavior(_behaviors[1]);
            
            Assert.AreEqual(0, vineComponent.EnterInvoked);
            Assert.AreEqual(0, behaviorComponent.EnterInvoked);
            vineComponent.FinishConfigure();
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            Assert.AreEqual(1, behaviorComponent.EnterInvoked);
        }

        // Adding a script to an already existing Widget without scripts.
        [Test]
        public void FirstScript()
        {
            var widget1 = CreateWidget();
            var widget2 = CreateWidget();
            var widget3 = CreateWidget();
            
            _elementManager.Add(widget1);
            _elementManager.Add(widget2);
            _elementManager.Add(widget3);
            _scriptService.Start();
            _sceneManager.Initialize("test", null);
            
            // New Vine
            AddScriptToWidget(widget1, _vines[0]);
            
            var vineComponent = _scriptFactory.GetVine(_vines[0]);
            Assert.AreEqual(0, vineComponent.EnterInvoked);
            vineComponent.FinishConfigure();
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            
            // New Behavior
            AddScriptToWidget(widget2, _behaviors[0]);
            
            var behaviourComponent = _scriptFactory.GetBehavior(_behaviors[0]);
            Assert.AreEqual(1, behaviourComponent.EnterInvoked);
            
            // New Combined
            AddScriptToWidget(widget3, _behaviors[1], _vines[1]);
            
            vineComponent = _scriptFactory.GetVine(_vines[1]);
            behaviourComponent = _scriptFactory.GetBehavior(_behaviors[1]);
            
            Assert.AreEqual(0, vineComponent.EnterInvoked);
            Assert.AreEqual(0, behaviourComponent.EnterInvoked);
            vineComponent.FinishConfigure();
            Assert.AreEqual(1, behaviourComponent.EnterInvoked);
            Assert.AreEqual(1, vineComponent.EnterInvoked);
        }
        
        // Adding a script to an already existing Widget containing scripts. 
        [Test]
        public void AdditionalScripts()
        {
            var widget = CreateWidget(_vines[0]);
            _elementManager.Add(widget);
            _scriptService.Start();
            _sceneManager.Initialize("test", null);
            
            var vineComponent = _scriptFactory.GetVine(_vines[0]);
            vineComponent.FinishConfigure();

            // Add a new Behavior
            AddScriptToWidget(widget, _behaviors[0]);

            var behaviourComponent = _scriptFactory.GetBehavior(_behaviors[0]);
            
            Assert.AreEqual(2, vineComponent.EnterInvoked);
            Assert.AreEqual(1, behaviourComponent.EnterInvoked);
        }
        
        
        
        private Widget CreateWidget(params EnkluScript[] scripts)
        {
            var widget = new Widget(new GameObject("ScriptRunner_Tests"), null, null, null);
            AddScriptToWidget(widget, scripts);
            return widget;
        }

        private void AddScriptToWidget(Widget widget, params EnkluScript[] scripts)
        {
            var existingScripts = JArray.Parse(
                widget.Schema.GetOwn("scripts", "[]").Value);

            for (int i = 0, len = scripts.Length; i < len; i++)
            {
                var script = scripts[i];
                
                if (!existingScripts.Contains(script.Data.Id))
                {
                    existingScripts.Add(JToken.FromObject(new Dictionary<string, string>
                    {
                        { "id", script.Data.Id }
                    }));
                
                    _scriptManager.AddEntry(script.Data.Id, script);
                }
            }

            widget.Schema.Set("scripts", JsonConvert.SerializeObject(existingScripts));
        }
    }
}