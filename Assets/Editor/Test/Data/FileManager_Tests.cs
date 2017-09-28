using System;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class FileManager_Tests
    {
        public class Test
        {
            public string Foo = Guid.NewGuid().ToString();
        }

        private FileManager _files;

        [SetUp]
        public void Setup()
        {
            var fileSystem = new MemoryFileSystem();
            fileSystem.Set(new File<byte[]>(
                Uri("A"),
                new byte[0]));

            _files = new FileManager();
            _files.Configure(
                "memory://",
                new JsonSerializer(),
                fileSystem);
        }

        [Test]
        public void ProtocolBadReject()
        {
            Assert.Throws<ArgumentException>(() => _files.Configure(
                "test",
                new JsonSerializer(),
                new NullFileSystem()));

            Assert.Throws<ArgumentException>(() => _files.Configure(
                "test:",
                new JsonSerializer(),
                new NullFileSystem()));

            Assert.Throws<ArgumentException>(() => _files.Configure(
                "test://a",
                new JsonSerializer(),
                new NullFileSystem()));
        }

        [Test]
        public void ProtocolGood()
        {
            Assert.DoesNotThrow(() => _files.Configure(
                "null://",
                new JsonSerializer(),
                new NullFileSystem()));
        }

        [Test]
        public void TestGetNothing()
        {
            var failureCalled = false;

            _files
                .Get<Test>(Uri("test"))
                .OnFailure(_ => failureCalled = true);

            Assert.IsTrue(failureCalled);
        }

        [Test]
        public void TestGetSomething()
        {
            var successCalled = false;

            _files
                .Get<Test>(Uri("A"))
                .OnSuccess(file =>
                {
                    successCalled = true;
                });

            Assert.IsTrue(successCalled);
        }

        [Test]
        public void TestSet()
        {
            var successCalled = false;
            
            var uri = Uri("test");
            var data = new Test();

            _files
                .Set(new File<Test>(
                    uri,
                    data))
                .OnSuccess(file =>
                {
                    Assert.AreEqual(uri, file.Uri);
                    Assert.AreSame(data, file.Data);

                    successCalled = true;
                });

            Assert.IsTrue(successCalled);
        }

        private string Uri(string endpoint)
        {
            return "memory://" + endpoint;
        }
    }
}