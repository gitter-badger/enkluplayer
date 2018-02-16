using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.Txn
{
    [TestFixture]
    public class ElementTxnStore_Tests
    {
        private ElementTxnStore _store;

        [SetUp]
        public void Setup()
        {
            _store = new ElementTxnStore();
        }

        [Test]
        public void Apply()
        {

        }
    }
}
