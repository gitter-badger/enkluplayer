using Assets.Source.Player.Scripting;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using Jint;
using NUnit.Framework;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class ElementSchemaJsApi_Tests
    {
        private Engine _engine;
        private ElementJs _element;

        [SetUp]
        public void Setup()
        {
            _engine = ScriptingHostFactory.NewEngine(false);

            _element = new ElementJs(null, null, new Element());
            _engine.SetValue("element", _element);
        }

        [Test]
        public void GetSetNumber()
        {
            var output = _engine.Run(@"
                element.schema.setNumber('foo', 12);
                element.schema.getNumber('foo');
            ");

            Assert.AreEqual(12, output.AsNumber());
        }
        
        [Test]
        public void GetSetString()
        {
            var output = _engine.Run(@"
                element.schema.setString('fooString', 'bar');
                element.schema.getString('fooString');
            ");

            Assert.AreEqual("bar", output.AsString());
        }
        
        [Test]
        public void GetSetBool()
        {
            var output = _engine.Run(@"
                element.schema.setBool('fooBool', true);
                element.schema.getBool('fooBool')
            ");

            Assert.AreEqual(true, output.AsBoolean());
        }

        [Test]
        public void WatchUnwatch()
        {
            // Setup test variables & watch.
            _engine.Run(@"
                var cachedPrev;
                var cachedNext;
                var invokeCount = 0;

                function onVisibleChanged(prev, next) {
                    invokeCount++;
                    cachedPrev = prev;
                    cachedNext = next;
                }

                element.schema.setBool('visible', true);
                element.schema.watchBool('visible', onVisibleChanged);
            ");

            Assert.AreEqual("undefined", _engine.Run("cachedPrev").ToString());
            Assert.AreEqual("undefined", _engine.Run("cachedNext").ToString());
            Assert.AreEqual(0, _engine.Run("invokeCount").AsNumber());

            // Change and check for updates.
            _engine.Run(@"
                element.schema.setBool('visible', false);
            ");

            Assert.AreEqual(true, _engine.Run("cachedPrev").AsBoolean());
            Assert.AreEqual(false, _engine.Run("cachedNext").AsBoolean());
            Assert.AreEqual(1, _engine.Run("invokeCount").AsNumber());

            // Change again.
            _engine.Run(@"
                element.schema.setBool('visible', true);
            ");

            Assert.AreEqual(false, _engine.Run("cachedPrev").AsBoolean());
            Assert.AreEqual(true, _engine.Run("cachedNext").AsBoolean());
            Assert.AreEqual(2, _engine.Run("invokeCount").AsNumber());

            // Unsub and ensure nothing changes.
            _engine.Run(@"
                element.schema.unwatchBool('visible', onVisibleChanged);
                element.schema.setBool('visible', false);
            ");

            Assert.AreEqual(false, _engine.Run("cachedPrev").AsBoolean());
            Assert.AreEqual(true, _engine.Run("cachedNext").AsBoolean());
            Assert.AreEqual(2, _engine.Run("invokeCount").AsNumber());
        }
    }
}
