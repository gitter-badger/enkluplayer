using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Test.UI;
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

        /// <summary>
        /// Tests a scene loading from scratch.
        /// </summary>
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

        /// <summary>
        /// Tests a script being added to an already added scene.
        /// </summary>
        [Test]
        public void NewScript()
        {
            _scriptService.Start();
            _sceneManager.Initialize("test", null);
            
            var behaviorWidget = CreateWidget(_behaviors[0]);
            _elementManager.Add(behaviorWidget);
            
            var vineWidget = CreateWidget(_vines[0]);
            _elementManager.Add(vineWidget);
            
            var vineComponent = _scriptFactory.GetVine(_vines[0]);
            var behaviorComponent = _scriptFactory.GetBehavior(_behaviors[0]);
            
            Assert.AreEqual(0, vineComponent.EnterInvoked);
            Assert.AreEqual(0, behaviorComponent.EnterInvoked);
            
            vineComponent.FinishConfigure();
            
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            Assert.AreEqual(1, behaviorComponent.EnterInvoked);
        }
        
        private Widget CreateWidget(params EnkluScript[] scripts)
        {
            var widget = new Widget(new GameObject("ScriptRunner_Tests"), null, null, null);
            for (var i = 0; i < scripts.Length; i++)
            {
                AddScriptToWidget(scripts[i], widget);
            }

            return widget;
        }

        private void AddScriptToWidget(EnkluScript script, Widget widget)
        {
            var existingScripts = JArray.Parse(
                widget.Schema.GetOwn("scripts", "[]").Value);

            if (!existingScripts.Contains(script.Data.Id))
            {
                existingScripts.Add(JToken.FromObject(new Dictionary<string, string>
                {
                    { "id", script.Data.Id }
                }));
                
                _scriptManager.AddEntry(script.Data.Id, script);
            }
            
            widget.Schema.Set("scripts", JsonConvert.SerializeObject(existingScripts));
        }
    }
}