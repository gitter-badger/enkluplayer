using CreateAR.SpirePlayer.Vine;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.Vine
{
    public class VinePreprocessor_Tests
    {
        private JsVinePreProcessor _preProcessor;

        [SetUp]
        public void Setup()
        {
            _preProcessor = new JsVinePreProcessor();
        }

        [Test]
        public void Script()
        {
            var processed = _preProcessor.Execute("{{return 'foo';}}");

            Assert.AreEqual("foo", processed);
        }

        [Test]
        public void Script_Loop()
        {
            var processed = _preProcessor.Execute(
"{{ var nums = []; for (var i = 0, len = 10; i < len; i++) nums.push(i); return nums.join(''); }}");

            Assert.AreEqual("0123456789", processed);
        }
    }
}