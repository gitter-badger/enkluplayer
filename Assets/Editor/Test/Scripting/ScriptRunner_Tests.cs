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

        private TestScriptManager _testManager;
        private TestScriptFactory _testFactory;
        private EnkluScript[] _behaviors = new EnkluScript[3];
        private EnkluScript[] _vines = new EnkluScript[3];
        
        [SetUp]
        public void Setup()
        {
            _testManager = new TestScriptManager();
            _testFactory = new TestScriptFactory();
            _scriptRunner = new ScriptRunner(
                _testManager, _testFactory, null, new ElementJsCache(null));
            
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
            var widget = WidgetUtil.CreateWidget(_testManager, _behaviors[0]);
            
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartAllScripts();
            
            // Behaviors load synchronously. Check that the script has loaded & invoked.
            Assert.AreEqual(ScriptRunner.SetupState.Done, _scriptRunner.GetSetupState(widget));
            
            var component = _testFactory.GetBehavior(_behaviors[0]);
            Assert.AreEqual(1, component.EnterInvoked);
        }

        [Test]
        public void Vine()
        {
            var widget = WidgetUtil.CreateWidget(_testManager, _vines[0]);
            
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartAllScripts();
            
            // Vines are async, check that it hasn't finished.
            Assert.AreEqual(ScriptRunner.SetupState.Parsing, _scriptRunner.GetSetupState(widget));
            
            var component = _testFactory.GetVine(_vines[0]);
            
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
            var widget = WidgetUtil.CreateWidget(_testManager,
                _behaviors[2], _vines[1], _behaviors[0], _behaviors[1], _vines[2], _vines[0]);
            
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartAllScripts();
            
            Assert.AreEqual(ScriptRunner.SetupState.Parsing, _scriptRunner.GetSetupState(widget));
            
            var vineComponents = new TestVineMonoBehaviour[_vines.Length];
            for (var i = 0; i < vineComponents.Length; i++)
            {
                vineComponents[i] = _testFactory.GetVine(_vines[i]);
                
                // Ensure vine is loading
                Assert.AreEqual(0, vineComponents[i].EnterInvoked);
            }
            
            var behaviorComponents = new TestBehaviorMonoBehaviour[_behaviors.Length];
            for (var i = 0; i < behaviorComponents.Length; i++)
            {
                behaviorComponents[i] = _testFactory.GetBehavior(_behaviors[i]);
                
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
            _scriptRunner.StartAllScripts();
            
            WidgetUtil.AddScriptToWidget(widget, _testManager, _vines[0]);
            
            var vineComponent = _testFactory.GetVine(_vines[0]);
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
            _scriptRunner.StartAllScripts();
            
            WidgetUtil.AddScriptToWidget(widget, _testManager, _behaviors[0]);
            
            var behaviourComponent = _testFactory.GetBehavior(_behaviors[0]);
            Assert.AreEqual(1, behaviourComponent.EnterInvoked);
        }
        
        [Test]
        public void FirstScriptCombined()
        {
            var widget = WidgetUtil.CreateWidget();
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartAllScripts();
            
            // New Combined
            WidgetUtil.AddScriptToWidget(widget, _testManager, _behaviors[1], _vines[1]);
            
            var vineComponent = _testFactory.GetVine(_vines[1]);
            var behaviourComponent = _testFactory.GetBehavior(_behaviors[1]);
            
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
            var widget = WidgetUtil.CreateWidget(_testManager, _vines[0]);
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartAllScripts();
            
            var vineComponent = _testFactory.GetVine(_vines[0]);
            vineComponent.FinishConfigure();

            Assert.AreEqual(1, vineComponent.EnterInvoked);
            
            // Add a new Behavior
            WidgetUtil.AddScriptToWidget(widget, _testManager, _behaviors[0]);
            
            // Ensure existing behavior exited
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            Assert.AreEqual(1, vineComponent.ExitInvoked);
            
            // Ensure new behavior invoked
            vineComponent = _testFactory.GetVine(_vines[0]);
            vineComponent.FinishConfigure();
            
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            Assert.AreEqual(0, vineComponent.ExitInvoked);
            
            var behaviourComponent = _testFactory.GetBehavior(_behaviors[0]);
            Assert.AreEqual(1, behaviourComponent.EnterInvoked);
        }
        #endregion

        #region Script Updates
        [Test]
        public void UpdatingVine()
        {
            var widget = WidgetUtil.CreateWidget(_testManager, _vines[0]);
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartAllScripts();
            
            var oldComponent = _testFactory.GetVine(_vines[0]);
            oldComponent.FinishConfigure();
            Assert.AreEqual(1, oldComponent.EnterInvoked);
            Assert.AreEqual(0, oldComponent.ExitInvoked);

            var vineScript = _testManager.Create(oldComponent.EnkluScript.Data.Id);
            
            // Old vine won't exit until new one has finished configuring
            vineScript.Updated();
            Assert.AreEqual(1, oldComponent.EnterInvoked);
            Assert.AreEqual(0, oldComponent.ExitInvoked);
            
            var newComponent = _testFactory.GetVine(_vines[0]);
            newComponent.FinishConfigure();
            
            Assert.AreEqual(1, oldComponent.EnterInvoked);
            Assert.AreEqual(1, oldComponent.ExitInvoked);
            
            Assert.AreNotSame(oldComponent, newComponent);
            Assert.AreEqual(1, newComponent.EnterInvoked);
            Assert.AreEqual(0, newComponent.ExitInvoked);
        }

        [Test]
        public void UpdatingBehavior()
        {
            var widget = WidgetUtil.CreateWidget(_testManager, _behaviors[0]);
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartAllScripts();
            
            var oldComponent = _testFactory.GetBehavior(_behaviors[0]);
            Assert.AreEqual(1, oldComponent.EnterInvoked);
            Assert.AreEqual(0, oldComponent.ExitInvoked);

            var behaviorScript = _testManager.Create(oldComponent.EnkluScript.Data.Id);
            
            // Old behavior exits immediately since parsing is synchronous
            behaviorScript.Updated();
            Assert.AreEqual(1, oldComponent.EnterInvoked);
            Assert.AreEqual(1, oldComponent.ExitInvoked);
            
            var newComponent = _testFactory.GetBehavior(_behaviors[0]);
            
            Assert.AreNotSame(oldComponent, newComponent);
            Assert.AreEqual(1, newComponent.EnterInvoked);
            Assert.AreEqual(0, newComponent.ExitInvoked);
        }

        [Test]
        public void UpdatingStack()
        {
            // Test to make sure updating one script causes all of the scripts on an element to update
            var widget = WidgetUtil.CreateWidget(_testManager, _vines[0], _behaviors[0]);
            _scriptRunner.AddWidget(widget);
            _scriptRunner.ParseAll();
            _scriptRunner.StartAllScripts();
            
            var oldVineComponent = _testFactory.GetVine(_vines[0]);
            var oldBehaviorComponent = _testFactory.GetBehavior(_behaviors[0]);
            oldVineComponent.FinishConfigure();
            Assert.AreEqual(1, oldVineComponent.EnterInvoked);
            Assert.AreEqual(0, oldVineComponent.ExitInvoked);
            Assert.AreEqual(1, oldBehaviorComponent.EnterInvoked);
            Assert.AreEqual(0, oldBehaviorComponent.ExitInvoked);

            var vineScript = _testManager.Create(oldVineComponent.EnkluScript.Data.Id);
            
            // Old vine won't exit until new one has finished configuring. Behavior waits for vine.
            vineScript.Updated();
            Assert.AreEqual(1, oldVineComponent.EnterInvoked);
            Assert.AreEqual(0, oldVineComponent.ExitInvoked);
            Assert.AreEqual(1, oldBehaviorComponent.EnterInvoked);
            Assert.AreEqual(0, oldBehaviorComponent.ExitInvoked);
            
            var newVineComponent = _testFactory.GetVine(_vines[0]);
            
            newVineComponent.FinishConfigure();
            
            Assert.AreEqual(1, oldVineComponent.EnterInvoked);
            Assert.AreEqual(1, oldVineComponent.ExitInvoked);
            Assert.AreEqual(2, oldBehaviorComponent.EnterInvoked);
            Assert.AreEqual(1, oldBehaviorComponent.ExitInvoked);
            
            Assert.AreNotSame(oldVineComponent, newVineComponent);
            Assert.AreEqual(1, newVineComponent.EnterInvoked);
            Assert.AreEqual(0, newVineComponent.ExitInvoked);
        }
        #endregion
    }
}