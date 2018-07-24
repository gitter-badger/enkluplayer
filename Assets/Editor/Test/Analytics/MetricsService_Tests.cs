using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test.Analytics
{
    public class DummyMetricsTarget : IMetricsTarget
    {
        public class Record
        {
            public string Key;
            public float Value;
        }

        public readonly List<Record> Records = new List<Record>();

        public void Send(string key, float value)
        {
            Records.Add(new Record
            {
                Key = key,
                Value = value
            });
        }
    }

    [TestFixture]
    public class MetricsService_Tests
    {
        public const string KEY = "metric.test";

        private DummyMetricsTarget _target;
        private MetricsService _metrics;
        
        [SetUp]
        public void Setup()
        {
            _target = new DummyMetricsTarget();
            
            _metrics = new MetricsService();
            _metrics.AddTarget(_target);
        }

        [Test]
        public void Time()
        {
            var timer = _metrics.Timer(KEY);
            var id = timer.Start();

            var i = 0;
            while (i++ < int.MaxValue / 100)
            {
                // 
            }

            timer.Stop(id);

            Assert.AreEqual(KEY, _target.Records[0].Key);
        }

        [Test]
        public void TimeSync()
        {
            Assert.AreEqual(_metrics.Timer(KEY), _metrics.Timer(KEY));
        }
        
        [Test]
        public void TimeMulti()
        {
            var timer = _metrics.Timer(KEY);
            var a = timer.Start();

            var i = 0;
            while (i++ < int.MaxValue / 100)
            {
                // 
            }

            var b = timer.Start();
            timer.Stop(a);
            timer.Stop(b);

            Assert.AreEqual(2, _target.Records.Count);
            Assert.IsTrue(_target.Records[0].Value > _target.Records[1].Value);
        }
        
        [Test]
        public void CounterAdd()
        {
            var counter = _metrics.Counter(KEY);

            counter.Add(10);
            counter.Add(3);

            Assert.AreEqual(13, _target.Records.Last().Value);
        }

        [Test]
        public void CounterSubtract()
        {
            var counter = _metrics.Counter(KEY);

            counter.Subtract(10);

            Assert.AreEqual(-10, _target.Records.Last().Value);
        }
        
        [Test]
        public void CounterIncrement()
        {
            var counter = _metrics.Counter(KEY);

            counter.Increment();
            counter.Increment();

            Assert.AreEqual(2, _target.Records.Last().Value);
        }

        [Test]
        public void CounterDecrement()
        {
            var counter = _metrics.Counter(KEY);

            counter.Decrement();
            counter.Decrement();

            Assert.AreEqual(-2, _target.Records.Last().Value);
        }

        [Test]
        public void CounterCount()
        {
            var counter = _metrics.Counter(KEY);

            counter.Count(17);

            Assert.AreEqual(17, _target.Records.Last().Value);
        }

        [Test]
        public void Value()
        {
            var value = _metrics.Value(KEY);

            value.Value(14.1f);

            Assert.IsTrue(Math.Abs(14.1f - _target.Records.Last().Value) < Mathf.Epsilon);
        }
    }
}