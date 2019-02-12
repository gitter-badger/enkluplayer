using Assets.Source.Player.Scripting;
using Jint;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class TimeJsApi_Test
    {
        private Engine _engine;

        [SetUp]
        public void Setup()
        {
            _engine = ScriptingHostFactory.NewEngine(false);

            _engine.SetValue("time", TimeJsApi.Instance);
        }

        [Test]
        public void Now()
        {
            var result = (float) _engine.Run("time.now()").AsNumber();
            var unityTime = Time.time;

            Assert.IsTrue(Mathf.Approximately(result, unityTime));
        }

        [Test]
        public void DeltaTime()
        {
            var result = (float) _engine.Run("time.dt()").AsNumber();

            Assert.IsTrue(Mathf.Approximately(result, Time.deltaTime));
        }
    }
}