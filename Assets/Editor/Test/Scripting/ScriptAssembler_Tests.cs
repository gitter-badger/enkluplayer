using System;
using System.Security.Cryptography.X509Certificates;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Vine;
using Jint.Parser;
using NUnit.Framework;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class ScriptAssembler_Tests
    {
        private IScriptAssembler _scriptAssembler;

        private TestScriptManager _scriptManager;
        private TestScriptFactory _scriptFactory;
        private EnkluScript[] _behaviors = new EnkluScript[3];
        private EnkluScript[] _vines = new EnkluScript[3];
        
        [SetUp]
        public void Setup()
        {
            _scriptManager = new TestScriptManager();
            _scriptFactory = new TestScriptFactory();
            
            _scriptAssembler = new ScriptAssembler(
                _scriptManager, 
                _scriptFactory,
                new ElementJsCache(new ElementJsFactory(_scriptManager)),
                null,
                null
            );
            
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

        #region Initial Scripts

        [Test]
        public void Initial_None()
        {
            var widget = WidgetUtil.CreateWidget();

            var cbCalled = 0;
            _scriptAssembler.OnScriptsUpdated += (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(0, @new.Length);
                cbCalled++;
            };
            _scriptAssembler.Setup(widget);
            
            Assert.AreEqual(1, cbCalled);
        }

        [Test]
        public void Initial_Vine()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _vines[0], _vines[1]);

            var cbCalled = 0;
            _scriptAssembler.OnScriptsUpdated += (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(2, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.NotNull(@new[1]);
                Assert.AreEqual(_vines[0], @new[0].EnkluScript);
                Assert.AreEqual(_vines[1], @new[1].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.Setup(widget);

            // Ensure invoke hasn't happened until Vine has loaded.
            Assert.AreEqual(0, cbCalled);
            
            var vineComponent0 = _scriptFactory.GetVine(widget, _vines[0]);
            var vineComponent1 = _scriptFactory.GetVine(widget, _vines[1]);
            vineComponent0.FinishConfigure();
            vineComponent1.FinishConfigure();
            
            Assert.AreEqual(1, cbCalled);
        }
        
        [Test]
        public void Initial_Behavior()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0], _behaviors[1]);

            var cbCalled = 0;
            _scriptAssembler.OnScriptsUpdated += (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(2, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.NotNull(@new[1]);
                Assert.AreEqual(_behaviors[0], @new[0].EnkluScript);
                Assert.AreEqual(_behaviors[1], @new[1].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.Setup(widget);
            
            Assert.AreEqual(1, cbCalled);
        }

        [Test]
        public void Initial_Mixed()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0], _vines[0]);

            var cbCalled = 0;
            _scriptAssembler.OnScriptsUpdated += (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(2, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.NotNull(@new[1]);
                Assert.AreEqual(_behaviors[0], @new[0].EnkluScript);
                Assert.AreEqual(_vines[0], @new[1].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.Setup(widget);
            
            var vineComponent0 = _scriptFactory.GetVine(widget, _vines[0]);
            vineComponent0.FinishConfigure();
            
            Assert.AreEqual(1, cbCalled);
        }
        #endregion
        
        #region Adding Scripts

        [Test]
        public void New_NoPrior()
        {
            var widget = WidgetUtil.CreateWidget();

            var cbCalled = 0;

            Action<Script[], Script[]> initialInvoke = (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(0, @new.Length);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated += initialInvoke;

            _scriptAssembler.Setup(widget);
            Assert.AreEqual(1, cbCalled);


            Action<Script[], Script[]> updateInvoke = (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(1, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.AreEqual(_behaviors[0], @new[0].EnkluScript);
                
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated -= initialInvoke;
            _scriptAssembler.OnScriptsUpdated += updateInvoke;
            
            WidgetUtil.AddScriptToWidget(widget, _scriptManager, _behaviors[0]);
            
            Assert.AreEqual(2, cbCalled);
        }

        [Test]
        public void New_VinePrior()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _vines[0]);

            var cbCalled = 0;

            Action<Script[], Script[]> initialInvoke = (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(1, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.AreEqual(_vines[0], @new[0].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated += initialInvoke;
            
            _scriptAssembler.Setup(widget);

            var vineComponent = _scriptFactory.GetVine(widget, _vines[0]);
            vineComponent.FinishConfigure();
            
            Assert.AreEqual(1, cbCalled);
            
            Action<Script[], Script[]> updateInvoke = (old, @new) =>
            {
                Assert.AreEqual(1, old.Length);
                Assert.AreEqual(2, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.NotNull(@new[1]);
                Assert.AreEqual(_vines[0], @new[0].EnkluScript);
                Assert.AreEqual(_behaviors[0], @new[1].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated -= initialInvoke;
            _scriptAssembler.OnScriptsUpdated += updateInvoke;
            
            WidgetUtil.AddScriptToWidget(widget, _scriptManager, _behaviors[0]);
            
            // The component is rebuilt, so it needs to be finalized again.
            vineComponent = _scriptFactory.GetVine(widget, _vines[0]);
            vineComponent.FinishConfigure();
            
            Assert.AreEqual(2, cbCalled);
        }
        
        [Test]
        public void New_BehaviorPrior()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0]);

            var cbCalled = 0;

            Action<Script[], Script[]> initialInvoke = (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(1, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.AreEqual(_behaviors[0], @new[0].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated += initialInvoke;
            
            _scriptAssembler.Setup(widget);
            Assert.AreEqual(1, cbCalled);
            
            Action<Script[], Script[]> updateInvoke = (old, @new) =>
            {
                Assert.AreEqual(1, old.Length);
                Assert.AreEqual(2, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.NotNull(@new[1]);
                Assert.AreEqual(_behaviors[0], @new[0].EnkluScript);
                Assert.AreEqual(_vines[0], @new[1].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated -= initialInvoke;
            _scriptAssembler.OnScriptsUpdated += updateInvoke;
            
            WidgetUtil.AddScriptToWidget(widget, _scriptManager, _vines[0]);
            
            var vineComponent = _scriptFactory.GetVine(widget, _vines[0]);
            vineComponent.FinishConfigure();
            
            Assert.AreEqual(2, cbCalled);
        }

        [Test]
        public void New_Multiple()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0]);

            var cbCalled = 0;

            Action<Script[], Script[]> initialInvoke = (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(1, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.AreEqual(_behaviors[0], @new[0].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated += initialInvoke;
            
            _scriptAssembler.Setup(widget);
            Assert.AreEqual(1, cbCalled);
            
            Action<Script[], Script[]> updateInvoke = (old, @new) =>
            {
                Assert.AreEqual(1, old.Length);
                Assert.AreEqual(3, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.NotNull(@new[1]);
                Assert.NotNull(@new[2]);
                Assert.AreEqual(_behaviors[0], @new[0].EnkluScript);
                Assert.AreEqual(_behaviors[1], @new[1].EnkluScript);
                Assert.AreEqual(_vines[0], @new[2].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated -= initialInvoke;
            _scriptAssembler.OnScriptsUpdated += updateInvoke;
            
            WidgetUtil.AddScriptToWidget(widget, _scriptManager, _vines[0], _behaviors[1]);
            
            var vineComponent = _scriptFactory.GetVine(widget, _vines[0]);
            vineComponent.FinishConfigure();
            
            Assert.AreEqual(2, cbCalled);
        }
        #endregion
        
        #region Script Updates
        
        [Test]
        public void Updating_Vine()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _vines[0], _vines[1]);

            var cbCalled = 0;
            Action<Script[], Script[]> initialInvoke = (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(2, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.NotNull(@new[1]);
                Assert.AreEqual(_vines[0], @new[0].EnkluScript);
                Assert.AreEqual(_vines[1], @new[1].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated += initialInvoke;
            _scriptAssembler.Setup(widget);
            
            var vineComponent0 = _scriptFactory.GetVine(widget, _vines[0]);
            var vineComponent1 = _scriptFactory.GetVine(widget, _vines[1]);
            vineComponent0.FinishConfigure();
            vineComponent1.FinishConfigure();
            
            Assert.AreEqual(1, cbCalled);

            Action<Script[], Script[]> updateInvoke = (old, @new) =>
            {
                Assert.AreEqual(2, old.Length);
                Assert.AreEqual(2, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.NotNull(@new[1]);
                Assert.AreEqual(_vines[0], @new[0].EnkluScript);
                Assert.AreEqual(_vines[1], @new[1].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated -= initialInvoke;
            _scriptAssembler.OnScriptsUpdated += updateInvoke;

            _vines[0].Updated();
            var newComponent = _scriptFactory.GetVine(widget, _vines[0]);
            newComponent.FinishConfigure();
            
            Assert.AreNotEqual(vineComponent0, newComponent);
            
            Assert.AreEqual(2, cbCalled);
        }
        
        [Test]
        public void Updating_Behavior()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0], _behaviors[1]);

            var cbCalled = 0;
            Action<Script[], Script[]> initialInvoke = (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(2, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.NotNull(@new[1]);
                Assert.AreEqual(_behaviors[0], @new[0].EnkluScript);
                Assert.AreEqual(_behaviors[1], @new[1].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated += initialInvoke;
            _scriptAssembler.Setup(widget);
            
            var behaviorComponent = _scriptFactory.GetBehavior(widget, _behaviors[0]);
            
            Assert.AreEqual(1, cbCalled);

            Action<Script[], Script[]> updateInvoke = (old, @new) =>
            {
                Assert.AreEqual(2, old.Length);
                Assert.AreEqual(2, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.NotNull(@new[1]);
                Assert.AreEqual(_behaviors[0], @new[0].EnkluScript);
                Assert.AreEqual(_behaviors[1], @new[1].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated -= initialInvoke;
            _scriptAssembler.OnScriptsUpdated += updateInvoke;

            _behaviors[0].Updated();
            var newComponent = _scriptFactory.GetBehavior(widget, _behaviors[0]);
            
            Assert.AreNotEqual(behaviorComponent, newComponent);
            
            Assert.AreEqual(2, cbCalled);
        }
        #endregion
        
        #region Removing Scripts

        [Test]
        public void Removal()
        {
            var widget = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0], _vines[0]);

            var cbCalled = 0;
            
            // Setup
            Action<Script[], Script[]> initialInvoke = (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(2, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.NotNull(@new[1]);
                Assert.AreEqual(_behaviors[0], @new[0].EnkluScript);
                Assert.AreEqual(_vines[0], @new[1].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated += initialInvoke;
            _scriptAssembler.Setup(widget);
            
            var vineComponent0 = _scriptFactory.GetVine(widget, _vines[0]);
            vineComponent0.FinishConfigure();
            
            Assert.AreEqual(1, cbCalled);

            // Remove 1st
            Action<Script[], Script[]> firstRemoval = (old, @new) =>
            {
                Assert.AreEqual(2, old.Length);
                Assert.AreEqual(1, @new.Length);

                Assert.NotNull(@new[0]);
                Assert.AreEqual(_behaviors[0], @new[0].EnkluScript);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated -= initialInvoke;
            _scriptAssembler.OnScriptsUpdated += firstRemoval;
            
            WidgetUtil.RemoveScriptFromWidget(widget, _scriptManager, _vines[0]);
            
            Assert.AreEqual(2, cbCalled);
            
            // Remove 2nd
            Action<Script[], Script[]> secondRemoval = (old, @new) =>
            {
                Assert.AreEqual(1, old.Length);
                Assert.AreEqual(0, @new.Length);
                cbCalled++;
            };
            _scriptAssembler.OnScriptsUpdated -= firstRemoval;
            _scriptAssembler.OnScriptsUpdated += secondRemoval;
            
            WidgetUtil.RemoveScriptFromWidget(widget, _scriptManager, _behaviors[0]);
            
            Assert.AreEqual(3, cbCalled);
        }
        
        #endregion
        
        #region Content Widgets

        [Test]
        public void NoAsset()
        {
            var widget = WidgetUtil.CreateContentWidget(_scriptManager);

            var cbCalled = 0;
            _scriptAssembler.OnScriptsUpdated += (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(0, @new.Length);

                cbCalled++;
            };
            _scriptAssembler.Setup(widget);
            
            Assert.AreEqual(1, cbCalled);
        }
        
        [Test]
        public void DelayedAsset()
        {
            var assetAssembler = new TestAssetAssembler();
            var widget = WidgetUtil.CreateContentWidget(_scriptManager, assetAssembler);

            var cbCalled = 0;
            _scriptAssembler.OnScriptsUpdated += (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(0, @new.Length);

                cbCalled++;
            };
            _scriptAssembler.Setup(widget);
            
            Assert.AreEqual(0, cbCalled);
            
            assetAssembler.FinishLoad();
            Assert.AreEqual(1, cbCalled);
        }
        
        [Test]
        public void AssetUpdated()
        {
            var assetAssembler = new TestAssetAssembler();
            var widget = WidgetUtil.CreateContentWidget(_scriptManager, assetAssembler);

            var cbCalled = 0;
            _scriptAssembler.OnScriptsUpdated += (old, @new) =>
            {
                Assert.AreEqual(0, old.Length);
                Assert.AreEqual(0, @new.Length);

                cbCalled++;
            };
            _scriptAssembler.Setup(widget);
            
            assetAssembler.FinishLoad();
            Assert.AreEqual(1, cbCalled);
            
            assetAssembler.FinishLoad();
            Assert.AreEqual(2, cbCalled);
        }
        #endregion
    }
}
