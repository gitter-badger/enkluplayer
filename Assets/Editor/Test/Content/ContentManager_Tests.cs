using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using NUnit.Framework;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer.Test
{
    public class DummyAppDataManager : IAppDataManager
    {
        public readonly List<ContentData> _content = new List<ContentData>
        {
            new ContentData
            {
                Id = "Unique",
                Unique = true
            },
            new ContentData
            {
                Id = "A"
            },
            new ContentData
            {
                Id = "B"
            }
        };

        public event Action OnLoaded;
        public event Action OnUnloaded;

        public string LoadedApp { get; private set; }

        public T Get<T>(string id) where T : StaticData
        {
            return _content.FirstOrDefault(content => content.Id == id) as T;
        }

        public T[] GetAll<T>() where T : StaticData
        {
            return _content.Cast<T>().ToArray();
        }

        public T GetByName<T>(string name) where T : StaticData
        {
            throw new NotImplementedException();
        }

        public IAsyncToken<Void> Load(string id)
        {
            throw new NotImplementedException();
        }

        public IAsyncToken<Void> Unload()
        {
            throw new NotImplementedException();
        }
    }

    [Ignore("Requires creating Content, which is a MonoBehaviour. Run in playmode.")]
    [TestFixture]
    public class ContentManager_Tests
    {
        private ContentManager _content;

        [SetUp]
        public void Setup()
        {
            _content = new ContentManager(
                new DummyContentFactory(),
                new DummyAppDataManager());
        }
        
        [Test]
        public void RequestShared()
        {
            var a = _content.Request("A");
            var b = _content.Request("A");

            Assert.AreSame(a, b);
        }

        [Test]
        public void RequestUnique()
        {
            var a = _content.Request("Unique");
            var b = _content.Request("Unique");

            Assert.AreNotSame(a, b);
        }

        [Test]
        public void FindAllUnique()
        {
            var a = _content.Request("Unique");
            var b = _content.Request("Unique");

            var content = new List<Content>();
            _content.FindAll("Unique", content);

            Assert.AreEqual(2, content.Count);
            Assert.AreSame(a, content[0]);
            Assert.AreSame(b, content[1]);
        }

        [Test]
        public void FindAllShared()
        {
            var a = _content.Request("A");
            var b = _content.Request("A");

            var content = new List<Content>();
            _content.FindAll("A", content);

            Assert.AreEqual(1, content.Count);
            Assert.AreSame(a, content[0]);
            Assert.AreSame(b, content[0]);
        }

        [Test]
        public void FindShared()
        {
            var a = _content.Request("A");
            var b = _content.Request("A");

            var find = _content.FindShared("A");

            Assert.AreSame(a, find);
            Assert.AreSame(b, find);
        }

        [Test]
        public void ReleaseUnique()
        {
            var a = _content.Request("Unique", "Tag");
            
            _content.Release(a);

            var content = new List<Content>();
            _content.FindAll("Unique", content);

            Assert.AreEqual(0, content.Count);
        }

        [Test]
        public void ReleaseShared()
        {
            var a = _content.Request("A", "Tag");

            _content.Release(a);

            var content = new List<Content>();
            _content.FindAll("A", content);

            Assert.AreEqual(1, content.Count);
            Assert.AreSame(a, content[0]);
        }

        [Test]
        public void ReleaseWithTagsEmpty()
        {
            var a = _content.Request("A", "Tag1");
            var b = _content.Request("B", "Tag1", "Tag2");
            var c = _content.Request("Unique", "Tag1", "Tag2");

            _content.ReleaseAll("Foo");

            var content = new List<Content>();
            _content.FindAll("Unique", content);

            Assert.AreSame(c, content[0]);

            var find = _content.FindShared("A");
            Assert.AreEqual(a, find);

            find = _content.FindShared("B");
            Assert.AreEqual(b, find);
        }

        [Test]
        public void ReleaseWithTags()
        {
            _content.Request("A", "Tag1");
            var b = _content.Request("B", "Tag1", "Tag2");
            var c = _content.Request("Unique", "Tag1", "Tag2");

            _content.ReleaseAll("Tag1");

            var content = new List<Content>();
            _content.FindAll("Unique", content);

            Assert.AreSame(c, content[0]);

            var find = _content.FindShared("A");
            Assert.IsNull(find);

            find = _content.FindShared("B");
            Assert.AreEqual(b, find);

            _content.ReleaseAll("Tag2");

            content.Clear();
            _content.FindAll("Unique", content);

            Assert.AreEqual(0, content.Count);
            Assert.IsNull(_content.FindShared("B"));
        }

        [Test]
        public void MultiReleaseException()
        {
            var a = _content.Request("Unique");

            _content.Release(a);

            Assert.Throws<NullReferenceException>(() =>
            {
                _content.Release(a);
            });
        }
    }
}