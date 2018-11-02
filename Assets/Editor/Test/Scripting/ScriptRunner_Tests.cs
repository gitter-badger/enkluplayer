using System;
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
    public class ScriptRunner_Tests
    {
        private ScriptRunner _scriptRunner;

        private TestScriptManager _scriptManager;
        private TestScriptFactory _scriptFactory;
        private EnkluScript[] _behaviors = new EnkluScript[3];
        private EnkluScript[] _vines = new EnkluScript[3];
        
        [SetUp]
        public void Setup()
        {
            TestBehaviorMonoBehaviour.ResetInvokeIds();
            
            _scriptManager = new TestScriptManager();
            _scriptFactory = new TestScriptFactory();
            _scriptRunner = new ScriptRunner(
                _scriptManager, _scriptFactory, null, new ElementJsCache(null));
            
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

        [Test]
        public void Behavior()
        {
            var widget = CreateWidget(_behaviors[0]);
            
            _scriptRunner.AddWidget(widget);
            _scriptRunner.Parse();
            
            // Behaviors load synchronously. Check that the script has loaded & invoked.
            Assert.AreEqual(ScriptRunner.SetupState.Done, _scriptRunner.GetSetupState(widget));
            
            var component = _scriptFactory.GetBehavior(_behaviors[0]);
            Assert.AreEqual(1, component.EnterInvoked);
        }

        [Test]
        public void Vine()
        {
            var widget = CreateWidget(_vines[0]);
            
            _scriptRunner.AddWidget(widget);
            _scriptRunner.Parse();
            
            // Vines are async, check that it hasn't finished.
            Assert.AreEqual(ScriptRunner.SetupState.Parsing, _scriptRunner.GetSetupState(widget));
            
            var component = _scriptFactory.GetVine(_vines[0]);
            
            Assert.AreEqual(0, component.EnterInvoked);
            
            // Resolve pending finish, and check for loaded & invoked.
            component.FinishConfigure();
            Assert.AreEqual(ScriptRunner.SetupState.Done, _scriptRunner.GetSetupState(widget));
            Assert.AreEqual(1, component.EnterInvoked);
        }

        [Test]
        public void Combined()
        {
            // Test both scripts together. Load them in a random-ish order
            var widget = CreateWidget(
                _behaviors[2], _vines[1], _behaviors[0], _behaviors[1], _vines[2], _vines[0]);
            
            _scriptRunner.AddWidget(widget);
            _scriptRunner.Parse();
            
            Assert.AreEqual(ScriptRunner.SetupState.Parsing, _scriptRunner.GetSetupState(widget));
            
            var vineComponents = new TestVineMonoBehaviour[_vines.Length];
            for (var i = 0; i < vineComponents.Length; i++)
            {
                vineComponents[i] = _scriptFactory.GetVine(_vines[i]);
                
                // Ensure vine is loading
                Assert.AreEqual(0, vineComponents[i].EnterInvoked);
            }
            
            var behaviorComponents = new TestBehaviorMonoBehaviour[_behaviors.Length];
            for (var i = 0; i < behaviorComponents.Length; i++)
            {
                behaviorComponents[i] = _scriptFactory.GetBehavior(_behaviors[i]);
                
                // Behaviors should exist
                Assert.AreEqual(0, behaviorComponents[i].EnterInvoked);
            }
            
            // Helper to check Vine enter invocation counts
            Action<int[], int> checkVineInvokes = (vineInvokes, behaviorInvokes) =>
            {
                Assert.AreEqual(vineInvokes[0], vineComponents[0].EnterInvoked);
                Assert.AreEqual(vineInvokes[1], vineComponents[1].EnterInvoked);
                Assert.AreEqual(vineInvokes[2], vineComponents[2].EnterInvoked);
                Assert.AreEqual(behaviorInvokes, behaviorComponents[0].EnterInvoked);
                Assert.AreEqual(behaviorInvokes, behaviorComponents[1].EnterInvoked);
                Assert.AreEqual(behaviorInvokes, behaviorComponents[2].EnterInvoked);
            };
            
            // Start resolving Vines. Currently, they run in the order they finish parsing
            vineComponents[1].FinishConfigure();
            checkVineInvokes(new[] {0, 1, 0}, 0);
            
            vineComponents[2].FinishConfigure();
            checkVineInvokes(new[] {0, 1, 1}, 0);
            
            // The final vine resolve will trigger behaviors
            vineComponents[0].FinishConfigure();
            checkVineInvokes(new[] {1, 1, 1}, 1);
            
            // Check behavior invoke order, should match script list order
            Assert.AreEqual(0, behaviorComponents[2].LastInvokeId);
            Assert.AreEqual(1, behaviorComponents[0].LastInvokeId);
            Assert.AreEqual(2, behaviorComponents[1].LastInvokeId);
            
            Assert.AreEqual(ScriptRunner.SetupState.Done, _scriptRunner.GetSetupState(widget));
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