using CreateAR.SpirePlayer.UI;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class ElementState_Tests
    {
        private ElementState _state;

        [SetUp]
        public void SetUp()
        {
            _state = new ElementState();
            _state.Set("foo", 5);
        }
        
        [Test]
        public void GetPropInt()
        {
            Assert.AreEqual(5, _state.Get<int>("foo").Value);
        }

        [Test]
        public void SetPropInt()
        {
            var prop = _state.Get<int>("foo");
            Assert.AreEqual(5, prop.Value);

            _state.Set("foo", 12);

            Assert.AreEqual(12, prop.Value);
        }

        /// <summary>
        /// Demonstrates that once set, the type cannot be changed.
        /// </summary>
        [Test]
        public void GetPropChangeType()
        {
            Assert.AreEqual(false, _state.Get<bool>("foo").Value);

            _state.Set("foo", true);

            Assert.AreEqual(false, _state.Get<bool>("foo").Value);
        }

        [Test]
        public void WrapInt()
        {
            var state = new ElementState();
            state.Set("bar", 17);

            _state.Wrap(state);

            Assert.AreEqual(17, _state.Get<int>("bar").Value);
        }

        [Test]
        public void WrapIntSet()
        {
            var state = new ElementState();
            state.Set("bar", 17);

            _state.Wrap(state);
            _state.Set("bar", 4);

            Assert.AreEqual(4, _state.Get<int>("bar").Value);
            Assert.AreEqual(17, state.Get<int>("bar").Value);
        }
    }
}