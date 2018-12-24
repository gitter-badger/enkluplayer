using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Vine;
using Jint.Parser;
using NUnit.Framework;

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

        [TearDown]
        public void Teardown()
        {
            TestBehaviorMonoBehaviour.ResetInvokeIds();
        }

        #region Existing Scripts
        [Test]
        public void Behavior()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0]);
            
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartScripts();
            
            // Behaviors load synchronously. Check that the script has loaded & invoked.
            Assert.AreEqual(ScriptRunner.SetupState.Done, _scriptRunner.GetSetupState(widget));
            
            var component = _scriptFactory.GetBehavior(_behaviors[0]);
            Assert.AreEqual(1, component.EnterInvoked);
        }

        [Test]
        public void Vine()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _vines[0]);
            
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartScripts();
            
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
            var widget = WidgetUtil.CreateWidget(_scriptManager,
                _behaviors[2], _vines[1], _behaviors[0], _behaviors[1], _vines[2], _vines[0]);
            
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartScripts();
            
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
            Log.Info(this, "{0} {1} {2}", behaviorComponents[2].LastEnterInvokeId, behaviorComponents[1].LastEnterInvokeId, behaviorComponents[0].LastEnterInvokeId);
            Assert.AreEqual(0, behaviorComponents[2].LastEnterInvokeId);
            Assert.AreEqual(1, behaviorComponents[0].LastEnterInvokeId);
            Assert.AreEqual(2, behaviorComponents[1].LastEnterInvokeId);
            
            Assert.AreEqual(ScriptRunner.SetupState.Done, _scriptRunner.GetSetupState(widget));
        }
        #endregion
        
        #region New Scripts
        // Adding a script to an already existing Widget without scripts.

        [Test]
        public void FirstScriptVine()
        {
            var widget = WidgetUtil.CreateWidget();
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartScripts();
            
            WidgetUtil.AddScriptToWidget(widget, _scriptManager, _vines[0]);
            
            var vineComponent = _scriptFactory.GetVine(_vines[0]);
            Assert.AreEqual(0, vineComponent.EnterInvoked);
            vineComponent.FinishConfigure();
            Assert.AreEqual(1, vineComponent.EnterInvoked);
        }

        [Test]
        public void FirstScriptBehavior()
        {
            var widget = WidgetUtil.CreateWidget();
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartScripts();
            
            WidgetUtil.AddScriptToWidget(widget, _scriptManager, _behaviors[0]);
            
            var behaviourComponent = _scriptFactory.GetBehavior(_behaviors[0]);
            Assert.AreEqual(1, behaviourComponent.EnterInvoked);
        }
        
        [Test]
        public void FirstScriptCombined()
        {
            var widget = WidgetUtil.CreateWidget();
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartScripts();
            
            // New Combined
            WidgetUtil.AddScriptToWidget(widget, _scriptManager, _behaviors[1], _vines[1]);
            
            var vineComponent = _scriptFactory.GetVine(_vines[1]);
            var behaviourComponent = _scriptFactory.GetBehavior(_behaviors[1]);
            
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
            var widget = WidgetUtil.CreateWidget(_scriptManager, _vines[0]);
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartScripts();
            
            var vineComponent = _scriptFactory.GetVine(_vines[0]);
            vineComponent.FinishConfigure();

            Assert.AreEqual(1, vineComponent.EnterInvoked);
            
            // Add a new Behavior
            WidgetUtil.AddScriptToWidget(widget, _scriptManager, _behaviors[0]);
            
            // Ensure existing behavior exited
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            Assert.AreEqual(1, vineComponent.ExitInvoked);
            
            // Ensure new behavior invoked
            vineComponent = _scriptFactory.GetVine(_vines[0]);
            vineComponent.FinishConfigure();
            
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            Assert.AreEqual(0, vineComponent.ExitInvoked);
            
            var behaviourComponent = _scriptFactory.GetBehavior(_behaviors[0]);
            Assert.AreEqual(1, behaviourComponent.EnterInvoked);
        }
        #endregion

        #region Script Updates
        [Test]
        public void UpdatingVine()
        {
            // Vines
            var widget = WidgetUtil.CreateWidget(_scriptManager, _vines[0]);
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartScripts();
            
            var oldComponent = _scriptFactory.GetVine(_vines[0]);
            oldComponent.FinishConfigure();
            Assert.AreEqual(1, oldComponent.EnterInvoked);
            Assert.AreEqual(0, oldComponent.ExitInvoked);

            var vineScript = _scriptManager.Create(oldComponent.EnkluScript.Data.Id);
            
            vineScript.Updated();
            Assert.AreEqual(1, oldComponent.EnterInvoked);
            Assert.AreEqual(0, oldComponent.ExitInvoked);
            
            var newComponent = _scriptFactory.GetVine(_vines[0]);
            newComponent.FinishConfigure();
            
            Assert.AreEqual(1, oldComponent.EnterInvoked);
            Assert.AreEqual(1, oldComponent.ExitInvoked);
            
            
            Assert.AreNotSame(oldComponent, newComponent);
            Assert.AreEqual(1, newComponent.EnterInvoked);
            Assert.AreEqual(0, newComponent.ExitInvoked);
        }
        #endregion
    }
}