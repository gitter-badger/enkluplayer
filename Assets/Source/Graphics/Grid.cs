using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Renders an okay grid.
    /// </summary>
    public class Grid : MonoBehaviour
    {
        public int CellSize = 1;
        public Vector2 GridSize = new Vector2(10, 10);
        public Color PrimaryColor = new Color(0f, 0f, 0f, 1f);
        public Color SecondaryColor = new Color(0f, 0f, 0f, 1f);

        public Material Material;
        public float LodDropoff = -0.9f;

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

        private void OnPostRender()
        {
            DrawGrid(GridSize, CellSize, SecondaryColor);
            DrawGrid(GridSize, CellSize * 4, PrimaryColor);
        }

        private void DrawGrid(
            Vector2 worldSize,
            int cellSize,
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