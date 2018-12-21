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
            var behaviorWidget = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0]);
            _elementManager.Add(behaviorWidget);
            
            var vineWidget = WidgetUtil.CreateWidget(_scriptManager, _vines[0]);
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
            var vineWidget = WidgetUtil.CreateWidget(_scriptManager, _vines[0]);
            _elementManager.Add(vineWidget);
            
            var vineComponent = _scriptFactory.GetVine(_vines[0]);
            
            Assert.AreEqual(0, vineComponent.EnterInvoked);
            vineComponent.FinishConfigure();
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            
            // Late add Behavior
            var behaviorWidget = WidgetUtil.CreateWidget(_scriptManager, _behaviors[0]);
            _elementManager.Add(behaviorWidget);
            
            var behaviorComponent = _scriptFactory.GetBehavior(_behaviors[0]);
            
            Assert.AreEqual(1, behaviorComponent.EnterInvoked);
            
            // Late add Combined
            var mixedWidget = WidgetUtil.CreateWidget(_scriptManager, _behaviors[1], _vines[1]);
            _elementManager.Add(mixedWidget);

            vineComponent = _scriptFactory.GetVine(_vines[1]);
            behaviorComponent = _scriptFactory.GetBehavior(_behaviors[1]);
            
            Assert.AreEqual(0, vineComponent.EnterInvoked);
            Assert.AreEqual(0, behaviorComponent.EnterInvoked);
            vineComponent.FinishConfigure();
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            Assert.AreEqual(1, behaviorComponent.EnterInvoked);
        }
    }
}