using System.Collections.Generic;
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

        public Color ReadyColor;
        public Color ErrorColor;
        public Color SavingColor;
        public Color LoadingColor;
        public Color EditingColor;

        public void Error()
        {
            ChangeColor(ErrorColor);
        }

        public void Ready()
        {
            ChangeColor(ReadyColor);
        }

        public void Saving()
        {
            ChangeColor(SavingColor);
        }

        public void Loading()
        {
            ChangeColor(LoadingColor);
        }

        public void Editing()
        {
            ChangeColor(EditingColor);
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Awake()
        {
            SetupMaterials();
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