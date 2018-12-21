using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Vine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <inheritdoc />
    public class VineMonoBehaviour : VineScript
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
        private VineImporter _importer;

        /// <summary>
        /// Creates elements.
        /// </summary>
        private IElementFactory _elements;

        /// <summary>
        /// The parent of the script.
        /// </summary>
        public Element Parent { get; private set; }

        /// <summary>
        /// EnkluScript.
        /// </summary>
        public EnkluScript Script { get; private set; }
        
        /// <inheritdoc />
        public override void Initialize(
            Element parent, 
            EnkluScript script, 
            VineImporter importer, 
            IElementFactory elements)
        {
            Parent = parent;
            Script = script;
            _importer = importer;
            _elements = elements;
        }

        /// <inheritdoc />
        public override IAsyncToken<Void> Configure()
        {
            Log.Info(this, "Importing Vine {0}.", Script.Data.Id);

            var token = new AsyncToken<Void>();

            _importer
                .Parse(Script.Source, Parent != null ? Parent.Schema : null)
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

        /// <inheritdoc />
        public override void Enter()
        {
            base.Enter();
            
            if (null == _description)
            {
                return;
            }

            _element = _elements.Element(_description);

            Parent.AddChild(_element);
        }

        /// <inheritdoc />
        public override void FrameUpdate()
        {
            //
        }

        /// <inheritdoc />
        public override void Exit()
        {
            base.Exit();
            
            if (null != _element)
            {
                _element.Destroy();
                _element = null;
            }
        }
    }
}
