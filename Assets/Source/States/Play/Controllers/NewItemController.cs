using System;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the new item menu.
    /// </summary>
    public class NewItemController : InjectableMonoBehaviour
    {
        /// <summary>
        /// Events to listen to.
        /// </summary>
        private IUXEventHandler _events;

        /// <summary>
        /// Container to add everything to.
        /// </summary>
        private Element _container;

        /// <summary>
        /// Grid element.
        /// </summary>
        private GridWidget _grid;

        /// <summary>
        /// Back button.
        /// </summary>
        private ButtonWidget _backButton;

        /// <summary>
        /// Parses vines.
        /// </summary>
        [Inject]
        public VineImporter Parser { get; set; }

        /// <summary>
        /// Creates elements.
        /// </summary>
        [Inject]
        public IElementFactory Elements{ get; set; }

        /// <summary>
        /// Called when we wish to create a prop.
        /// </summary>
        public event Action OnConfirm;

        /// <summary>
        /// Called when we wish to cancel prop creation.
        /// </summary>
        public event Action OnCancel;

        /// <summary>
        /// Initializes the controller + readies it for show/hide.
        /// </summary>
        /// <param name="events">Events to listen to.</param>
        /// <param name="container">Container to add elements to.</param>
        public void Initialize(IUXEventHandler events, Element container)
        {
            _events = events;
            _container = container;
        }

        /// <summary>
        /// Shows the menu.
        /// </summary>
        public void Show()
        {
            {
                var description = Parser.Parse(
                    @"<?Vine><Button id='btn-back' icon='cancel' />");

                // TODO: REMOVE HACK
                description.Elements[0].Schema.Vectors["position"] = new Vec3(-0.35f, 0, 0);

                _backButton = (ButtonWidget) Elements.Element(description);
                _backButton.Activator.OnActivated += BackButton_OnActivate;
                _container.AddChild(_backButton);
            }

            {
                _grid = (GridWidget)Elements.Element(Parser.Parse(@"<Grid fontSize=20 />"));
                _container.AddChild(_grid);
            }
        }

        /// <summary>
        /// Hides the menu.
        /// </summary>
        public void Hide()
        {
            _backButton.Activator.OnActivated -= BackButton_OnActivate;
            _backButton.Destroy();

            _grid.Destroy();
        }

        /// <summary>
        /// Uninitializes controller. Show/Hide should not be called again
        /// until Initialize is called.
        /// </summary>
        public void Uninitialize()
        {
            
        }

        /// <summary>
        /// Called when the back button has been pressed.
        /// </summary>
        /// <param name="activatorPrimitive">The primitive.</param>
        private void BackButton_OnActivate(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnCancel)
            {
                OnCancel();
            }
        }
    }
}