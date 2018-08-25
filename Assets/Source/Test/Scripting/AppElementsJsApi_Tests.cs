#if UNITY_EDITOR

using CreateAR.SpirePlayer.Scripting;

namespace CreateAR.SpirePlayer.Test.Scripting
{
    public class AppElementsJsApi_Tests : JsTestBase
    {
        private AppElementsJsApi _elementsApi;
        
        [RuntimeSetUpFixture]
        public override void SetUp()
        {
            base.SetUp();
            
            var cache = new ElementJsCache(null, _engine);
            _elementsApi = new AppElementsJsApi(cache, ElementFactory, Elements);
            _engine.SetValue("elements", _elementsApi);
        }

        [RuntimeTest]
        public void Create()
        {
            var element = Run<ElementJs>("elements.create('Cursor')");
            RuntimeAssert.IsTrue(element != null, "Cursor was null.");
            RuntimeAssert.AreEqual("Cursor", element.type, "Element was not of type Cursor.");
   
            // cleanup
            element.destroy();
        }
        
        [RuntimeTest]
        public void CreateWithId()
        {
            var element = Run<ElementJs>("elements.create('Cursor', 'cursor')");
            
            RuntimeAssert.AreEqual("cursor", element.id, "Element was created with wrong id.");
            
            // cleanup
            element.destroy();
        }
        
        [RuntimeTest]
        public void Find()
        {
            ElementFactory.Element(@"
                <?Vine>
                <Container id='root'>
                    <Container id='a' />
                    <Container id='b' />
                    <Container id='c' />
                </Container>");
            var el = Run<ElementJs>(@"elements.byId('c')");
            
            RuntimeAssert.AreEqual("c", el.id, "Could not find element c!");
        }

        [RuntimeTest]
        public void AddChild()
        {
            Run(@"
                var d = elements.create('Container', 'd');
                var c = elements.byId('c');
                c.addChild(d);");
            
            RuntimeAssert.AreEqual("d", Elements.ById("c").Children[0].Id, "Add child failed.");
        }
        
        [RuntimeTest]
        public void RemoveChild()
        {
            Run(@"
                var d = elements.byId('d');
                var c = elements.byId('c');
                c.removeChild(d);
                
                var children = c.children;
                for (var i = 0, len = children.length; i < len; i++) {
                    if (children[i].id == 'd') {
                        assert.isTrue(false, 'Child was not removed.');
                    }
                }");
        }
        
        [RuntimeTest]
        public void IterateChildren()
        {

            Run(@"
                var root = elements.byId('root');
                var children = root.children;

                assert.areEqual(3, children.length, 'Children mismatch!');
                assert.areEqual('a', children[0].id, 'A should be child 0.');
                assert.areEqual('b', children[1].id, 'B should be child 1.');
                assert.areEqual('c', children[2].id, 'C should be child 2.');");
        }
        
        [RuntimeTest]
        public void Destroy()
        {
            Run(@"
                var d = elements.byId('d');
                d.destroy();
                
                assert.isNull(elements.byId('d'), 'Element was not destroyed.');");
        }
    }
}

#endif