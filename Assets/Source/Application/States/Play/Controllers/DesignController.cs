using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls design mode menus.
    /// </summary>
    public class DesignController
    {
        /// <summary>
        /// Elements!
        /// </summary>
        private readonly IElementFactory _elements;
        
        /// <summary>
        /// All states.
        /// </summary>
        private readonly IDesignState[] _states;

        /// <summary>
        /// State machine.
        /// </summary>
        private readonly FiniteStateMachine _fsm;
        
        /// <summary>
        /// Root of controls.
        /// </summary>
        private GameObject _root;
        
        /// <summary>
        /// Root float.
        /// </summary>
        private FloatWidget _float;

        /// <summary>
        /// Root element of dynamic menus.
        /// </summary>
        private ScaleTransition _dynamicRoot;

        /// <summary>
        /// Root element of static menus.
        /// </summary>
        private ContainerWidget _staticRoot;
        
        /// <summary>
        /// Constuctor.
        /// </summary>
        public DesignController(
            IElementFactory elements,
            
            // design states
            MainDesignState main,
            ContentDesignState content,
            AnchorDesignState anchors)
        {
            _elements = elements;
            
            _states = new IDesignState[]
            {
                main,
                content,
                anchors
            };

            _fsm = new FiniteStateMachine(_states);
        }

        /// <summary>
        /// Starts controllers.
        /// </summary>
        public void Setup()
        {
            _root = new GameObject("Design");
            
            // create dynamic root
            {
                _float = (FloatWidget) _elements.Element(@"<?Vine><Float id='Root' position=(0, 0, 2) face='camera'><ScaleTransition /></Float>");
                _float.GameObject.transform.parent = _root.transform;
                _dynamicRoot = (ScaleTransition) _float.Children[0];
            }

            // create static root
            {
                _staticRoot = (ContainerWidget) _elements.Element(@"<?Vine><Container />");
                _staticRoot.GameObject.transform.parent = _root.transform;
            }

            // initialize states
            for (var i = 0; i < _states.Length; i++)
            {
                _states[i].Initialize(
                    this,
                    _root,
                    _float,
                    _staticRoot);
            }

            // start initial state
            _fsm.Change<MainDesignState>();
        }
        
        /// <summary>
        /// Tears down controller.
        /// </summary>
        public void Teardown()
        {
            _fsm.Change(null);

            _float.Destroy();
            _staticRoot.Destroy();

            Object.Destroy(_root);
        }

        /// <summary>
        /// Changes design state.
        /// </summary>
        /// <typeparam name="T">The type of design state.</typeparam>
        public void ChangeState<T>() where T : IDesignState
        {
            _fsm.Change<T>();
        }
    }
}