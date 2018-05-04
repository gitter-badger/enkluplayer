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
                .Open<DummyUIElement>(new UIReference(), out id)
                .OnSuccess(element =>
                {
                    success = true;

                    // check id
                    Assert.AreEqual(element.StackId, id);

                    // check lifecycke
                    Assert.AreEqual(0, element.CreatedCalled);
                    Assert.AreEqual(1, element.AddedCalled);
                    Assert.AreEqual(2, element.RevealedCalled);
                });

            // make sure success was called
            Assert.IsTrue(success);
        }

        [Test]
        public void OpenCovered()
        {
            // Arrange
            uint _;
            DummyUIElement c = null;

            // Act
            _ui.Open<DummyUIElement>(new UIReference(), out _).OnSuccess(el => c = el); ;
            _ui.Open<DummyUIElement>(new UIReference(), out _);

            // Assert
            Assert.AreEqual(3, c.CoveredCalled);
        }

        [Test]
        public void OpenUniqueId()
        {
            uint a, b, c;
            _ui.Open<DummyUIElement>(new UIReference(), out a);
            _ui.Open<DummyUIElement>(new UIReference(), out b);
            _ui.Open<DummyUIElement>(new UIReference(), out c);

            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a, c);
            Assert.AreNotEqual(b, c);
        }

        [Test]
        public void OpenUniqueElement()
        {
            uint _;
            DummyUIElement a = null, b = null;

            _ui.Open<DummyUIElement>(new UIReference(), out _).OnSuccess(el => a = el);
            _ui.Open<DummyUIElement>(new UIReference(), out _).OnSuccess(el => b = el);

            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void Reveal()
        {
            // Arrange
            uint aId, bId;
            DummyUIElement a = null, b = null;
            
            // Act
            _ui.Open<DummyUIElement>(new UIReference(), out aId).OnSuccess(el => a = el);
            _ui.Open<DummyUIElement>(new UIReference(), out bId).OnSuccess(el => b = el);
            _ui.Reveal(aId);
            
            // lifecycle
            Assert.AreEqual(4, a.RevealedCalled);
            Assert.AreEqual(3, b.RemovedCalled);
        }

        [Test]
        public void RevealOpen()
        {
            // Arrange
            uint aId;
            DummyUIElement a = null;

            // Act
            _ui.Open<DummyUIElement>(new UIReference(), out aId).OnSuccess(el => a = el);
            _ui.Reveal(aId);

            // Assert
            Assert.AreEqual(2, a.RevealedCalled); // Revealed should not have been called by reveal
        }

        [Test]
        public void RevealMultiple()
        {
            // Arrange
            uint aId, _;
            DummyUIElement a = null;

            // Act
            _ui.Open<DummyUIElement>(new UIReference(), out aId).OnSuccess(el => a = el);
            _ui.Open<DummyUIElement>(new UIReference(), out _);
            _ui.Reveal(aId);
            _ui.Reveal(aId);
            _ui.Reveal(aId);

            // Assert
            Assert.AreEqual(4, a.RevealedCalled); // Revealed should only be called once
        }

        [Test]
        public void Close()
        {
            // Arrange
            uint aId;
            DummyUIElement a = null;

            // Act
            _ui.Open<DummyUIElement>(new UIReference(), out aId).OnSuccess(el => a = el);
            _ui.Close(aId);

            // Assert
            Assert.AreEqual(3,  a.RemovedCalled);
        }

        [Test]
        public void CloseMany()
        {
            // Arrange
            uint aId, _;
            DummyUIElement a = null, b = null, c = null;

            // Act
            _ui.Open<DummyUIElement>(new UIReference(), out aId).OnSuccess(el => a = el);
            _ui.Open<DummyUIElement>(new UIReference(), out _).OnSuccess(el => b = el);
            _ui.Open<DummyUIElement>(new UIReference(), out _).OnSuccess(el => c = el);
            _ui.Close(aId);

            // Assert
            Assert.AreEqual(4, a.RemovedCalled);
            Assert.AreEqual(4, b.RemovedCalled);
            Assert.AreEqual(3, c.RemovedCalled);
        }
    }
}