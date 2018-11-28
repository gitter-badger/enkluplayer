using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Util;
using NUnit.Framework;

namespace CreateAR.EnkluPlayer.Test.Util
{
    [TestFixture]
    public class TweenManager_Tests
    {
        private TweenManager _tweens;
        private Element _el;

        [SetUp]
        public void Setup()
        {
            _tweens = new TweenManager();
            _el = new Element();
        }

        [Test]
        public void CreateFloatTween()
        {
            
        }
    }
}