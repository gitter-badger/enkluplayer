using CreateAR.Enkluplayer.Test;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Vine;
using Jint.Parser;
using NUnit.Framework;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class ScriptService_Tests
    {
        private TestScriptManager _scriptManager;
        private TestAppController _appController;
        private ScriptService _scriptService;
        
        
        private TestScriptFactory _scriptFactory;
        private readonly EnkluScript[] _behaviors = new EnkluScript[3];
        private readonly EnkluScript[] _vines = new EnkluScript[3];

        private TestSceneManager _sceneManager;
        
        [SetUp]
        public void Setup()
        {
            _appController = new TestAppController();
            _sceneManager = new TestSceneManager();
            _scriptManager = new TestScriptManager();
            _scriptFactory = new TestScriptFactory();
            _scriptService = new ScriptService(
                null, 
                null, 
                _scriptManager, 
                _scriptFactory,
                new TestScriptExecutorFactory(), 
                _sceneManager,
                _appController);
            
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

        /// Tests a scene loading from scratch.
        [Test]
        public void InitialScene()
        {
            var parentElement = ElementUtil.CreateElement();
            _sceneManager.SetRoot(parentElement);
            
            var behaviorElement = ElementUtil.CreateElement(_scriptManager, _behaviors[0]);
            var vineElement = ElementUtil.CreateElement(_scriptManager, _vines[0]);
            
            parentElement.AddChild(behaviorElement);
            parentElement.AddChild(vineElement);
            
            _scriptService.Start();
            _appController.Load(null);

            var vineComponent = _scriptFactory.GetVine(vineElement, _vines[0]);
            var behaviorComponent = _scriptFactory.GetBehavior(behaviorElement, _behaviors[0]);
            
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
            var parentElement = ElementUtil.CreateElement();
            _sceneManager.SetRoot(parentElement);
            
            _scriptService.Start();
            _appController.Load(null);

            
            // Late add Vine
            var vineElement = ElementUtil.CreateElement(_scriptManager, _vines[0]);
            parentElement.AddChild(vineElement);
            
            var vineComponent = _scriptFactory.GetVine(vineElement, _vines[0]);
            
            Assert.AreEqual(0, vineComponent.EnterInvoked);
            vineComponent.FinishConfigure();
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            
            // Late add Behavior
            var behaviorElement = ElementUtil.CreateElement(_scriptManager, _behaviors[0]);
            parentElement.AddChild(behaviorElement);
            
            var behaviorComponent = _scriptFactory.GetBehavior(behaviorElement, _behaviors[0]);
            
            Assert.AreEqual(1, behaviorComponent.EnterInvoked);
            
            // Late add Combined
            var mixedElement = ElementUtil.CreateElement(_scriptManager, _behaviors[1], _vines[1]);
            parentElement.AddChild(mixedElement);

            vineComponent = _scriptFactory.GetVine(mixedElement, _vines[1]);
            behaviorComponent = _scriptFactory.GetBehavior(mixedElement, _behaviors[1]);
            
            Assert.AreEqual(0, vineComponent.EnterInvoked);
            Assert.AreEqual(0, behaviorComponent.EnterInvoked);
            vineComponent.FinishConfigure();
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            Assert.AreEqual(1, behaviorComponent.EnterInvoked);
        }
    }
}