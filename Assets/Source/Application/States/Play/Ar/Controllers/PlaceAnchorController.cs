using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controller for UI to place an anchor.
    /// </summary>
    [InjectVine("Anchor.Place")]
    public class PlaceAnchorController : InjectableIUXController
    {
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
        /// Called when placement is finalized
        /// </summary>
        public event Action<ElementData> OnOk;

        /// <summary>
        /// Called to cancel placement.
        /// </summary>
        public event Action OnCancel;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

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
        /// Preps creating a new anchor.
        /// </summary>
        public void Initialize(PlayModeConfig config)
        {
            Instantiate(
                config.AnchorPrefab,
                PrefabContainer.GameObject.transform);
        }
    }
}