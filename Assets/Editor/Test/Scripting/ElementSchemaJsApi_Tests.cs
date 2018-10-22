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
            _engine = new Engine(options => {
                options.CatchClrExceptions(exception => { throw exception; });
                options.AllowClr();
            });

            _element = new ElementJs(null, null, new Element());
            _engine.SetValue("this", _element);
        }

        [Test]
        public void GetSetNumber()
        {
            var output = _engine.Run(@"
                this.this.schema.setNumber('foo', 12);
                this.this.schema.getNumber('foo');
            ");

            Assert.AreEqual(12, output.AsNumber());
        }
        
        [Test]
        public void GetSetString()
        {
            var output = _engine.Run(@"
                
                this.this.schema.setString('fooString', 'bar');
                this.this.schema.getString('fooString');
            ");

            Assert.AreEqual("bar", output.AsString());
        }
        
        [Test]
        public void GetSetBool()
        {
            var output = _engine.Run(@"
                this.this.schema.setBool('fooBool', true);
                this.this.schema.getBool('fooBool')
            ");

            Assert.AreEqual(true, output.AsBoolean());
        }

        [Test]
        public void WatchUnwatch()
        {
            _engine.Run(@"
                var cachedPrev;
                var cachedNext;
                var invokeCount = 0;

                function onVisibleChanged(prev, next) {
                    invokeCount++;
                    cachedPrev = prev;
                    cachedNext = next;
                }

                this.this.schema.setBool('visible', true);
                this.this.schema.watchBool('visible', onVisibleChanged);
            ");

            Assert.AreEqual("undefined", _engine.Run("cachedPrev").ToString());
            Assert.AreEqual("undefined", _engine.Run("cachedNext").ToString());
            Assert.AreEqual(0, _engine.Run("invokeCount").AsNumber());

            _engine.Run(@"
                this.this.schema.setBool('visible', false);
            ");

            Assert.AreEqual(true, _engine.Run("cachedPrev").AsBoolean());
            Assert.AreEqual(false, _engine.Run("cachedNext").AsBoolean());
            Assert.AreEqual(1, _engine.Run("invokeCount").AsNumber());

            _engine.Run(@"
                this.this.schema.setBool('visible', true);
            ");

            Assert.AreEqual(false, _engine.Run("cachedPrev").AsBoolean());
            Assert.AreEqual(true, _engine.Run("cachedNext").AsBoolean());
            Assert.AreEqual(2, _engine.Run("invokeCount").AsNumber());

            _engine.Run(@"
                this.this.schema.unwatchBool('visible', onVisibleChanged);
                this.this.schema.setBool('visible', false);
            ");

            Assert.AreEqual(false, _engine.Run("cachedPrev").AsBoolean());
            Assert.AreEqual(true, _engine.Run("cachedNext").AsBoolean());
            Assert.AreEqual(2, _engine.Run("invokeCount").AsNumber());
        }
    }
}
