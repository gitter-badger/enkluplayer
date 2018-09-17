using System.Collections.Generic;
using CreateAR.EnkluPlayer.DataStructures;
using NUnit.Framework;

namespace CreateAR.EnkluPlayer.Test.DataStructures
{
    [TestFixture]
    public class OptimizedObjectPool_Tests
    {
        public class DummyObject : IOptimizedObjectPoolElement
        {
            private static int IDS = 0;

            public int Index { get; set; }

            public readonly int Id = IDS++;
        }

        [Test]
        public void AllocateMin()
        {
            const int min = 10;
            var num = 0;

            var pool = new OptimizedObjectPool<DummyObject>(min, 0, 10, () =>
            {
                num++;

                return new DummyObject();
            });

            Assert.AreEqual(min, num);
            Assert.AreEqual(min, pool.Size);
        }

        [Test]
        public void AllocateGrowSize()
        {
            var pool = new OptimizedObjectPool<DummyObject>(1, 0, 3, () => new DummyObject());

            pool.Get();

            Assert.AreEqual(1, pool.Size);

            pool.Get();

            Assert.AreEqual(4, pool.Size);
        }

        [Test]
        public void AllocateNoGrow()
        {
            var pool = new OptimizedObjectPool<DummyObject>(1, 10, 0, () => new DummyObject());

            pool.Get();

            Assert.IsNull(pool.Get());
            Assert.AreEqual(1, pool.Size);
        }

        [Test]
        public void AllocateMax()
        {
            var pool = new OptimizedObjectPool<DummyObject>(2, 2, 0, () => new DummyObject());

            Assert.IsNotNull(pool.Get());
            Assert.IsNotNull(pool.Get());
            Assert.IsNull(pool.Get());
        }

        [Test]
        public void AllocateGrowToMax()
        {
            var pool = new OptimizedObjectPool<DummyObject>(1, 2, 1, () => new DummyObject());

            Assert.IsNotNull(pool.Get());
            Assert.IsNotNull(pool.Get());
            Assert.IsNull(pool.Get());
        }

        [Test]
        public void AllocateGrowOverMax()
        {
            var pool = new OptimizedObjectPool<DummyObject>(1, 2, 10, () => new DummyObject());

            Assert.IsNotNull(pool.Get());
            Assert.IsNotNull(pool.Get());
            Assert.IsNull(pool.Get());
        }

        [Test]
        public void GetDifferent()
        {
            var instances = new List<int>();
            var pool = new OptimizedObjectPool<DummyObject>(1, 0, 4, () => new DummyObject());

            for (var i = 0; i < 10; i++)
            {
                var instance = pool.Get();
                Assert.IsFalse(instances.Contains(instance.Id));

                instances.Add(instance.Id);
            }
        }

        [Test]
        public void GetPut()
        {
            var pool = new OptimizedObjectPool<DummyObject>(1, 0, 0, () => new DummyObject());

            var instance = pool.Get();
            Assert.IsNotNull(instance);
            Assert.Null(pool.Get());
            pool.Put(instance);

            instance = pool.Get();
            Assert.IsNotNull(instance);
            Assert.Null(pool.Get());
            pool.Put(instance);
        }
    }
}