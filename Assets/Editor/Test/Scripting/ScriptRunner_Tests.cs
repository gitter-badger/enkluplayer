using System;
using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Vine;
using Jint.Parser;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class ScriptRunner_Tests
    {
        private ScriptRunner _scriptRunner;
        
        private TestScriptFactory _scriptFactory;
        private EnkluScript[] _behaviors = new EnkluScript[3];
        private EnkluScript[] _vines = new EnkluScript[3];
        
        [SetUp]
        public void Setup()
        {
            _scriptRunner = new ScriptRunner(new TestScriptManager());
            
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
            
            // Behaviors load synchronously. Check that the script has loaded & invoked.
            _scriptRunner.ParseSync();
            Assert.AreEqual(ScriptRunner.SetupState.Done, _scriptRunner.GetSetupState(widget));
            
            var component = _scriptFactory.GetBehavior(_behaviors[0]);
            Assert.AreEqual(1, component.EnterInvoked);
        }

        [Test]
        public void Vine()
        {
            var widget = CreateWidget(_vines[0]);
            
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
            
            Assert.AreEqual(ScriptRunner.SetupState.Parsing, _scriptRunner.GetSetupState(widget));
            
            var vineComponents = new TestVineMonoBehaviour[_vines.Length];
            for (var i = 0; i < vineComponents.Length; i++)
            {
                vineComponents[i] = _scriptFactory.GetVine(_vines[i]);
                
                // Ensure vine is loading
                Assert.AreEqual(0, vineComponents[i].EnterInvoked);
            }
            
            // Helper to check Vine enter invocation counts
            Action<int[], bool> checkVineInvokes = (vineInvokes, behaviorsNull) =>
            {
                Assert.AreEqual(vineInvokes[0], vineComponents[0].EnterInvoked);
                Assert.AreEqual(vineInvokes[1], vineComponents[1].EnterInvoked);
                Assert.AreEqual(vineInvokes[2], vineComponents[2].EnterInvoked);
                Assert.AreEqual(behaviorsNull, _scriptFactory.GetBehavior(_behaviors[0]) == null);
                Assert.AreEqual(behaviorsNull, _scriptFactory.GetBehavior(_behaviors[1]) == null);
                Assert.AreEqual(behaviorsNull, _scriptFactory.GetBehavior(_behaviors[2]) == null);
            };
            
            // Start resolving Vines. Currently, they run in the order they finish parsing
            vineComponents[1].FinishConfigure();
            checkVineInvokes(new[] {0, 1, 0}, true);
            
            vineComponents[2].FinishConfigure();
            checkVineInvokes(new[] {0, 1, 1}, true);
            
            // The final vine resolve will trigger behaviors
            vineComponents[0].FinishConfigure();
            checkVineInvokes(new[] {1, 1, 1}, false);
            
            // Check Behavior order
            var behaviorComponents = new TestBehaviorMonoBehaviour[_behaviors.Length];
            for (var i = 0; i < behaviorComponents.Length; i++)
            {
                behaviorComponents[i] = _scriptFactory.GetBehavior(_behaviors[i]);
                
                // Behaviors should exist and be invoked now
                Assert.AreEqual(1, behaviorComponents[i].EnterInvoked);
            }
            
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
                existingScripts.Add(script.Data.Id);
            }
            
            widget.Schema.Set("scripts", existingScripts);
        }
    }
}