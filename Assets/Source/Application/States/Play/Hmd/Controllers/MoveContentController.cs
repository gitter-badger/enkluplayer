using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// View for moving content.
    /// </summary>
    [InjectVine("Design.MoveContent")]
    public class MoveContentController : InjectableIUXController
    {
        /// <summary>
        /// The controller we're moving.
        /// </summary>
        private ContentDesignController _controller;

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
        public event Action<ContentDesignController> OnConfirm;

        /// <summary>
        /// Called when canceled.
        /// </summary>
        public event Action OnCancel;

        /// <summary>
        /// Initializes the controller with a piece of content.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public void Initialize(ContentDesignController controller)
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
        protected override void Awake()
        {
            base.Awake();

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

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();

            _controller = null;
            _unityElement = null;
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