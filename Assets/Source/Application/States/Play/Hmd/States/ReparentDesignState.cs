using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// State for reparenting.
    /// </summary>
    public class ReparentDesignState : IArDesignState
    {
        /// <summary>
        /// Controller group tag for reparent.
        /// </summary>
        private const string TAG_REPARENT = "reparent";

        /// <summary>
        /// Manages controllers.
        /// </summary>
        private readonly IElementControllerManager _controllers;
        
        /// <summary>
        /// Designer.
        /// </summary>
        private HmdDesignController _design;
        
        /// <summary>
        /// Content we are reparenting.
        /// </summary>
        private ElementSplashDesignController _content;

        /// <summary>
        /// Renders hierarchy.
        /// </summary>
        private HierarchyLineRenderer _lines;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ReparentDesignState(IElementControllerManager controllers)
        {
            _controllers = controllers;
        }

        /// <inheritdoc />
        public void Initialize(
            HmdDesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;
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

            _content = (ElementSplashDesignController) context;

            var reparentContext = new ReparentDesignController.ReparentDesignControllerContext
            {
                Content = _content,
                Reparent = Reparent,
                Cancel = Cancel
            };

            _controllers
                .Group(TAG_REPARENT)
                .Filter(new TypeElementControllerFilter(typeof(IUnityElement)))
                .Add<ReparentDesignController>(reparentContext);

            _lines = Camera.main.gameObject.GetComponent<HierarchyLineRenderer>();
            if (null != _lines)
            {
                _lines.Selected = _content.Element;
            }
            else
            {
                Log.Warning(this, "Could not find HierarchyLineRenderer.");
            }

            _controllers.Activate(TAG_REPARENT);
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            if (null != _lines)
            {
                _lines.Selected = null;
            }

            _controllers.Deactivate(TAG_REPARENT);

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