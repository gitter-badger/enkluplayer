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
    public class ScriptService_Tests
    {
        private ScriptService _scriptService;
        private IElementManager _elementManager;
        
        private TestScriptFactory _scriptFactory;
        private EnkluScript[] _behaviors = new EnkluScript[3];
        private EnkluScript[] _vines = new EnkluScript[3];
        
        [SetUp]
        public void Setup()
        {
            _elementManager = new TestElementManager();
            _scriptService = new ScriptService(null, null, _elementManager);
            
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
        public void InitialScene()
        {
            var behavior = CreateWidget(_behaviors[0]);
            _elementManager.Add(behavior);
            
            var vine = CreateWidget(_vines[0]);
            _elementManager.Add(vine);

            _scriptService.Start();

            var vineComponent = _scriptFactory.GetVine(_vines[0]);
            var behaviorComponent = _scriptFactory.GetBehavior(_behaviors[0]);
            
            Assert.AreEqual(0, vineComponent.EnterInvoked);
            Assert.IsNull(behaviorComponent);
            
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
                existingScripts.Add(script.Data.Id);
            }
            
            widget.Schema.Set("scripts", existingScripts);
        }
    }
}