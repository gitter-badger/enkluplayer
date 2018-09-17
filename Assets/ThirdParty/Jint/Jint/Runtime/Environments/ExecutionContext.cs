using CreateAR.EnkluPlayer.DataStructures;
using Jint.Native;

namespace Jint.Runtime.Environments
{
    public sealed class ExecutionContext : IOptimizedObjectPoolElement
    {
        private LexicalEnvironment _lexicalEnvironment;
        private LexicalEnvironment _variableEnvironment;

        public LexicalEnvironment LexicalEnvironment
        {
            get { return _lexicalEnvironment; }
            set
            {
                if (null != _lexicalEnvironment)
                {
                    Engine.PoolLexicalEnvironment.Put(_lexicalEnvironment);
                }

                _lexicalEnvironment = value;
            }
        }

        public LexicalEnvironment VariableEnvironment
        {
            get { return _variableEnvironment; }
            set
            {
                if (null != _variableEnvironment)
                {
                    Engine.PoolLexicalEnvironment.Put(_variableEnvironment);
                }

                _variableEnvironment = value;
            }
        }

        public JsValue ThisBinding { get; set; }

        public int Index { get; set; }
        public bool Available { get; set; }
    }
}
