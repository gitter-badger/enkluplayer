using System;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Util;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Util.Tween
{
    [TestFixture]
    public class Tween_Tests
    {
        private ElementSchema _schema;

        [SetUp]
        public void Setup()
        {
            _schema = new ElementSchema();
        }
        
        [Test]
        public void FloatTweenTo()
        {
            const float to = 0.5f;

            var tween = new FloatTween(_schema, new TweenData
            {
                Prop = "foo",
                To = to,
                DurationSec = 0.1f
            });

            Assert.IsTrue(_schema.Get<float>("foo").Value < Mathf.Epsilon);

            tween.Time = 1;

            Assert.IsTrue(Math.Abs(to - _schema.Get<float>("foo").Value) < Mathf.Epsilon);
        }

        [Test]
        public void FloatTweenFromTo()
        {
            const float to = 0.5f;
            const float from = 50f;

            var tween = new FloatTween(_schema, new TweenData
            {
                Prop = "foo",
                To = to,
                From = from,
                CustomFrom = true,
                DurationSec = 0.1f
            });
            
            tween.Time = 0;

            Assert.IsTrue(Math.Abs(from - _schema.Get<float>("foo").Value) < Mathf.Epsilon);

            tween.Time = 1;

            Assert.IsTrue(Math.Abs(to - _schema.Get<float>("foo").Value) < Mathf.Epsilon);
        }

        [Test]
        public void FloatTweenToLinear()
        {
            const float to = 0.5f;

            var tween = new FloatTween(_schema, new TweenData
            {
                Prop = "foo",
                To = to,
                DurationSec = 1f
            });

            Assert.IsTrue(_schema.Get<float>("foo").Value < Mathf.Epsilon);

            tween.Time = 0.5f;

            Assert.IsTrue(Math.Abs(to / 2f - _schema.Get<float>("foo").Value) < Mathf.Epsilon);
        }

        [Test]
        public void FloatTweenDelay()
        {
            const float to = 0.5f;

            var tween = new FloatTween(_schema, new TweenData
            {
                Prop = "foo",
                To = to,
                DurationSec = 1f,
                DelaySec = 0.5f
            });
            
            tween.Time = 0.5f;

            // ensure it hasn't started yet
            Assert.IsTrue(_schema.Get<float>("foo").Value < Mathf.Epsilon);

            tween.Time = 1.5f;

            // ensure it completes successfully with aggregated time
            Assert.IsTrue(Math.Abs(to - _schema.Get<float>("foo").Value) < Mathf.Epsilon);
        }

        [Test]
        public void FloatTweenOnComplete()
        {
            var called = false;
            var tween = new FloatTween(_schema, new TweenData
            {
                Prop = "foo",
                To = 1f,
                DurationSec = 1f
            });
            tween.OnComplete += () => called = true;


            tween.Time = 1;

            Assert.IsTrue(called);
        }

        [Test]
        public void FloatTweenOnStart()
        {
            var called = false;
            var tween = new FloatTween(_schema, new TweenData
            {
                Prop = "foo",
                To = 1f,
                DurationSec = 1f
            });
            tween.OnStart += () => called = true;

            Assert.IsFalse(called);

            tween.Time = 0.1f;

            Assert.IsTrue(called);
        }

        [Test]
        public void FloatTweenOnStartZero()
        {
            var called = false;
            var tween = new FloatTween(_schema, new TweenData
            {
                Prop = "foo",
                To = 1f,
                DurationSec = 1f
            });
            tween.OnStart += () => called = true;

            Assert.IsFalse(called);

            tween.Time = 0f;

            Assert.IsTrue(called);
        }

        [Test]
        public void FloatTweenOnStartNoMulti()
        {
            var called = 0;
            var tween = new FloatTween(_schema, new TweenData
            {
                Prop = "foo",
                To = 1f,
                DurationSec = 1f
            });
            tween.OnStart += () => called++;

            tween.Time = 0.1f;
            tween.Time = 0.2f;
            tween.Time = 0.3f;

            Assert.AreEqual(1, called);
        }

        [Test]
        public void FloatTweenOnStartRewindMulti()
        {
            var called = 0;
            var tween = new FloatTween(_schema, new TweenData
            {
                Prop = "foo",
                To = 1f,
                DurationSec = 1f
            });
            tween.OnStart += () => called++;

            tween.Time = 0.1f;
            tween.Time = 0f;
            tween.Time = 0.3f;

            Assert.AreEqual(2, called);
        }

        [Test]
        public void FloatTweenOnCompleteNoMulti()
        {
            var called = 0;
            var tween = new FloatTween(_schema, new TweenData
            {
                Prop = "foo",
                To = 1f,
                DurationSec = 1f
            });
            tween.OnComplete += () => called++;

            tween.Time = 1;
            tween.Time = 1;
            tween.Time = 1;

            Assert.AreEqual(1, called);
        }

        [Test]
        public void FloatTweenOnCompleteReverseMulti()
        {
            var called = 0;
            var tween = new FloatTween(_schema, new TweenData
            {
                Prop = "foo",
                To = 1f,
                DurationSec = 1f
            });
            tween.OnComplete += () => called++;

            tween.Time = 1;
            tween.Time = 0.9f;
            tween.Time = 1;

            Assert.AreEqual(2, called);
        }
    }
}