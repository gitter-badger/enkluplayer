using System;
using CreateAR.Enkluplayer.Test;
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
                new TestScriptExecutorFactory());
            
            var parser = new DefaultScriptParser(
                null, new JsVinePreProcessor());
            
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
            TestBehaviorScript.ResetInvokeIds();
        }
        
        #region State Machine

        [Test]
        public void MultipleSceneRoots()
        {
            var element = ElementUtil.CreateElement();
            _scriptRunner.AddSceneRoot(element);
            Assert.Throws<Exception>(() => _scriptRunner.AddSceneRoot(element));
        }
        
        [Test]
        public void MultipleStart()
        {
            var cbCalled = 0;
            var element = ElementUtil.CreateElement();
            _scriptRunner.AddSceneRoot(element);
            _scriptRunner.StartRunner()
                .OnSuccess(_ =>
                {
                    cbCalled++;
                    Assert.Throws<Exception>(() => _scriptRunner.StartRunner());
                })
                .OnFailure(exception => Assert.Fail("Failed to start runner: " + exception));
            Assert.AreEqual(1, cbCalled);
        }
        
        
        #endregion
        
        #region Existing Scripts

        [Test]
        public void SingleWidget_NoScripts()
        {
            var widget = ElementUtil.CreateElement();
            
            _scriptRunner.AddSceneRoot(widget);
            _scriptRunner.StartRunner()
                .OnSuccess(_ => Assert.Pass())
                .OnFailure(exception => Assert.Fail("Failed to start runner: " + exception));
            
            _scriptRunner.StopRunner();
            
            Assert.Fail("Start scripts wasn't synchronous in this test");
        }
        
        [Test]
        public void SingleWidget_Vines()
        {
            var widget = ElementUtil.CreateElement(_scriptManager, _vines[0], _vines[1]);
            
            _scriptRunner.AddSceneRoot(widget);

            TestVineScript vine0 = null;
            TestVineScript vine1 = null;
            var cbCalled = 0;
            _scriptRunner.StartRunner()
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
            
            _scriptRunner.StopRunner();
            Assert.AreEqual(1, vine0.ExitInvoked);
            Assert.AreEqual(1, vine1.ExitInvoked);
        }
        
        [Test]
        public void SingleWidget_Behaviors()
        {
            var widget = ElementUtil.CreateElement(_scriptManager, _behaviors[0], _behaviors[1]);
            
            _scriptRunner.AddSceneRoot(widget);
            var token = _scriptRunner.StartRunner();

            TestBehaviorScript behavior0 = null;
            TestBehaviorScript behavior1 = null;
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
            
            _scriptRunner.StopRunner();
            Assert.AreEqual(1, behavior0.ExitInvoked);
            Assert.AreEqual(1, behavior1.ExitInvoked);
        }

        [Test]
        public void SingleWidget_Mixed()
        {
            // Order matters and will be checked in this test.
            var widget = ElementUtil.CreateElement(_scriptManager, _behaviors[2], _vines[1], _behaviors[0], _behaviors[1], _vines[2], _vines[0]);
            
            _scriptRunner.AddSceneRoot(widget);
            var token = _scriptRunner.StartRunner();

            TestVineScript vine0 = null;
            TestVineScript vine1 = null;
            TestVineScript vine2 = null;
            TestBehaviorScript behavior0 = null;
            TestBehaviorScript behavior1 = null;
            TestBehaviorScript behavior2 = null;
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
            
            _scriptRunner.StopRunner();
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
            var elementA = ElementUtil.CreateElement(_scriptManager, _behaviors[0]);
            var elementB = ElementUtil.CreateElement(_scriptManager, _vines[0]);
            
            elementA.AddChild(elementB);
            
            _scriptRunner.AddSceneRoot(elementA);

            TestVineScript vine = null;
            TestBehaviorScript behavior = null;

            var cbCalled = 0;
            _scriptRunner.StartRunner()
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
            behavior = _scriptFactory.GetBehavior(elementA, _behaviors[0]);
            Assert.AreEqual(0, behavior.EnterInvoked);
            
            vine = _scriptFactory.GetVine(elementB, _vines[0]);
            vine.FinishConfigure();
            
            Assert.AreEqual(1, cbCalled);
            
            _scriptRunner.StopRunner();
            Assert.AreEqual(1, vine.ExitInvoked);
            Assert.AreEqual(1, behavior.ExitInvoked);
            Assert.AreEqual(0, behavior.LastExitInvokeId);
        }
        
        #endregion
        
        #region Updating Scripts

        [Test]
        public void Update_Vine()
        {
            var element = ElementUtil.CreateElement(_scriptManager, _vines[0], _behaviors[0]);
            
            _scriptRunner.AddSceneRoot(element);

            TestVineScript initialVine = null;
            TestBehaviorScript behavior = null;
            var cbCalled = 0;
            _scriptRunner.StartRunner()
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
            
            initialVine = _scriptFactory.GetVine(element, _vines[0]);
            behavior = _scriptFactory.GetBehavior(element, _behaviors[0]);
            
            initialVine.FinishConfigure();
            Assert.AreEqual(1, cbCalled);
            
            // Update, check to make sure nothing enters before the new one is ready.
            // Ensure the behavior also exits/enters with the same instance.
            initialVine.EnkluScript.Updated();
            var updatedVine = _scriptFactory.GetVine(element, _vines[0]);
            Assert.AreNotSame(initialVine, updatedVine);
            Assert.AreEqual(1, initialVine.ExitInvoked);
            Assert.AreEqual(1, behavior.ExitInvoked);
            Assert.AreEqual(0, updatedVine.EnterInvoked);
            
            updatedVine.FinishConfigure();
            Assert.AreEqual(1, initialVine.ExitInvoked);
            Assert.AreEqual(1, behavior.ExitInvoked);
            Assert.AreEqual(1, updatedVine.EnterInvoked);
            Assert.AreEqual(2, behavior.EnterInvoked);
            
            _scriptRunner.StopRunner();
            Assert.AreEqual(1, initialVine.ExitInvoked);
            Assert.AreEqual(1, updatedVine.ExitInvoked);
            Assert.AreEqual(2, behavior.ExitInvoked);
        }

        [Test]
        public void Update_Behavior()
        {
            var element = ElementUtil.CreateElement(_scriptManager, _behaviors[0]);
            
            _scriptRunner.AddSceneRoot(element);
            var token = _scriptRunner.StartRunner();

            TestBehaviorScript initialBehavior = null;
            var cbCalled = 0;
            token
                .OnSuccess(_ =>
                {
                    initialBehavior = _scriptFactory.GetBehavior(element, _behaviors[0]);
                    Assert.NotNull(initialBehavior);
                    Assert.AreEqual(1, initialBehavior.EnterInvoked);
                    Assert.AreEqual(0, initialBehavior.ExitInvoked);
                    cbCalled++;
                })
                .OnFailure(exception =>
                {
                    Assert.Fail("Failed to start runner: " + exception);
                });
            Assert.AreEqual(1, cbCalled);
            
            // Update, ensure the old component exits and the new component is entered.
            initialBehavior.EnkluScript.Updated();
            var updatedBehavior = _scriptFactory.GetBehavior(element, _behaviors[0]);
            Assert.AreEqual(1, updatedBehavior.EnterInvoked);
            Assert.AreEqual(1, initialBehavior.EnterInvoked);
            Assert.AreEqual(1, initialBehavior.ExitInvoked);
            
            // Make sure original still only has exited once.
            _scriptRunner.StopRunner();
            Assert.AreEqual(1, updatedBehavior.ExitInvoked);
            Assert.AreEqual(1, initialBehavior.ExitInvoked);
        }
        
        #endregion
        
        #region Hierarchy

        [Test]
        public void AddChild()
        {
            var elementA = ElementUtil.CreateElement(_scriptManager, _behaviors[0]);
            var elementB = ElementUtil.CreateElement(_scriptManager, _behaviors[1]);
            
            _scriptRunner.AddSceneRoot(elementA);
            var cbCalled = 0;
            _scriptRunner.StartRunner()
                .OnSuccess(_ => cbCalled++)
                .OnFailure(exception => Assert.Fail("Failed to start runner: " + exception));
            Assert.AreEqual(1, cbCalled, "Expected Start to be synchronous.");

            var behavior0 = _scriptFactory.GetBehavior(elementA, _behaviors[0]);
            Assert.AreEqual(1, behavior0.EnterInvoked);
            
            elementA.AddChild(elementB);
            
            var behavior1 = _scriptFactory.GetBehavior(elementB, _behaviors[1]);
            Assert.AreEqual(1, behavior0.EnterInvoked);
            Assert.AreEqual(0, behavior0.ExitInvoked);
            Assert.AreEqual(1, behavior1.EnterInvoked);
            
            _scriptRunner.Update();
            Assert.AreEqual(1, behavior0.UpdateInvoked);
            Assert.AreEqual(1, behavior1.UpdateInvoked);
        }

        [Test]
        public void RemoveChild()
        {
            var elementA = ElementUtil.CreateElement(_scriptManager, _behaviors[0]);
            var elementB = ElementUtil.CreateElement(_scriptManager, _behaviors[1]);
            elementA.AddChild(elementB);
            
            _scriptRunner.AddSceneRoot(elementA);
            var cbCalled = 0;
            _scriptRunner.StartRunner()
                .OnSuccess(_ => cbCalled++)
                .OnFailure(exception => Assert.Fail("Failed to start runner: " + exception));
            Assert.AreEqual(1, cbCalled, "Expected Start to be synchronous.");

            var behavior0 = _scriptFactory.GetBehavior(elementA, _behaviors[0]);
            var behavior1 = _scriptFactory.GetBehavior(elementB, _behaviors[1]);
            
            _scriptRunner.Update();
            Assert.AreEqual(1, behavior0.UpdateInvoked);
            Assert.AreEqual(1, behavior1.UpdateInvoked);

            elementA.RemoveChild(elementB);
            _scriptRunner.Update();
            Assert.AreEqual(2, behavior0.UpdateInvoked);
            Assert.AreEqual(1, behavior1.UpdateInvoked);
            
            Assert.AreEqual(0, behavior0.ExitInvoked);
            Assert.AreEqual(1, behavior1.ExitInvoked);
        }
        
        [Test]
        public void Reparenting()
        {
            // Ensure all widgets are found, even with a boring Element in between.
            var elementA = ElementUtil.CreateElement(_scriptManager, _behaviors[0]);
            var elementB = ElementUtil.CreateElement(_scriptManager, _behaviors[1]);
            var elementC = ElementUtil.CreateElement(_scriptManager, _behaviors[2]);
            var elementD = ElementUtil.CreateElement(_scriptManager, _behaviors[2]);
            
            elementB.AddChild(elementC);
            elementB.AddChild(elementD);
            elementA.AddChild(elementB);
            
            _scriptRunner.AddSceneRoot(elementA);

            TestBehaviorScript behavior0;
            TestBehaviorScript behavior1;
            TestBehaviorScript behavior2_c;
            TestBehaviorScript behavior2_d;
            var cbCalled = 0;
            _scriptRunner.StartRunner()
                .OnSuccess(_ =>
                {
                    cbCalled++;
                    
                    behavior0 = _scriptFactory.GetBehavior(elementA, _behaviors[0]);
                    behavior1 = _scriptFactory.GetBehavior(elementB, _behaviors[1]);
                    behavior2_c = _scriptFactory.GetBehavior(elementC, _behaviors[2]);
                    behavior2_d = _scriptFactory.GetBehavior(elementD, _behaviors[2]);
                    
                    _scriptRunner.Update();
                    Assert.AreEqual(0, behavior0.LastUpdateInvokeId);
                    Assert.AreEqual(1, behavior1.LastUpdateInvokeId);
                    Assert.AreEqual(2, behavior2_c.LastUpdateInvokeId);
                    Assert.AreEqual(3, behavior2_d.LastUpdateInvokeId);
                    
                    elementA.AddChild(elementC);
                    behavior2_c = _scriptFactory.GetBehavior(elementC, _behaviors[2]);
                    
                    // Ensure invoke order changes after reparenting
                    _scriptRunner.Update();
                    Assert.AreEqual(4, behavior0.LastUpdateInvokeId);
                    Assert.AreEqual(5, behavior1.LastUpdateInvokeId);
                    Assert.AreEqual(7, behavior2_c.LastUpdateInvokeId);
                    Assert.AreEqual(6, behavior2_d.LastUpdateInvokeId);
                })
                .OnFailure(exception => Assert.Fail("Failed to start runner: " + exception));
            Assert.AreEqual(1, cbCalled);
        }
        
        #endregion
        
        #region Visibility

        [Test]
        public void InvisibleElement()
        {
            var element = ElementUtil.CreateElement(_scriptManager, _behaviors[0]);
            element.LocalVisibleProp.Value = false;

            _scriptRunner.AddSceneRoot(element);

            TestBehaviorScript behavior0;
            var cbCalled = 0;
            _scriptRunner.StartRunner()
                .OnSuccess(_ =>
                {
                    cbCalled++;
                    behavior0 = _scriptFactory.GetBehavior(element, _behaviors[0]);
                    
                    // Should not be invoked if it is invisible
                    Assert.AreEqual(0, behavior0.EnterInvoked);
                    
                    _scriptRunner.Update();
                    Assert.AreEqual(0, behavior0.UpdateInvoked);

                    // Make visible, ensure the script enters/updates properly
                    element.LocalVisibleProp.Value = true;
                    Assert.AreEqual(1, behavior0.EnterInvoked);
                    Assert.AreEqual(0, behavior0.UpdateInvoked);
                    
                    _scriptRunner.Update();
                    Assert.AreEqual(1, behavior0.UpdateInvoked);
                })
                .OnFailure(exception => Assert.Fail("Failed to start runner: " + exception));
            Assert.AreEqual(1, cbCalled);
        }

        [Test]
        public void InvisibleParent()
        {
            var parent = ElementUtil.CreateElement(_scriptManager, _behaviors[0]);
            var child = ElementUtil.CreateElement(_scriptManager, _behaviors[1]);
            parent.LocalVisibleProp.Value = false;
            
            parent.AddChild(child);
            _scriptRunner.AddSceneRoot(parent);

            TestBehaviorScript behavior0;
            TestBehaviorScript behavior1;
            var cbCalled = 0;
            _scriptRunner.StartRunner()
                .OnSuccess(_ =>
                {
                    cbCalled++;
                    behavior0 = _scriptFactory.GetBehavior(parent, _behaviors[0]);
                    behavior1 = _scriptFactory.GetBehavior(child, _behaviors[1]);
                    
                    // Ensure the child doesn't run
                    Assert.AreEqual(0, behavior0.EnterInvoked);
                    Assert.AreEqual(0, behavior1.EnterInvoked);
                    
                    _scriptRunner.Update();
                    Assert.AreEqual(0, behavior0.UpdateInvoked);
                    Assert.AreEqual(0, behavior1.UpdateInvoked);

                    parent.LocalVisibleProp.Value = true;
                    
                    // Ensure the child runs
                    Assert.AreEqual(1, behavior0.EnterInvoked);
                    Assert.AreEqual(1, behavior1.EnterInvoked);
                    
                    _scriptRunner.Update();
                    Assert.AreEqual(1, behavior0.UpdateInvoked);
                    Assert.AreEqual(1, behavior1.UpdateInvoked);
                })
                .OnFailure(exception => Assert.Fail("Failed to start runner: " + exception));
            Assert.AreEqual(1, cbCalled);
        }

        [Test]
        public void NeverVisible()
        {
            // Ensure Elements that never become visible have their callbacks cleaned up.

            var element = ElementUtil.CreateElement(_scriptManager, _behaviors[0]);
            element.LocalVisibleProp.Value = false;
            
            _scriptRunner.AddSceneRoot(element);

            TestBehaviorScript behavior0;
            var cbCalled = 0;
            _scriptRunner.StartRunner()
                .OnSuccess(_ =>
                {
                    cbCalled++;
                    behavior0 = _scriptFactory.GetBehavior(element, _behaviors[0]);
                    
                    Assert.AreEqual(0, behavior0.EnterInvoked);
                    
                    _scriptRunner.StopRunner();

                    element.LocalVisibleProp.Value = true;
                    Assert.AreEqual(0, behavior0.EnterInvoked);
                })
                .OnFailure(exception => Assert.Fail("Failed to start runner: " + exception));
            Assert.AreEqual(1, cbCalled);
        }
        
        #endregion

        #region Misc
        
        [Test]
        public void ScriptSharing()
        {
            var widgetA = ElementUtil.CreateElement(_scriptManager, _behaviors[0]);
            var widgetB = ElementUtil.CreateElement(_scriptManager, _behaviors[0]);
            
            widgetA.AddChild(widgetB);
            
            _scriptRunner.AddSceneRoot(widgetA);

            TestBehaviorScript aBehavior = null;
            TestBehaviorScript bBehavior = null;
            var cbCalled = 0;
            _scriptRunner.StartRunner()
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
           
           _scriptRunner.StopRunner();
           Assert.AreEqual(1, aBehavior.ExitInvoked);
           Assert.AreEqual(1, bBehavior.ExitInvoked);
           Assert.AreEqual(0, aBehavior.LastExitInvokeId);
           Assert.AreEqual(1, bBehavior.LastExitInvokeId);
        }
        
        #endregion
    }
}