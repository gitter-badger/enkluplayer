using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer.Test.Scripting
{
    public class ElementSchemaJsApi_Tests : JsTestBase
    {
        private ElementJs _element;
        private AppElementsJsApi _elementsApi;
        
        [RuntimeSetUpFixture]
        public override void SetUp()
        {
            base.SetUp();
            
            var cache = new ElementJsCache(_engine);
            _elementsApi = new AppElementsJsApi(cache, ElementFactory, Elements);
            _engine.SetValue("elements", _elementsApi);

            _element = _elementsApi.create("Container", "a");
        }

        [RuntimeTest]
        public void GetSetNumber()
        {
            Run(@"
                var a = elements.byId('a');
                a.schema.setNumber('foo', 12);
                assert.areEqual(
                    12,
                    a.schema.getNumber('foo'),
                    'Get/set didn\'t work.');");
        }
        
        [RuntimeTest]
        public void GetSetString()
        {
            Run(@"
                var a = elements.byId('a');
                a.schema.setString('fooString', 'bar');
                assert.areEqual(
                    'bar',
                    a.schema.getString('fooString'),
                    'Get/set didn\'t work.');");
        }
        
        [RuntimeTest]
        public void GetSetBool()
        {
            Run(@"
                var a = elements.byId('a');
                a.schema.setBool('fooBool', true);
                assert.areEqual(
                    true,
                    a.schema.getBool('fooBool'),
                    'Get/set didn\'t work.');");
        }

        [RuntimeTest]
        public void WatchUnwatch()
        {
            Run(@"
                var called = false;
                var a = elements.byId('a');
                a.schema.setNumber('foo', 12);

                function fooHandler(prev, next) {
                    called = true;

                    assert.areEqual(12, prev, 'Previous value is incorrect.');
                    assert.areEqual(5, next, 'Next value is incorrect.');
                }

                a.schema.watchNumber('foo', fooHandler);
                a.schema.setNumber('foo', 5);

                assert.isTrue(called, 'Callback was not called.');
                
                a.schema.unwatchNumber('foo', fooHandler);
    
                a.schema.setNumber('foo', 75);
");
        }
    }
}