using System.Collections.Generic;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Vine;
using Jint.Parser;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class ScriptCollectionRunner_Tests
    {
        private ScriptCollectionRunner _scriptRunner;
        private TestScriptFactory _scriptFactory;

        private EnkluScript _behavior;
        private EnkluScript _vine;

        [SetUp]
        public void Setup()
        {
            _scriptFactory = new TestScriptFactory();
            
            var parser = new DefaultScriptParser(
                null, new JsVinePreProcessor(), new JavaScriptParser());
            

            var behaviorData = new ScriptData
            {
                Id = "script-behavior"
            };

            var vineData = new ScriptData
            {
                Id = "script-vine",
                TagString = "vine"
            };

            // For testing, just load the script IDs as the source program.
            _behavior = new EnkluScript(
                parser, new TestScriptLoader(behaviorData.Id), behaviorData);
            _vine = new EnkluScript(
                parser, new TestScriptLoader(behaviorData.Id), vineData);

            _scriptRunner = new ScriptCollectionRunner(
                new GameObject("ScriptCollectionRunner_Tests"),
                _scriptFactory);
        }

        [Test]
        public void Behavior()
        {
            // Behaviors load synchronously. Check that the script has loaded & invoked.
            _scriptRunner.Setup(new List<EnkluScript>{ _behavior });
            Assert.AreEqual(ScriptCollectionRunner.SetupState.Done, _scriptRunner.CurrentState);

            var component = _scriptFactory.GetBehavior(_behavior);
            Assert.AreEqual(1, component.EnterInvoked);
        }

        [Test]
        public void Vine()
        {
            // Vines are async, check that it hasn't finished.
            _scriptRunner.Setup(new List<EnkluScript>{ _vine });
            Assert.AreEqual(ScriptCollectionRunner.SetupState.Initializing, _scriptRunner.CurrentState);
            
            var component = _scriptFactory.GetVine(_vine);
            
            Assert.AreEqual(0, component.EnterInvoked);
            
            // Resolve pending finish, and check for loaded & invoked.
            component.FinishConfigure();
            Assert.AreEqual(ScriptCollectionRunner.SetupState.Done, _scriptRunner.CurrentState);
            Assert.AreEqual(1, component.EnterInvoked);
        }

        [Test]
        public void Combined()
        {
            // Test both scripts together.
            _scriptRunner.Setup(new List<EnkluScript>{ _behavior, _vine });
            Assert.AreEqual(ScriptCollectionRunner.SetupState.Initializing, _scriptRunner.CurrentState);
            
            var vineComponent = _scriptFactory.GetVine(_vine);
            var behaviorComponent = _scriptFactory.GetBehavior(_behavior);
            
            // Ensure vine is loading, behavior doesn't exist yet
            Assert.AreEqual(0, vineComponent.EnterInvoked);
            Assert.Null(behaviorComponent);
            
            // Resolve the vine, check that things loaded & invoked.
            vineComponent.FinishConfigure();
            behaviorComponent = _scriptFactory.GetBehavior(_behavior);
            
            Assert.AreEqual(ScriptCollectionRunner.SetupState.Done, _scriptRunner.CurrentState);
            Assert.AreEqual(1, vineComponent.EnterInvoked);
            Assert.AreEqual(1, behaviorComponent.EnterInvoked);
        }
    }
}
