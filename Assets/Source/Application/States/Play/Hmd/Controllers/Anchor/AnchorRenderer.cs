using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Affects how anchors are rendered.
    /// </summary>
    public class AnchorRenderer : MonoBehaviour
    {
        public enum PollType
        {
            Dynamic,
            Forced
        }

        /// <summary>
        /// Materials.
        /// </summary>
        private Material[] _materials;
        
        /// <summary>
        /// The anchor.
        /// </summary>
        public WorldAnchorWidget Anchor { get; set; }

        public PollType Poll { get; set; }

        public Color ForcedColor { get; set; }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Awake()
        {
            SetupMaterials();
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        private void Update()
        {
            if (Poll == PollType.Forced)
            {
                ChangeColor(ForcedColor);
                return;
            }

            if (null == Anchor)
            {
                ChangeColor(Color.white);
                return;
            }

            switch (Anchor.Status)
            {
                case WorldAnchorStatus.None:
                {
                    ChangeColor(Color.white);
                    return;
                }
                case WorldAnchorStatus.IsLoading:
                {
                    ChangeColor(Color.blue);
                    return;
                }
                case WorldAnchorStatus.IsImporting:
                {
                    ChangeColor(Color.magenta);
                    return;
                }
                case WorldAnchorStatus.IsReadyLocated:
                {
                    ChangeColor(Color.green);
                    return;
                }
                case WorldAnchorStatus.IsReadyNotLocated:
                {
                    ChangeColor(Color.yellow);
                    return;
                }
                case WorldAnchorStatus.IsError:
                {
                    ChangeColor(Color.red);
                    return;
                }
            }
        }

        /// <summary>
        /// Sets up materials.
        /// </summary>
        private void SetupMaterials()
        {
            var materials = new List<Material>();
            var renderers = GetComponentsInChildren<MeshRenderer>();
            for (int i = 0, len = renderers.Length; i < len; i++)
            {
                materials.AddRange(renderers[i].materials);
            }

            _materials = materials.ToArray();

            ChangeColor(Color.white);
        }

        /// <summary>
        /// Changes the color of materials.
        /// </summary>
        /// <param name="color">The color.</param>
        private void ChangeColor(Color color)
        {
            for (var i = 0; i < _materials.Length; i++)
            {
                _materials[i].SetColor("_Color", color);
            }
        }
    }
}