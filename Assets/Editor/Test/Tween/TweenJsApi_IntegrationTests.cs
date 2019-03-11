using System;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Test.Scripting;
using CreateAR.EnkluPlayer.Util;
using Jint;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Util.Tween
{
    [TestFixture]
    public class TweenJsApi_IntegrationTests
    {
        private TweenManager _tweens;
        private TweenManagerJsApi _api;
        private ElementJs _el;

        [SetUp]
        public void Setup()
        {
            _tweens = new TweenManager();
            _api = new TweenManagerJsApi(_tweens);

            var scripts = new DummyScriptManager(false);
            _el = new ElementJs(scripts, new ElementJsCache(new ElementJsFactory(scripts)), new Element());
        }

        [Test]
        public void TweenData()
        {
            const float from = 1.4f;
            const float to = 7.4f;
            const float duration = 45f;
            const float delay = 10f;
            const string easing = TweenEasingTypes.BounceOut;

            var tween = _api
                .number(_el, "foo")
                .from(from)
                .to(to)
                .duration(duration)
                .delay(delay)
                .easing(easing.ToString());

            Assert.AreEqual("foo", tween.Data.Prop);
            Assert.IsTrue(tween.Data.CustomFrom);
            Assert.IsTrue(Math.Abs(from - (float) tween.Data.From) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(to - (float) tween.Data.To) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(duration - tween.Data.DurationSec) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(delay - tween.Data.DelaySec) < Mathf.Epsilon);
            Assert.AreEqual(easing, tween.Data.Easing);
        }

        [Test]
        public void TweenEvents()
        {
            var startCalled = false;
            var completeCalled = false;

            var tween = _api
                .number(_el, "foo")
                .to(1f)
                .duration(1f)
                .onStart(new TestJsCallback((a, b) =>
                {
                    startCalled = true;

                    return null;
                }))
                .onComplete(new TestJsCallback((a, b) =>
                {
                    completeCalled = true;

                    return null;
                }));

            Assert.IsFalse(startCalled);
            Assert.IsFalse(completeCalled);

            tween.start();

            Assert.IsTrue(startCalled);
            Assert.IsFalse(completeCalled);

            _tweens.Update(1f);

            Assert.IsTrue(completeCalled);
        }
    }
}