using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.Util
{
    public class StandardQueryResolver_Tests
    {
        private readonly StandardQueryResolver _resolver = new StandardQueryResolver();
        
        private string[] _tags =
        {
            "a",
            "b",
            "c",
            "d",
            "e"
        };
        
        [Test]
        public void MatchEmpty()
        {
            Assert.IsTrue(
                _resolver.Resolve(
                    string.Empty,
                    ref _tags),
                "Should match empty query.");
            
            Assert.IsTrue(
                _resolver.Resolve(
                    null,
                    ref _tags),
                "Should match empty query.");
        }

        [Test]
        public void NoMatchNullTags()
        {
            string[] tags = null;

            Assert.IsFalse(
                _resolver.Resolve(
                    "query",
                    ref tags));   
        }

        [Test]
        public void MatchNullPrecedence()
        {
            string[] tags = null;

            Assert.IsTrue(
                _resolver.Resolve(
                    null,
                    ref tags));
        }

        [Test]
        public void MatchWhitespace()
        {
            var query = "   ";

            Assert.IsTrue(
                _resolver.Resolve(
                    query,
                    ref _tags),
                "Should match single query.");
        }

        [Test]
        public void MatchNonQuery()
        {
            var query = " !! !!!!";

            Assert.IsFalse(
                _resolver.Resolve(
                    query,
                    ref _tags),
                "Should match single query.");
        }

        [Test]
        public void MatchSingle()
        {
            var query = "a";

            Assert.IsTrue(
                _resolver.Resolve(
                    query,
                    ref _tags),
                "Should match single query.");
        }
        
        [Test]
        public void MatchAnd()
        {
            var query = "a,b";

            Assert.IsTrue(
                _resolver.Resolve(
                    query,
                    ref _tags),
                "Should match and query.");
        }
        
        [Test]
        public void MatchAndFail()
        {
            var query = "a,z";

            Assert.IsFalse(
                _resolver.Resolve(
                    query,
                    ref _tags),
                "Should not match and query.");
        }
        
        [Test]
        public void MatchOr()
        {
            var query = "z a";

            Assert.IsTrue(
                _resolver.Resolve(
                    query,
                    ref _tags),
                "Should match or query.");
        }
        
        [Test]
        public void MatchNot()
        {
            var query = "!z";

            Assert.IsTrue(
                _resolver.Resolve(
                    query,
                    ref _tags),
                "Should match not query.");
        }

        [Test]
        public void MatchDoubleNot()
        {
            var query = "!!a";

            Assert.IsTrue(
                _resolver.Resolve(
                    query,
                    ref _tags),
                "Should match double not query.");
        }

        [Test]
        public void MatchComplex()
        {
            var query = "a q,b,!z";

            Assert.IsTrue(
                _resolver.Resolve(
                    query,
                    ref _tags),
                "Should match complex query.");
        }

        [Test]
        public void MatchComplexWhitespace()
        {
            var query = "a q, b   ,  !z ";

            Assert.IsTrue(
                _resolver.Resolve(
                    query,
                    ref _tags),
                "Should match complex query.");
        }
    }
}