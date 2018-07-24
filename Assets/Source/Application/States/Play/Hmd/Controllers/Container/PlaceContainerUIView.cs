using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controller for UI to place an anchor.
    /// </summary>
    public class PlaceContainerUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..container-container")]
        public ContainerWidget PrefabContainer { get; private set; }

        [InjectElements("..btn-ok")]
        public ButtonWidget BtnOk { get; private set; }

        [InjectElements("..btn-cancel")]
        public ButtonWidget BtnCancel { get; private set; }

        /// <summary>
        /// Called when placement is finalized.
        /// </summary>
        public event Action<ElementData> OnOk;

        /// <summary>
        /// Called to cancel placement.
        /// </summary>
        public event Action OnCancel;

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnOk.Activator.OnActivated += _ =>
            {
                if (null != OnOk)
                {
                    var id = Guid.NewGuid().ToString();
                    OnOk(new ElementData
                    {
                        Id = id,
                        Type = ElementTypes.CONTAINER,
                        Schema = new ElementSchemaData
                        {
                            Strings =
                            {
                                { "name", "Container" }
                            },
                            Vectors =
                            {
                                { "position", PrefabContainer.GameObject.transform.position.ToVec() }
                            }
                        }
                    });
                }
            };

            BtnCancel.Activator.OnActivated += _ =>
            {
                if (null != OnCancel)
                {
                    OnCancel();
                }
            };
        }

        /// <summary>
        /// Preps creating a new container.
        /// </summary>
        public void Initialize(PlayModeConfig config)
        {
            Instantiate(
                config.ContainerPrefab,
                PrefabContainer.GameObject.transform);
        }
    }
}