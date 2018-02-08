using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    [InjectVine("Design.PlaceItem")]
    public class PlaceObjectController : InjectableIUXController
    {
        private PropController _controller;

        public FloatWidget Container
        {
            get { return (FloatWidget) Root; }
        }

        [InjectElements("..(@type==ImageWidget)")]
        public ImageWidget[] Images { get; private set; }

        [InjectElements("..btn-ok")]
        public ButtonWidget BtnOk { get; private set; }

        [InjectElements("..btn-cancel")]
        public ButtonWidget BtnCancel { get; private set; }

        [InjectElements("..(@type==ContentWidget)")]
        public ContentWidget Content { get; private set; }

        [InjectElements("..content-container")]
        public ContainerWidget ContentContainer { get; private set; }

        public event Action OnCancel;
        public event Action<PropData> OnConfirm;
        public event Action<PropController> OnConfirmController;

        public void Initialize(PropController controller)
        {
            _controller = controller;

            var controllerTransform = controller.Content.GameObject.transform;

            Content.LocalVisible = false;
            Container.GameObject.transform.position = controllerTransform.position;
            
            controllerTransform.SetParent(
                ContentContainer.GameObject.transform,
                true);

            AddListeners();
        }

        public void Initialize(string assetId)
        {
            Content.LocalVisible = true;
            Content.Schema.Set("assetSrc", assetId);

            AddListeners();
        }

        private void AddListeners()
        {
            BtnOk.Activator.OnActivated += Ok_OnActivated;
            BtnCancel.Activator.OnActivated += Cancel_OnActivated;
        }

        private void Ok_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != _controller)
            {
                if (null != OnConfirmController)
                {
                    OnConfirmController(_controller);
                }

                return;
            }

            var prop = PropData.Create(Content);
            if (null == prop)
            {
                Log.Error(this,
                    "Could not create PropData from ContentWidget {0}.", Content);
                if (null != OnCancel)
                {
                    OnCancel();
                }

                return;
            }

            if (null != OnConfirm)
            {
                OnConfirm(prop);
            }
        }

        private void Cancel_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnCancel)
            {
                OnCancel();
            }
        }
    }
}