using System;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// View for moving content.
    /// </summary>
    public class MoveElementUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// The controller we're moving.
        /// </summary>
        private ElementSplashDesignController _controller;

        /// <summary>
        /// The element cast to a unity element.
        /// </summary>
        private IUnityElement _unityElement;

        /// <summary>
        /// Starting position of the element, in world space.
        /// </summary>
        private Vector3 _startingPosition;

        /// <summary>
        /// Elements.
        /// </summary>
        public FloatWidget Container
        {
            get { return (FloatWidget) Root; }
        }

        [InjectElements("..(@type==ImageWidget)")]
        public ImageWidget[] Images { get; set; }

        [InjectElements("..btn-ok")]
        public ButtonWidget BtnOk { get; set; }

        [InjectElements("..btn-cancel")]
        public ButtonWidget BtnCancel { get; set; }

        /// <summary>
        /// Called when confirmed.
        /// </summary>
        public event Action<ElementSplashDesignController> OnConfirm;

        /// <summary>
        /// Called when canceled.
        /// </summary>
        public event Action OnCancel;

        /// <summary>
        /// Initializes the controller with a piece of content.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public void Initialize(ElementSplashDesignController controller)
        {
            _controller = controller;
            _unityElement = controller.Element as IUnityElement;

            if (null != _unityElement)
            {
                _startingPosition = _unityElement.GameObject.transform.position;

                Container.GameObject.transform.position = _startingPosition;
            }
        }

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnOk.Activator.OnActivated += _ =>
            {
                if (null != OnConfirm)
                {
                    OnConfirm(_controller);
                }
            };

            BtnCancel.Activator.OnActivated += _ =>
            {
                if (null != _unityElement)
                {
                    _unityElement.GameObject.transform.position = _startingPosition;
                }

                if (null != OnCancel)
                {
                    OnCancel();
                }
            };
        }
        
        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            if (null == _unityElement)
            {
                return;
            }

            var world = Container.Content.transform.position;
            _unityElement.GameObject.transform.position = world;
        }
    }
}