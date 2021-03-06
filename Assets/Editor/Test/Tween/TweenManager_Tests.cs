﻿using System;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Util;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Util.Tween
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
        public void StartTweenEvent()
        {
            var called = false;

            // create tween
            var tween = _tweens.Float(_el, new TweenData
            {
                Prop = "foo",
                To = 1f,
                DurationSec = 1f
            });
            tween.OnStart += () => called = true;

            _tweens.Start(tween);

            Assert.IsTrue(called);
        }

        [Test]
        public void StopTween()
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
            _tweens.Stop(tween);
            _tweens.Update(10f);

            // make sure it did not advance
            Assert.IsTrue(Math.Abs(0.5f - _el.Schema.Get<float>("foo").Value) < Mathf.Epsilon);
            Assert.IsFalse(tween.IsComplete);
        }

        [Test]
        public void StopStartTween()
        {
            // create tween
            var tween = _tweens.Float(_el, new TweenData
            {
                Prop = "foo",
                To = 1f,
                DurationSec = 1f
            });

            _tweens.Start(tween);
            _tweens.Update(0.5f);
            _tweens.Stop(tween);

            _tweens.Start(tween);

            // make sure it was restarted
            Assert.IsTrue(_el.Schema.Get<float>("foo").Value < Mathf.Epsilon);

            _tweens.Update(0.5f);

            // make sure it advances properly again
            Assert.IsTrue(Math.Abs(0.5f - _el.Schema.Get<float>("foo").Value) < Mathf.Epsilon);
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

        [Test]
        public void CompleteRestartTween()
        {
            // create tween
            var tween = _tweens.Float(_el, new TweenData
            {
                Prop = "foo",
                To = 1f,
                DurationSec = 1f
            });

            _tweens.Start(tween);
            _tweens.Update(10f);

            Assert.IsTrue(tween.IsComplete);

            _tweens.Start(tween);
            _tweens.Update(0.5f);

            Assert.IsTrue(Math.Abs(tween.Time - 0.5f) < Mathf.Epsilon);
        }
    }
}