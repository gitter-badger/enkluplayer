using System;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Controller for UI to place an anchor.
    /// </summary>
    public class PlaceAnchorUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Prefab for an anchor.
        /// </summary>
        public AnchorRenderer AnchorPrefab;

        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..anchor-container")]
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
                        Type = ElementTypes.WORLD_ANCHOR,
                        Schema = new ElementSchemaData
                        {
                            Strings =
                            {
                                { "name", "World Anchor" }
                            },
                            Vectors =
                            {
                                { "position", PrefabContainer.GameObject.transform.position.ToVec() },
                                { "rotation", PrefabContainer.GameObject.transform.rotation.eulerAngles.ToVec() }
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
        /// Preps creating a new anchor.
        /// </summary>
        public void Initialize()
        {
            Instantiate(
                AnchorPrefab,
                PrefabContainer.GameObject.transform);
        }
    }
}