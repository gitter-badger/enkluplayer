using System;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;

namespace CreateAR.SpirePlayer
{
    public class NewItemController : InjectableMonoBehaviour
    {
        private IUXEventHandler _events;
        private Element _container;
        private GridWidget _grid;
        private Button _backButton;

        [Inject]
        public VineImporter Parser { get; set; }

        [Inject]
        public IElementFactory Elements{ get; set; }

        public event Action OnConfirm;
        public event Action OnCancel;

        public void Initialize(IUXEventHandler events, Element container)
        {
            _events = events;
            _container = container;
        }

        public void Show()
        {
            {
                var description = Parser.Parse(
                    @"<?Vine><Button id='btn-back' icon='cancel' />");

                // TODO: REMOVE HACK
                description.Elements[0].Schema.Vectors["position"] = new Vec3(-0.35f, 0, 0);

                _backButton = (Button) Elements.Element(description);
                _backButton.Activator.OnActivated += BackButton_OnActivate;
                _container.AddChild(_backButton);
            }

            {
                _grid = (GridWidget)Elements.Element(Parser.Parse(@"<Grid fontSize=20 />"));
                _container.AddChild(_grid);
            }
        }

        public void Hide()
        {
            _backButton.Activator.OnActivated -= BackButton_OnActivate;
            _backButton.Destroy();

            _grid.Destroy();
        }

        public void Uninitialize()
        {
            
        }

        private void BackButton_OnActivate(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnCancel)
            {
                OnCancel();
            }
        }
    }
}