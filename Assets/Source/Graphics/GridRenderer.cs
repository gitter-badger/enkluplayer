using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Configuration for a grid.
    /// </summary>
    [Serializable]
    public class GridConfig
    {
        [Tooltip("True iff grid is enabled.")]
        public bool Enabled;

        [Tooltip("The size of each cell, in world space.")]
        public float CellSize = 1f;

        [Tooltip("The worldspace size of the grid.")]
        public Vector2 GridSize = new Vector2(52, 52);

        [Tooltip("The color of the emphasized lines.")]
        public Color PrimaryColor = new Color(0f, 0f, 0f, 1f);

        [Tooltip("The color of the unemphasized lines.")]
        public Color SecondaryColor = new Color(0f, 0f, 0f, 1f);
    }

    /// <summary>
    /// Renders an okay-looking grid from GL.LINES.
    /// </summary>
    public class GridRenderer : MonoBehaviour
    {
        [Tooltip("The material to render with. Auto generated and only useful for viewing at runtime.")]
        public Material Material;

        /// <summary>
        /// The configuration to use.
        /// </summary>
        public GridConfig Config;
        
        /// <inheritdoc cref="MonoBehaviour"/>
        private void Awake()
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Material = new Material(Shader.Find("Hidden/Internal-Colored"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            
            // Turn backface culling off
            Material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

            // Turn off depth writes
            Material.SetInt("_ZWrite", 0);
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnPostRender()
        {
            if (!Config.Enabled)
            {
                return;
            }
            
            DrawGrid(Config.GridSize, Config.CellSize, Config.SecondaryColor);
            DrawGrid(Config.GridSize, Config.CellSize * 4, Config.PrimaryColor);
        }

        /// <summary>
        /// Draws a grid.
        /// </summary>
        /// <param name="worldSize">Size in world space.</param>
        /// <param name="cellSize">Size of each cell.</param>
        /// <param name="color">Color of the grid.</param>
        private void DrawGrid(
            Vector2 worldSize,
            float cellSize,
            Color color)
        {
            Material.SetPass(0);

            GL.PushMatrix();
            GL.Begin(GL.LINES);
            GL.Color(color);

            var startX = -worldSize.x / 2;
            var startZ = -worldSize.y / 2;
            var endX = worldSize.x / 2;
            var endZ = worldSize.y / 2;

            var numCellsX = worldSize.x / cellSize;
            var numCellsZ = worldSize.y / cellSize;

            for (var i = 0; i <= numCellsX; i++)
            {
                GL.Vertex3(startX + i * cellSize, 0f, startZ);
                GL.Vertex3(startX + i * cellSize, 0f, endZ);
            }

            for (var i = 0; i <= numCellsZ; i++)
            {
                GL.Vertex3(startX, 0f, startZ + i * cellSize);
                GL.Vertex3(endX, 0f, startZ + i * cellSize);
            }

            GL.End();
            GL.PopMatrix();
        }
    }
}