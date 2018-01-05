using Jint.Unity;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Runs a script.
    /// </summary>
    public class MonoBehaviourScriptingHost : InjectableMonoBehaviour
    {
        /// <summary>
        /// Script source.
        /// </summary>
        public TextAsset Source;

        /// <summary>
        /// MonoBehaviour to run it with.
        /// </summary>
        public MonoBehaviourSpireScript Script;

        /// <summary>
        /// True iff the script should execute immediately.
        /// </summary>
        public bool ExecuteOnAwake;

        /// <summary>
        /// Dependencies.
        /// </summary>
        [Inject]
        public IScriptRequireResolver Resolver { get; set; }
        [Inject]
        public IScriptManager Scripts { get; set; }
        [Inject]
        public IScriptParser Parser { get; set; }

        /// <inheritdoc cref="MonoBehaviour"/>
        protected override void Awake()
        {
            base.Awake();

            if (ExecuteOnAwake)
            {
                Execute();
            }
        }

        /// <summary>
        /// Executes the script.
        /// </summary>
        public void Execute()
        {
            if (null != Script && null != Source)
            {
                Script.Initialize(
                    new UnityScriptingHost(Script, Resolver, Scripts),
                    new SpireScript(
                        Parser,
                        new LocalScriptLoader
                        {
                            Program = Source.text
                        },
                        new ScriptData()));
                Script.Enter();
            }
        }
    }
}