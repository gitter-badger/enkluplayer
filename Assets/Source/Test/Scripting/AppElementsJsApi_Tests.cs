#if UNITY_EDITOR

using CreateAR.SpirePlayer.IUX;
using Jint;
using Jint.Native;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.Scripting
{
    [RuntimeTestFixture]
    public class AppElementsJsApi_Tests
    {
        private Engine _engine;
        private AppElementsJsApi _elementsApi;
        
        [Inject]
        public IElementManager Elements { get; set; }
        
        [Inject]
        public IElementFactory ElementFactory { get; set; }

        [RuntimeSetUpFixture]
        public void SetUp()
        {
            Main.Inject(this);
            
            _engine = new Engine(options =>
            {
                options.CatchClrExceptions(exception => { throw exception; });
                options.AllowClr();
            });

            _elementsApi = new AppElementsJsApi(ElementFactory, Elements, _engine);
            _engine.SetValue("elements", _elementsApi);
        }

        [RuntimeTest]
        public void CreateAndFindElements()
        {
            var element = Run<ElementJs>("elements.create('Cursor')");
            RuntimeAssert.IsTrue(element != null, "Cursor was null.");
            RuntimeAssert.AreEqual("Cursor", element.type, "Element was not of type Cursor.");

            var find = Run<ElementJs>(string.Format(
                "elements.byId('{0}')",
                element.id));

            RuntimeAssert.AreEqual(element, find, "Found element was different than created element.");
            
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
        
        private JsValue Run(string program)
        {
            return _engine.GetValue(_engine.Execute(program).GetCompletionValue());
        }

        private T Run<T>(string program)
        {
            return _engine.GetValue(_engine.Execute(program).GetCompletionValue()).To<T>();
        }
    }
}

#endif