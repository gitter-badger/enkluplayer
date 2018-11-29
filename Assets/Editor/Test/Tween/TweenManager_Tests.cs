using System;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Util;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Util
{
    [TestFixture]
    public class TweenManager_Tests
    {
        private TweenManager _tweens;
        private Element _el;

        [SetUp]
        public void Setup()
        {
            _tweens = new TweenManager();
            _el = new Element();
        }

        [Test]
        public void StartTween()
        {
            // create tween
            var tween = _tweens.Float(_el, new TweenData
            {
                Prop = "foo",
                To = 1f,
                DurationSec = 1f
            });

            // start and advance the manager
            _tweens.Start(tween);

            Assert.IsTrue(_el.Schema.Get<float>("foo").Value < Mathf.Epsilon);

            _tweens.Update(0.5f);

            Assert.IsTrue(Math.Abs(0.5f - _el.Schema.Get<float>("foo").Value) < Mathf.Epsilon);

            _tweens.Update(10f);

            Assert.IsTrue(Math.Abs(1f - _el.Schema.Get<float>("foo").Value) < Mathf.Epsilon);
            Assert.IsTrue(tween.IsComplete);
        }

        [Test]
        public void AbortTween()
        {
            // create tween
            var tween = _tweens.Float(_el, new TweenData
            {
                Prop = "foo",
                To = 1f,
                DurationSec = 1f
            });

            // start and advance the manager
            _tweens.Start(tween);
            _tweens.Update(0.5f);

            Assert.IsTrue(Math.Abs(0.5f - _el.Schema.Get<float>("foo").Value) < Mathf.Epsilon);

            // abort
            _tweens.Abort(tween);
            _tweens.Update(10f);

            // make sure it did not advance
            Assert.IsTrue(Math.Abs(0.5f - _el.Schema.Get<float>("foo").Value) < Mathf.Epsilon);
            Assert.IsFalse(tween.IsComplete);
        }

        [Test]
        public void PauseResumeTween()
        {
            // create tween
            var tween = _tweens.Float(_el, new TweenData
            {
                Prop = "foo",
                To = 1f,
                DurationSec = 1f
            });

            // start and advance the manager
            _tweens.Start(tween);

            Assert.IsTrue(_el.Schema.Get<float>("foo").Value < Mathf.Epsilon);

            _tweens.Update(0.5f);

            Assert.IsTrue(Math.Abs(0.5f - _el.Schema.Get<float>("foo").Value) < Mathf.Epsilon);

            _tweens.Pause(tween);
            _tweens.Update(10f);

            Assert.IsTrue(Math.Abs(0.5f - _el.Schema.Get<float>("foo").Value) < Mathf.Epsilon);

            _tweens.Resume(tween);
            _tweens.Update(10f);

            Assert.IsTrue(Math.Abs(1f - _el.Schema.Get<float>("foo").Value) < Mathf.Epsilon);
            Assert.IsTrue(tween.IsComplete);
        }
    }
}