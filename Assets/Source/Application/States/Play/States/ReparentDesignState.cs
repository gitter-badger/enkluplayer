using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State for reparenting.
    /// </summary>
    public class ReparentDesignState : IDesignState
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
        private DesignController _design;

        /// <summary>
        /// Unity root.
        /// </summary>
        private GameObject _unityRoot;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ReparentDesignState(IElementControllerManager controllers)
        {
            _controllers = controllers;
        }

        /// <inheritdoc />
        public void Initialize(
            DesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;
            _unityRoot = unityRoot;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entering {0}.", GetType().Name);

            _controllers
                .Filter(_distance)
                .Add<ReparentDesignController>(new ReparentDesignController.ReparentDesignControllerContext
                {
                    Lines = _unityRoot.GetComponent<ILineManager>()
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            _controllers
                .Remove<ReparentDesignController>()
                .Unfilter(_distance);

            Log.Info(this, "Exited {0}.", GetType().Name);
        }
    }
}