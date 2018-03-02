using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class ReparentDesignState : IDesignState
    {
        private readonly IElementControllerManager _controllers;

        private readonly DistanceElementControllerFilter _distance = new DistanceElementControllerFilter();

        private DesignController _design;

        private GameObject _unityRoot;

        public ReparentDesignState(IElementControllerManager controllers)
        {
            _controllers = controllers;
        }

        public void Initialize(
            DesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;
            _unityRoot = unityRoot;
        }

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

        public void Update(float dt)
        {
            
        }

        public void Exit()
        {
            _controllers
                .Remove<ReparentDesignController>()
                .Unfilter(_distance);

            Log.Info(this, "Exited {0}.", GetType().Name);
        }
    }
}