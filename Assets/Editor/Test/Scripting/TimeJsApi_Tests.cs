﻿using Jint;
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
            _engine = new Engine(options => {
                options.CatchClrExceptions(exception => { throw exception; });
                options.AllowClr();
            });

            _engine.SetValue("time", TimeJsApi.Instance);
        }

        [Test]
        public void Now()
        {
            var result = (float) _engine.Run("time.now()").AsNumber();

            // Enklu uses ms while Unity uses seconds.
            var unityTime = Time.time * 1000;

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