using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Test.UI;
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
            var factory = new DummyElementFactory();
            var root = factory.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = "root"
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = "root"
                    }
                }
            });

            _store = new ElementTxnStore(new ElementActionStrategy(
                factory,
                root));
        }

        [Test]
        public void Apply()
        {

        }
    }
}
