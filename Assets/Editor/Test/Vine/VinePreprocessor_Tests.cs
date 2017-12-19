using CreateAR.SpirePlayer.Vine;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.Vine
{
    public class VinePreprocessor_Tests
    {
        private VinePreprocessor _preprocessor;

        [SetUp]
        public void Setup()
        {
            _preprocessor = new VinePreprocessor();
        }

        [Test]
        public void Script()
        {
            var processed = _preprocessor.Execute("{{return 'foo';}}");

            Assert.AreEqual("foo", processed);
        }

        [Test]
        public void Script_Loop()
        {
            var processed = _preprocessor.Execute(
"{{ var nums = []; for (var i = 0, len = 10; i < len; i++) nums.push(i); return nums.join(''); }}");

            Assert.AreEqual("0123456789", processed);
        }
    }
}