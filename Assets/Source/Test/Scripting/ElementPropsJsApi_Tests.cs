namespace CreateAR.SpirePlayer.Test.Scripting
{
    public class ElementPropsJsApi_Tests : JsTestBase
    {
        private AppElementsJsApi _elementsApi;
        
        [RuntimeSetUpFixture]
        public override void SetUp()
        {
            base.SetUp();
            
            var cache = new ElementJsCache(_engine);
            _elementsApi = new AppElementsJsApi(cache, ElementFactory, Elements);
            _engine.SetValue("elements", _elementsApi);
        }
    }
}