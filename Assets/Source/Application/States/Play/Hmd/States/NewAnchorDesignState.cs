using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;
using Enklu.Data;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Creates a new anchor.
    /// </summary>
    public class NewAnchorDesignState : IArDesignState
    {
        /// <summary>
        /// Manages UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Updater.
        /// </summary>
        private readonly IElementUpdateDelegate _delegate;
        
        /// <summary>
        /// Design controller.
        /// </summary>
        private HmdDesignController _design;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Constructor.
        /// </summary>
        public NewAnchorDesignState(
            IElementUpdateDelegate @delegate,
            IUIManager ui)
        {
            _delegate = @delegate;
            _ui = ui;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _frame = _ui.CreateFrame();

            // open placement menu
            int id;
            _ui
                .Open<PlaceAnchorUIView>(new UIReference
                {
                    UIDataId = "Anchor.Place"
                }, out id)
                .OnSuccess(el =>
                {
                    el.OnOk += Place_OnOk;
                    el.OnCancel += () => _design.ChangeState<MainDesignState>();
                    el.Initialize();
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            //
        }

        /// <inheritdoc />
        public void Exit()
        {
            _frame.Release();
        }

        /// <inheritdoc />
        public void Initialize(HmdDesignController designer, GameObject unityRoot, Element dynamicRoot, Element staticRoot)
        {
            _design = designer;
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            //
        }

        /// <summary>
        /// Exports an anchor and updates associated data.
        /// </summary>
        /// <param name="data">The element data.</param>
        private IAsyncToken<Void> CreateAnchor(ElementData data)
        {
            var token = new AsyncToken<Void>();

            Log.Info(this, "Calculate offsets!");

            // append relative coordinates
            var position = data.Schema.Vectors["position"];
            var eulerAngles = data.Schema.Vectors["rotation"];

            // create element
            _delegate
                .Create(data)
                .OnSuccess(element =>
                {
                    var anchor = (WorldAnchorWidget)element;

                    // on hololens, position + rotation don't do anything-- so set the transform by hand
                    anchor.GameObject.transform.position = position.ToVector();
                    anchor.GameObject.transform.rotation = Quaternion.Euler(eulerAngles.ToVector());

                    // export
                    anchor.Export(_design.App.Id, _delegate.Active);

                    token.Succeed(Void.Instance);
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <summary>
        /// Called when user wishes to place an anchor in a spot.
        /// </summary>
        /// <param name="data">The ElementData to save.</param>
        private void Place_OnOk(ElementData data)
        {
            // kill menu
            _ui.Pop();

            // TODO: open progress indicator

            // create anchor
            CreateAnchor(data)
                .OnSuccess(_ =>
                {
                    _design.ChangeState<MainDesignState>();
                })
                .OnFailure(exception =>
                {
                    int id;
                    _ui
                        .Open<ICommonErrorView>(new UIReference
                        {
                            UIDataId = UIDataIds.ERROR
                        }, out id)
                        .OnSuccess(el =>
                        {
                            el.Message = string.Format("There was an error creating the anchor: {0}",
                                exception.Message);
                            el.Action = "Ok";
                            el.OnOk += () =>
                            {
                                _ui.Close(id);

                                _design.ChangeState<MainDesignState>();
                            };
                        })
                        .OnFailure(ex =>
                        {
                            _design.ChangeState<MainDesignState>();

                            Log.Error(this, "Could not open error view: {0}.", ex);
                        });
                });
        }
    }
}