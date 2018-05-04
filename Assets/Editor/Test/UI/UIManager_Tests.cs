using CreateAR.Commons.Unity.Async;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.UI
{
    public class DummyUIElement : IUIElement
    {
        private int _ordinal = 0;

        public uint StackId { get; set; }

        public int CreatedCalled { get; private set; }
        public int AddedCalled { get; private set; }
        public int RevealedCalled { get; private set; }
        public int CoveredCalled { get; private set; }
        public int RemovedCalled { get; private set; }

        public DummyUIElement()
        {
            CreatedCalled = AddedCalled = RevealedCalled = CoveredCalled = RemovedCalled = -1;
        }

        public void Created()
        {
            CreatedCalled = _ordinal++;
        }

        public void Added()
        {
            AddedCalled = _ordinal++;
        }

        public void Revealed()
        {
            RevealedCalled = _ordinal++;
        }

        public void Covered()
        {
            CoveredCalled = _ordinal++;
        }

        public void Removed()
        {
            RemovedCalled = _ordinal++;
        }
    }

    public class DummyElementLoader : IUIElementFactory
    {
        public IAsyncToken<IUIElement> Element(UIReference reference, uint id)
        {
            return new AsyncToken<IUIElement>(new DummyUIElement
            {
                StackId = id
            });
        }
    }

    [TestFixture]
    public class UIManager_Tests
    {
        private UIManager _ui;

        [SetUp]
        public void Setup()
        {
            _ui = new UIManager(new DummyElementLoader());
        }

        [Test]
        public void Open()
        {
            var success = false;

            uint id;
            _ui
                .Open(new UIReference(), out id)
                .OnSuccess(element =>
                {
                    success = true;

                    // check id
                    Assert.AreEqual(element.StackId, id);

                    // check lifecycke
                    var dummy = (DummyUIElement) element;
                    Assert.AreEqual(0, dummy.CreatedCalled);
                    Assert.AreEqual(1, dummy.AddedCalled);
                    Assert.AreEqual(2, dummy.RevealedCalled);
                });

            // make sure success was called
            Assert.IsTrue(success);
        }

        [Test]
        public void OpenCovered()
        {
            // Arrange
            uint _;
            IUIElement c = null;

            // Act
            _ui.Open(new UIReference(), out _).OnSuccess(el => c = el); ;
            _ui.Open(new UIReference(), out _);

            // Assert
            var dummy = (DummyUIElement) c;
            Assert.AreEqual(3, dummy.CoveredCalled);
        }

        [Test]
        public void OpenUniqueId()
        {
            uint a, b, c;
            _ui.Open(new UIReference(), out a);
            _ui.Open(new UIReference(), out b);
            _ui.Open(new UIReference(), out c);

            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a, c);
            Assert.AreNotEqual(b, c);
        }

        [Test]
        public void OpenUniqueElement()
        {
            uint _;
            IUIElement a = null, b = null;

            _ui.Open(new UIReference(), out _).OnSuccess(el => a = el);
            _ui.Open(new UIReference(), out _).OnSuccess(el => b = el);

            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void Reveal()
        {
            // Arrange
            uint aId, bId;
            IUIElement a = null, b = null;
            
            // Act
            _ui.Open(new UIReference(), out aId).OnSuccess(el => a = el);
            _ui.Open(new UIReference(), out bId).OnSuccess(el => b = el);
            _ui.Reveal(aId);
            
            // lifecycle
            var dummyA = (DummyUIElement) a;
            var dummyB = (DummyUIElement) b;
            Assert.AreEqual(4, dummyA.RevealedCalled);
            Assert.AreEqual(3, dummyB.RemovedCalled);
        }

        [Test]
        public void RevealOpen()
        {
            // Arrange
            uint aId, _;
            IUIElement a = null;

            // Act
            _ui.Open(new UIReference(), out aId).OnSuccess(el => a = el);
            _ui.Reveal(aId);

            // Assert
            var dummyA = (DummyUIElement) a;
            Assert.AreEqual(2, dummyA.RevealedCalled); // Revealed should not have been called by reveal
        }

        [Test]
        public void RevealMultiple()
        {
            // Arrange
            uint aId, _;
            IUIElement a = null;

            // Act
            _ui.Open(new UIReference(), out aId).OnSuccess(el => a = el);
            _ui.Open(new UIReference(), out _);
            _ui.Reveal(aId);
            _ui.Reveal(aId);
            _ui.Reveal(aId);

            // Assert
            var dummyA = (DummyUIElement) a;
            Assert.AreEqual(4, dummyA.RevealedCalled); // Revealed should only be called once
        }

        [Test]
        public void Close()
        {
            // Arrange
            uint aId;
            IUIElement a = null;

            // Act
            _ui.Open(new UIReference(), out aId).OnSuccess(el => a = el);
            _ui.Close(aId);

            // Assert
            Assert.AreEqual(3, ((DummyUIElement) a).RemovedCalled);
        }

        [Test]
        public void CloseMany()
        {
            // Arrange
            uint aId, _;
            IUIElement a = null, b = null, c = null;

            // Act
            _ui.Open(new UIReference(), out aId).OnSuccess(el => a = el);
            _ui.Open(new UIReference(), out _).OnSuccess(el => b = el);
            _ui.Open(new UIReference(), out _).OnSuccess(el => c = el);
            _ui.Close(aId);

            // Assert
            Assert.AreEqual(4, ((DummyUIElement) a).RemovedCalled);
            Assert.AreEqual(4, ((DummyUIElement) b).RemovedCalled);
            Assert.AreEqual(3, ((DummyUIElement) c).RemovedCalled);
        }
    }
}