using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    public class PlaceObjectController : AutoController
    {
        [InjectElements("..(@type==ImageWidget)")]
        public ImageWidget[] Images { get; private set; }

        [InjectElements("..btn-ok")]
        public ButtonWidget BtnOk { get; private set; }

        [InjectElements("..btn-cancel")]
        public ButtonWidget BtnCancel { get; private set; }

        [InjectElements("..(@type==ContentWidget)")]
        public ContentWidget Content { get; private set; }

        public event Action OnCancel;
        public event Action<PropData> OnConfirm;

        protected override void Initialize(
            Element element,
            object context)
        {
            base.Initialize(element, context);

            var assetId = context.ToString();
            Content.Schema.Set("assetSrc", assetId);

            BtnOk.Activator.OnActivated += Ok_OnActivated;
            BtnCancel.Activator.OnActivated += Cancel_OnActivated;
        }

        protected override void Uninitialize()
        {
            base.Uninitialize();

            BtnOk.Activator.OnActivated -= Ok_OnActivated;
            BtnCancel.Activator.OnActivated -= Cancel_OnActivated;
        }

        private void Ok_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
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