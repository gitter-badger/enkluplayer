using System.Collections.Generic;
using System.Diagnostics;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Assets;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Used to display load progress.
    /// </summary>
    public class LoadProgressManager : InjectableMonoBehaviour, ILoadProgressManager
    {
        /// <summary>
        /// Internal object used to represent a load.
        /// </summary>
        private class LoadProgressRecord
        {
            /// <summary>
            /// Unique id.
            /// </summary>
            public uint Id;

            public Transform Transform;

            public Bounds Bounds;
            
            public LoadProgress Progress;
            
            public Vector3[] Positions;

            public void Recalculate()
            {
                Positions = new []
                {
                    new Vector3(Bounds.min.x, Bounds.min.y, Bounds.min.z),
                    new Vector3(Bounds.max.x, Bounds.min.y, Bounds.min.z),
                    new Vector3(Bounds.max.x, Bounds.min.y, Bounds.max.z),
                    new Vector3(Bounds.min.x, Bounds.min.y, Bounds.max.z),

                    new Vector3(Bounds.min.x, Bounds.max.y, Bounds.min.z),
                    new Vector3(Bounds.max.x, Bounds.max.y, Bounds.min.z),
                    new Vector3(Bounds.max.x, Bounds.max.y, Bounds.max.z),
                    new Vector3(Bounds.min.x, Bounds.max.y, Bounds.max.z),
                };
            }
        }

        /// <summary>
        /// Unique id generator.
        /// </summary>
        private static uint _ids = 0;

        /// <summary>
        /// For drawing.
        /// </summary>
        private static Material _lineMaterial;

        /// <summary>
        /// List of all records we are currently tracking.
        /// </summary>
        private readonly List<LoadProgressRecord> _records = new List<LoadProgressRecord>();

        /// <summary>
        /// Color of lines.
        /// </summary>
        public Color LineColor;
        
        /// <inheritdoc cref="ILoadProgressManager"/>
        public uint ShowIndicator(Transform rootTransform, Bounds bounds, LoadProgress progress)
        {
            var record = new LoadProgressRecord
            {
                Id = _ids++,
                Transform = rootTransform,
                Bounds = bounds,
                Progress = progress
            };
            record.Recalculate();

            _records.Add(record);

            Verbose("Show load progress indicator. [id={0}]", record.Id);

            return record.Id;
        }
        
        /// <inheritdoc cref="ILoadProgressManager"/>
        public void HideIndicator(uint id)
        {
            var record = Record(id);
            if (null != record)
            {
                _records.Remove(record);
                
                Verbose("Hide load progress indicator. [id={0}]", record.Id);
            }
        }

        /// <summary>
        /// Retrieves a record by id.
        /// </summary>
        /// <param name="id">The unique id of the record.</param>
        /// <returns>The matching record.</returns>
        private LoadProgressRecord Record(uint id)
        {
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                if (record.Id == id)
                {
                    return record;
                }
            }

            return null;
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        private void Update()
        {
            // cull complete indicators
            for (var i = _records.Count - 1; i >= 0; i--)
            {
                var record = _records[i];
                if (record.Progress.IsComplete)
                {
                    HideIndicator(record.Id);
                }
            }
        }

        /// <summary>
        /// Render!
        /// </summary>
        private void OnRenderObject()
        {
            CreateLineMaterial();

            _lineMaterial.SetPass(0);
            
            GL.Begin(GL.LINES);
            {
                GL.Color(LineColor);

                for (int i = 0, len = _records.Count; i < len; i++)
                {
                    var record = _records[i];
                    var positions = _records[i].Positions;

                    GL.PushMatrix();
                    GL.MultMatrix(record.Transform.localToWorldMatrix);
                    {
                        var pos = positions[0]; GL.Vertex3(pos.x, pos.y, pos.z);
                        pos = positions[1]; GL.Vertex3(pos.x, pos.y, pos.z);

                        pos = positions[1]; GL.Vertex3(pos.x, pos.y, pos.z);
                        pos = positions[2]; GL.Vertex3(pos.x, pos.y, pos.z);

                        pos = positions[2]; GL.Vertex3(pos.x, pos.y, pos.z);
                        pos = positions[3]; GL.Vertex3(pos.x, pos.y, pos.z);

                        pos = positions[3]; GL.Vertex3(pos.x, pos.y, pos.z);
                        pos = positions[0]; GL.Vertex3(pos.x, pos.y, pos.z);
                        
                        pos = positions[4]; GL.Vertex3(pos.x, pos.y, pos.z);
                        pos = positions[5]; GL.Vertex3(pos.x, pos.y, pos.z);

                        pos = positions[5]; GL.Vertex3(pos.x, pos.y, pos.z);
                        pos = positions[6]; GL.Vertex3(pos.x, pos.y, pos.z);

                        pos = positions[6]; GL.Vertex3(pos.x, pos.y, pos.z);
                        pos = positions[7]; GL.Vertex3(pos.x, pos.y, pos.z);

                        pos = positions[7]; GL.Vertex3(pos.x, pos.y, pos.z);
                        pos = positions[4]; GL.Vertex3(pos.x, pos.y, pos.z);
                        
                        pos = positions[0]; GL.Vertex3(pos.x, pos.y, pos.z);
                        pos = positions[4]; GL.Vertex3(pos.x, pos.y, pos.z);

                        pos = positions[1]; GL.Vertex3(pos.x, pos.y, pos.z);
                        pos = positions[5]; GL.Vertex3(pos.x, pos.y, pos.z);

                        pos = positions[2]; GL.Vertex3(pos.x, pos.y, pos.z);
                        pos = positions[6]; GL.Vertex3(pos.x, pos.y, pos.z);

                        pos = positions[3]; GL.Vertex3(pos.x, pos.y, pos.z);
                        pos = positions[7]; GL.Vertex3(pos.x, pos.y, pos.z);
                    }
                    GL.PopMatrix();
                }
            }
            GL.End();
        }

        /// <summary>
        /// Lazily creates material for drawing lines.
        /// </summary>
        private static void CreateLineMaterial()
        {
            if (!_lineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                _lineMaterial = new Material(shader);
                _lineMaterial.hideFlags = HideFlags.HideAndDontSave;

                // Turn on alpha blending
                _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                // Turn backface culling off
                _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

                // Turn off depth writes
                _lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        /// <summary>
        /// Logging.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}