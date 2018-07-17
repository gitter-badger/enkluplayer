using System.Collections.Generic;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Affects how anchors are rendered.
    /// </summary>
    public class AnchorRenderer : MonoBehaviour
    {
        /// <summary>
        /// Materials.
        /// </summary>
        private Material[] _materials;
        
        /// <summary>
        /// The anchor.
        /// </summary>
        public WorldAnchorWidget Anchor { get; set; }

        public void PlaceholderSaving()
        {

        }

        public void PlaceholderError()
        {

        }

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
            if (null == Anchor)
            {
                ChangeColor(Color.white);
                return;
            }

            switch (Anchor.Status)
            {
                case WorldAnchorWidget.WorldAnchorStatus.None:
                {
                    ChangeColor(Color.white);
                    return;
                }
                case WorldAnchorWidget.WorldAnchorStatus.IsLoading:
                {
                    ChangeColor(Color.blue);
                    return;
                }
                case WorldAnchorWidget.WorldAnchorStatus.IsImporting:
                {
                    ChangeColor(Color.magenta);
                    return;
                }
                case WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated:
                {
                    ChangeColor(Color.green);
                    return;
                }
                case WorldAnchorWidget.WorldAnchorStatus.IsReadyNotLocated:
                {
                    ChangeColor(Color.yellow);
                    return;
                }
                case WorldAnchorWidget.WorldAnchorStatus.IsError:
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