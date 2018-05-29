using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// View for moving content.
    /// </summary>
    [InjectVine("Design.MoveContent")]
    public class MoveContentController : InjectableIUXController
    {
        private ContentDesignController _controller;

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

        public event Action<ContentDesignController> OnConfirm;
        public event Action OnCancel;

        public void Initialize(ContentDesignController controller)
        {
            _controller = controller;

            //var world = controller.Element.Schema.Get<Vec3>("position").Value.ToVector();
            var world = controller.transform.position;
            var worldToLocal = Container.GameObject.transform.worldToLocalMatrix;
            var local = worldToLocal.MultiplyPoint3x4(world);
            
            Container.Schema.Set(
                "position",
                local);
            Container.Schema.Set(
                "focus",
                local);
        }
    }
}