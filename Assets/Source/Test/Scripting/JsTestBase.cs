using Assets.Source.Player.Scripting;
using CreateAR.EnkluPlayer.IUX;
using Jint;
using Jint.Native;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [RuntimeTestFixture]
    public class JsTestBase
    {
        protected Engine _engine;
        
        [Inject]
        public IElementManager Elements { get; set; }
        
        [Inject]
        public IElementFactory ElementFactory { get; set; }

        [RuntimeSetUpFixture]
        public virtual void SetUp()
        {
            Main.Inject(this);

            _engine = new Engine(options =>
            {
                options.AllowClr();
                options.CatchClrExceptions(exception =>
                {
                    throw exception;
                });

                // Debugging Configuration
                options.DebugMode(false);
                options.AllowDebuggerStatement(false);
            });

            _engine.SetValue("assert", RuntimeAssertJsApi.Instance);
            _engine.SetValue("log", new LogJsApi(this));
        }
        
        protected JsValue Run(string program)
        {
            return _engine.GetValue(_engine.Execute(program).GetCompletionValue());
        }

        protected T Run<T>(string program)
        {
            return _engine.GetValue(_engine.Execute(program).GetCompletionValue()).To<T>();
        }
    }
}