using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State for reparenting.
    /// </summary>
    public class ReparentDesignState : IArDesignState
    {
        /// <summary>
        /// Manages controllers.
        /// </summary>
        private readonly IElementControllerManager _controllers;

        /// <summary>
        /// Filter.
        /// </summary>
        private readonly DistanceElementControllerFilter _distance = new DistanceElementControllerFilter();

        /// <summary>
        /// Designer.
        /// </summary>
        private ArDesignController _design;

        /// <summary>
        /// Unity root.
        /// </summary>
        private GameObject _unityRoot;

        /// <summary>
        /// Content we are reparenting.
        /// </summary>
        private ContentDesignController _content;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ReparentDesignState(IElementControllerManager controllers)
        {
            _controllers = controllers;
        }

        /// <inheritdoc />
        public void Initialize(
            ArDesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;
            _unityRoot = unityRoot;
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            //
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entering {0}.", GetType().Name);

            _content = (ContentDesignController) context;

            var reparentContext = new ReparentDesignController.ReparentDesignControllerContext
            {
                Content = _content,
                Lines = _unityRoot.GetComponent<ILineManager>(),
                Reparent = Reparent,
                Cancel = Cancel

            };

            _controllers
                .Filter(_distance)
                .Add<ReparentDesignController>(reparentContext);

            reparentContext.Lines.IsEnabled = true;
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            _unityRoot.GetComponent<ILineManager>().IsEnabled = false;

            _controllers
                .Remove<ReparentDesignController>()
                .Unfilter(_distance);

            Log.Info(this, "Exited {0}.", GetType().Name);
        }

        /// <summary>
        /// Called to reparent an element.
        /// </summary>
        /// <param name="parent">The new parent.</param>
        private void Reparent(Element parent)
        {
            _design
                .Elements
                .Reparent(_content.Element, parent)
                .OnSuccess(_ => _design.ChangeState<MainDesignState>())
                .OnFailure(exception => Log.Error(
                    this,
                    "Could not reparent element : {0}",
                    exception));
        }

        /// <summary>
        /// Cancels reparenting.
        /// </summary>
        private void Cancel()
        {
            _design.ChangeState<MainDesignState>();
        }
    }
}