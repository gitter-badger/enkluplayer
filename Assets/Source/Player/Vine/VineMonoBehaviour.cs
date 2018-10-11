using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Vine;

using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Creates elements from a vine.
    /// </summary>
    public class VineMonoBehaviour : InjectableMonoBehaviour
    {
        /// <summary>
        /// Parsed script.
        /// </summary>
        private ElementDescription _description;

        /// <summary>
        /// The element created.
        /// </summary>
        private Element _element;
        
        /// <summary>
        /// Imports from vine.
        /// </summary>
        [Inject]
        public VineImporter Importer { get; set; }

        /// <summary>
        /// Creates elements.
        /// </summary>
        [Inject]
        public IElementFactory Elements { get; set; }

        /// <summary>
        /// The parent of the script.
        /// </summary>
        public Element Parent { get; private set; }

        /// <summary>
        /// EnkluScript.
        /// </summary>
        public EnkluScript Script { get; private set; }
        
        /// <summary>
        /// Initializes script.
        /// </summary>
        public void Initialize(Element parent, EnkluScript script)
        {
            Parent = parent;
            Script = script;
        }

        /// <summary>
        /// Call after script is ready, before FSM flow.
        /// </summary>
        public IAsyncToken<Void> Configure()
        {
            Log.Info(this, "Importing Vine {0}.", Script.Data.Id);

            var token = new AsyncToken<Void>();
            
            // TODO: WHY IS THIS HAPPENING
            if (null == Importer)
            {
                Main.Inject(this);
            }

            Importer
                .Parse(Script.Source, Parent.Schema)
                .OnSuccess(description =>
                {
                    _description = description;

                    if (null == _description)
                    {
                        token.Fail(new Exception("Could not parse vine."));
                    }
                    else
                    {
                        token.Succeed(Void.Instance);
                    }
                });

            return token;
        }

        /// <summary>
        /// Runs script.
        /// </summary>
        public void Enter()
        {
            if (null == _description)
            {
                return;
            }

            _element = Elements.Element(_description);

            Parent.AddChild(_element);
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        public void FrameUpdate()
        {
            //
        }

        /// <summary>
        /// Destroys component and created elements.
        /// </summary>
        public void Exit()
        {
            if (null != _element)
            {
                _element.Destroy();
                _element = null;
            }
        }
    }
}