using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Vine;
using Jint.Parser;
using NUnit.Framework;

// ReSharper disable AccessToModifiedClosure

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class ScriptRunner_Tests
    {
        private ScriptRunner _scriptRunner;

        private TestScriptManager _scriptManager;
        private TestScriptFactory _scriptFactory;
        private readonly EnkluScript[] _behaviors = new EnkluScript[3];
        private readonly EnkluScript[] _vines = new EnkluScript[3];
        
        [SetUp]
        public void Setup()
        {
            _scriptManager = new TestScriptManager();
            _scriptFactory = new TestScriptFactory();
            _scriptRunner = new ScriptRunner(
                _scriptManager, 
                _scriptFactory, 
                new ElementJsCache(new ElementJsFactory(_scriptManager)),
                new ScriptingHostFactory(null, null, new ApplicationConfig()), 
                null);
            
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
        public void SingleWidget_NoScripts()
        {
            var widget = WidgetUtil.CreateWidget();
            
            _scriptRunner.AddSceneRoot(widget);
            _scriptRunner.StartAllScripts()
                .OnSuccess(_ => Assert.Pass())
                .OnFailure(exception => Assert.Fail("Failed to start runner: " + exception));
            
            _scriptRunner.StopAllScripts();
            
            Assert.Fail("Start scripts wasn't synchronous in this test");
        }
        
        [Test]
        public void SingleWidget_Vines()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _vines[0], _vines[1]);
            
            _scriptRunner.AddSceneRoot(widget);
            var token = _scriptRunner.StartAllScripts();

            TestVineMonoBehaviour vine0 = null;
            TestVineMonoBehaviour vine1 = null;
            var cbCalled = 0;
            token
                .OnSuccess(_ =>
                {
                    Assert.NotNull(vine0);
                    Assert.NotNull(vine1);
                    Assert.AreEqual(1, vine0.EnterInvoked);
                    Assert.AreEqual(1, vine1.EnterInvoked);
                    Assert.AreEqual(0, vine0.ExitInvoked);
                    Assert.AreEqual(0, vine1.ExitInvoked);
                    cbCalled++;
                })
                .OnFailure(exception =>
                {
                    Assert.Fail("Failed to start runner: " + exception);
                });
            Assert.AreEqual(0, cbCalled);
            
            vine0 = _scriptFactory.GetVine(widget, _vines[0]);
            vine1 = _scriptFactory.GetVine(widget,_vines[1]);
            
            vine0.FinishConfigure();
            Assert.AreEqual(0, cbCalled);
            
            vine1.FinishConfigure();
            Assert.AreEqual(1, cbCalled);
            
            _scriptRunner.StopAllScripts();
            Assert.AreEqual(1, vine0.ExitInvoked);
            Assert.AreEqual(1, vine1.ExitInvoked);
        }
        
        [Test]
        public void SingleWidget_Behaviors()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0], _behaviors[1]);
            
            _scriptRunner.AddSceneRoot(widget);
            var token = _scriptRunner.StartAllScripts();

            TestBehaviorMonoBehaviour behavior0 = null;
            TestBehaviorMonoBehaviour behavior1 = null;
            var cbCalled = 0;
            token
                .OnSuccess(_ =>
                {
                    behavior0 = _scriptFactory.GetBehavior(widget, _behaviors[0]);
                    behavior1 = _scriptFactory.GetBehavior(widget, _behaviors[1]);
                    
                    Assert.NotNull(behavior0);
                    Assert.NotNull(behavior1);
                    Assert.AreEqual(1, behavior0.EnterInvoked);
                    Assert.AreEqual(1, behavior1.EnterInvoked);
                    Assert.AreEqual(0, behavior0.ExitInvoked);
                    Assert.AreEqual(0, behavior1.ExitInvoked);
                    cbCalled++;
                })
                .OnFailure(exception =>
                {
                    Assert.Fail("Failed to start runner: " + exception);
                });
            
            // Behaviors parse synchronously
            Assert.AreEqual(1, cbCalled);
            
            _scriptRunner.StopAllScripts();
            Assert.AreEqual(1, behavior0.ExitInvoked);
            Assert.AreEqual(1, behavior1.ExitInvoked);
        }

        [Test]
        public void SingleWidget_Mixed()
        {
            // Order matters and will be checked in this test.
            var widget = WidgetUtil.CreateWidget(_scriptManager, _behaviors[2], _vines[1], _behaviors[0], _behaviors[1], _vines[2], _vines[0]);
            
            _scriptRunner.AddSceneRoot(widget);
            var token = _scriptRunner.StartAllScripts();

            TestVineMonoBehaviour vine0 = null;
            TestVineMonoBehaviour vine1 = null;
            TestVineMonoBehaviour vine2 = null;
            TestBehaviorMonoBehaviour behavior0 = null;
            TestBehaviorMonoBehaviour behavior1 = null;
            TestBehaviorMonoBehaviour behavior2 = null;
            var cbCalled = 0;
            token
                .OnSuccess(_ =>
                {
                    Assert.NotNull(vine0);
                    Assert.NotNull(vine1);
                    Assert.NotNull(vine2);
                    Assert.NotNull(behavior0);
                    Assert.NotNull(behavior1);
                    Assert.NotNull(behavior2);
                    Assert.AreEqual(1, vine0.EnterInvoked);
                    Assert.AreEqual(1, vine1.EnterInvoked);
                    Assert.AreEqual(1, vine2.EnterInvoked);
                    Assert.AreEqual(1, behavior0.EnterInvoked);
                    Assert.AreEqual(1, behavior0.EnterInvoked);
                    Assert.AreEqual(1, behavior0.EnterInvoked);
                    
                    Assert.AreEqual(1, behavior0.LastEnterInvokeId);
                    Assert.AreEqual(2, behavior1.LastEnterInvokeId);
                    Assert.AreEqual(0, behavior2.LastEnterInvokeId);
                    cbCalled++;
                })
                .OnFailure(exception => Assert.Fail("Failed to start runner: " + exception));
            Assert.AreEqual(0, cbCalled);

            vine0 = _scriptFactory.GetVine(widget, _vines[0]);
            vine1 = _scriptFactory.GetVine(widget, _vines[1]);
            vine2 = _scriptFactory.GetVine(widget, _vines[2]);
            behavior0 = _scriptFactory.GetBehavior(widget, _behaviors[0]);
            behavior1 = _scriptFactory.GetBehavior(widget, _behaviors[1]);
            behavior2 = _scriptFactory.GetBehavior(widget, _behaviors[2]);

            vine0.FinishConfigure();
            vine1.FinishConfigure();
            vine2.FinishConfigure();
            Assert.AreEqual(1, cbCalled);
            
            _scriptRunner.StopAllScripts();
            Assert.AreEqual(1, vine0.ExitInvoked);
            Assert.AreEqual(1, vine1.ExitInvoked);
            Assert.AreEqual(1, vine2.ExitInvoked);
            Assert.AreEqual(1, behavior0.ExitInvoked);
            Assert.AreEqual(1, behavior0.ExitInvoked);
            Assert.AreEqual(1, behavior0.ExitInvoked);
            Assert.AreEqual(1, behavior0.LastExitInvokeId);
            Assert.AreEqual(2, behavior1.LastExitInvokeId);
            Assert.AreEqual(0, behavior2.LastExitInvokeId);
        }

        [Test]
        public void MultipleWidgets()
        {
            var widgetA = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0]);
            var widgetB = WidgetUtil.CreateWidget(_scriptManager, _vines[0]);
            
            widgetA.AddChild(widgetB);
            
            _scriptRunner.AddSceneRoot(widgetA);

            TestVineMonoBehaviour vine = null;
            TestBehaviorMonoBehaviour behavior = null;

            var cbCalled = 0;
            _scriptRunner.StartAllScripts()
                .OnSuccess(_ =>
                {
                    Assert.NotNull(vine);
                    Assert.NotNull(behavior);
                    Assert.AreEqual(1, vine.EnterInvoked);
                    Assert.AreEqual(1, behavior.EnterInvoked);
                    
                    cbCalled++;
                })
                .OnFailure(exception => Assert.Fail("Failed to start runner: " + exception));
            
            Assert.AreEqual(0, cbCalled);

            // Ensure script hasn't been run until vine finishes.
            behavior = _scriptFactory.GetBehavior(widgetA, _behaviors[0]);
            Assert.AreEqual(0, behavior.EnterInvoked);
            
            vine = _scriptFactory.GetVine(widgetB, _vines[0]);
            vine.FinishConfigure();
            
            Assert.AreEqual(1, cbCalled);
            
            _scriptRunner.StopAllScripts();
            Assert.AreEqual(1, vine.ExitInvoked);
            Assert.AreEqual(1, behavior.ExitInvoked);
            Assert.AreEqual(0, behavior.LastExitInvokeId);
        }
        
        #endregion
        
        #region Updating Scripts

        [Test]
        public void Update_Vine()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _vines[0], _behaviors[0]);
            
            _scriptRunner.AddSceneRoot(widget);
            var token = _scriptRunner.StartAllScripts();

            TestVineMonoBehaviour initialVine = null;
            TestBehaviorMonoBehaviour behavior = null;
            var cbCalled = 0;
            token
                .OnSuccess(_ =>
                {
                    Assert.NotNull(initialVine);
                    Assert.NotNull(behavior);
                    Assert.AreEqual(1, initialVine.EnterInvoked);
                    Assert.AreEqual(1, behavior.EnterInvoked);
                    Assert.AreEqual(0, initialVine.ExitInvoked);
                    Assert.AreEqual(0, behavior.ExitInvoked);
                    cbCalled++;
                })
                .OnFailure(exception =>
                {
                    Assert.Fail("Failed to start runner: " + exception);
                });
            Assert.AreEqual(0, cbCalled);
            
            initialVine = _scriptFactory.GetVine(widget, _vines[0]);
            behavior = _scriptFactory.GetBehavior(widget, _behaviors[0]);
            
            initialVine.FinishConfigure();
            Assert.AreEqual(1, cbCalled);
            
            // Update, check to make sure nothing exits before the new one is ready.
            // Ensure the behavior also exits/enters with the same instance.
            initialVine.EnkluScript.Updated();
            var updatedVine = _scriptFactory.GetVine(widget, _vines[0]);
            Assert.AreNotSame(initialVine, updatedVine);
            Assert.AreEqual(0, updatedVine.EnterInvoked);
            Assert.AreEqual(0, initialVine.ExitInvoked);
            Assert.AreEqual(0, behavior.ExitInvoked);
            
            updatedVine.FinishConfigure();
            Assert.AreEqual(1, initialVine.ExitInvoked);
            Assert.AreEqual(1, behavior.ExitInvoked);
            Assert.AreEqual(1, updatedVine.EnterInvoked);
            Assert.AreEqual(2, behavior.EnterInvoked);
            
            _scriptRunner.StopAllScripts();
            Assert.AreEqual(1, initialVine.ExitInvoked);
            Assert.AreEqual(1, updatedVine.ExitInvoked);
            Assert.AreEqual(2, behavior.ExitInvoked);
        }

        [Test]
        public void Update_Behavior()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _vines[0], _behaviors[0]);
            
            _scriptRunner.AddSceneRoot(widget);
            var token = _scriptRunner.StartAllScripts();

            TestVineMonoBehaviour vine = null;
            TestBehaviorMonoBehaviour initialBehavior = null;
            var cbCalled = 0;
            token
                .OnSuccess(_ =>
                {
                    Assert.NotNull(vine);
                    Assert.NotNull(initialBehavior);
                    Assert.AreEqual(1, vine.EnterInvoked);
                    Assert.AreEqual(1, initialBehavior.EnterInvoked);
                    Assert.AreEqual(0, vine.ExitInvoked);
                    Assert.AreEqual(0, initialBehavior.ExitInvoked);
                    cbCalled++;
                })
                .OnFailure(exception =>
                {
                    Assert.Fail("Failed to start runner: " + exception);
                });
            Assert.AreEqual(0, cbCalled);
            
            vine = _scriptFactory.GetVine(widget, _vines[0]);
            initialBehavior = _scriptFactory.GetBehavior(widget, _behaviors[0]);
            
            vine.FinishConfigure();
            Assert.AreEqual(1, cbCalled);
            
            // Update, ensure the old component exits and the new component is entered.
            // Ensure the behavior also exits/enters with the same instance.
            vine.EnkluScript.Updated();
            var updatedVine = _scriptFactory.GetVine(widget, _vines[0]);
            Assert.AreEqual(0, updatedVine.EnterInvoked);
            Assert.AreEqual(0, vine.ExitInvoked);
            Assert.AreEqual(0, initialBehavior.ExitInvoked);
            
            updatedVine.FinishConfigure();
            Assert.AreEqual(1, vine.ExitInvoked);
            Assert.AreEqual(1, initialBehavior.ExitInvoked);
            Assert.AreEqual(1, updatedVine.EnterInvoked);
            Assert.AreEqual(2, initialBehavior.EnterInvoked);
            
            _scriptRunner.StopAllScripts();
            Assert.AreEqual(1, vine.ExitInvoked);
            Assert.AreEqual(1, updatedVine.ExitInvoked);
            Assert.AreEqual(2, initialBehavior.ExitInvoked);
        }
        
        #endregion
        
        #region Hierarchy

        [Test]
        public void MixedElements()
        {
            // Ensure all widgets are found, even with a boring Element in between.
            var widgetA = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0]);
            var elementB = new Element();
            var widgetC = WidgetUtil.CreateWidget(_scriptManager, _behaviors[1]);
            var widgetD = WidgetUtil.CreateWidget(_scriptManager, _behaviors[2]);
            
            widgetC.AddChild(widgetD);
            elementB.AddChild(widgetC);
            widgetA.AddChild(elementB);
            
            _scriptRunner.AddSceneRoot(widgetA);

            TestBehaviorMonoBehaviour behavior0 = null;
            TestBehaviorMonoBehaviour behavior1 = null;
            TestBehaviorMonoBehaviour behavior2 = null;
            var cbCalled = 0;
            _scriptRunner.StartAllScripts()
                .OnSuccess(_ =>
                {
                    behavior0 = _scriptFactory.GetBehavior(widgetA, _behaviors[0]);
                    behavior1 = _scriptFactory.GetBehavior(widgetC, _behaviors[1]);
                    behavior2 = _scriptFactory.GetBehavior(widgetD, _behaviors[2]);
                    
                    Assert.NotNull(behavior0);
                    Assert.NotNull(behavior1);
                    Assert.NotNull(behavior2);
                    
                    Assert.AreEqual(1, behavior0.EnterInvoked);
                    Assert.AreEqual(1, behavior1.EnterInvoked);
                    Assert.AreEqual(1, behavior2.EnterInvoked);
                    Assert.AreEqual(0, behavior0.LastEnterInvokeId);
                    Assert.AreEqual(1, behavior1.LastEnterInvokeId);
                    Assert.AreEqual(2, behavior2.LastEnterInvokeId);
                    cbCalled++;
                })
                .OnFailure(exception => Assert.Fail("Failed to start runner: " + exception));
            Assert.AreEqual(1, cbCalled);
            
            _scriptRunner.StopAllScripts();
            
            Assert.AreEqual(1, behavior0.ExitInvoked);
            Assert.AreEqual(1, behavior1.ExitInvoked);
            Assert.AreEqual(1, behavior2.ExitInvoked);
            Assert.AreEqual(0, behavior0.LastExitInvokeId);
            Assert.AreEqual(1, behavior1.LastExitInvokeId);
            Assert.AreEqual(2, behavior2.LastExitInvokeId);
        }
        
        [Test]
        public void Reparenting()
        {
            
        }
        
        #endregion

        #region Misc
        
        [Test]
        public void ScriptSharing()
        {
            var widgetA = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0]);
            var widgetB = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0]);
            
            widgetA.AddChild(widgetB);
            
            _scriptRunner.AddSceneRoot(widgetA);

            TestBehaviorMonoBehaviour aBehavior = null;
            TestBehaviorMonoBehaviour bBehavior = null;
            var cbCalled = 0;
            _scriptRunner.StartAllScripts()
                .OnSuccess(_ =>
                {
                    aBehavior = _scriptFactory.GetBehavior(widgetA, _behaviors[0]);
                    bBehavior = _scriptFactory.GetBehavior(widgetB, _behaviors[0]);
                    
                    Assert.NotNull(aBehavior);
                    Assert.NotNull(bBehavior);
                    Assert.AreNotEqual(aBehavior, bBehavior);
                    
                    Assert.AreEqual(1, aBehavior.EnterInvoked);
                    Assert.AreEqual(1, bBehavior.EnterInvoked);
                    Assert.AreEqual(0, aBehavior.LastEnterInvokeId);
                    Assert.AreEqual(1, bBehavior.LastEnterInvokeId);

                    cbCalled++;
                })
                .OnFailure(exception => Assert.Fail("Failed to start runner: " + exception));
                
           Assert.AreEqual(cbCalled, 1);
           
           _scriptRunner.StopAllScripts();
           Assert.AreEqual(1, aBehavior.ExitInvoked);
           Assert.AreEqual(1, bBehavior.ExitInvoked);
           Assert.AreEqual(0, aBehavior.LastExitInvokeId);
           Assert.AreEqual(1, bBehavior.LastExitInvokeId);
        }
        
        #endregion
    }
}