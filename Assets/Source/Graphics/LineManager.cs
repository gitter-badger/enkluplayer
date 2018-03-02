using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Basic implementation of <c>ILineManager</c> using Unity builtins.
    /// 
    /// TODO: Move away from LineRenderer, bake out single mesh for all lines.
    /// </summary>
    public class LineManager : MonoBehaviour, ILineManager
    {
        /// <summary>
        /// Parallel lists of line data and the associated renderer.
        /// </summary>
        private readonly List<LineData> _lines = new List<LineData>();
        private readonly List<LineRenderer> _renderer = new List<LineRenderer>();

        /// <summary>
        /// Parent of all line renderers.
        /// </summary>
        private Transform _root;

        /// <summary>
        /// Material.
        /// </summary>
        private Material _material;

        /// <summary>
        /// True iff the manager should render the lines.
        /// </summary>
        public bool IsEnabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// The collection of lines.
        /// </summary>
        public ReadOnlyCollection<LineData> Lines { get; private set; }

        /// <inheritdoc />
        public void Add(LineData line)
        {
            if (_lines.Contains(line))
            {
                return;
            }

            _lines.Add(line);
            _renderer.Add(NewRenderer(line));
        }

        /// <inheritdoc />
        public void Remove(LineData line)
        {
            var index = _lines.IndexOf(line);
            if (-1 != index)
            {
                _lines.RemoveAt(index);
                _renderer.RemoveAt(index);
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Awake()
        {
            Lines = new ReadOnlyCollection<LineData>(_lines);

            var go = new GameObject("LineRoot");
            _root = go.transform;
            _root.parent = transform;

            _material = new Material(Shader.Find("Unlit/Color"));
            _material.SetColor("_Color", new Color(0, 1, 76f / 255f, 1));
        }
        
        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            // update renderers
            for (var i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];
                var lineRenderer = _renderer[i];
                lineRenderer.widthMultiplier = line.Thickness;
                lineRenderer.SetPosition(0, line.Start);
                lineRenderer.SetPosition(1, line.End);
            }
        }

        /// <summary>
        /// Creates a new renderer.
        /// </summary>
        /// <param name="line">The line to create a renderer for.</param>
        /// <returns></returns>
        private LineRenderer NewRenderer(LineData line)
        {
            var instance = new GameObject("Line");
            instance.transform.SetParent(_root);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;

            var lineRenderer = instance.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.sharedMaterial = _material;
            lineRenderer.SetPositions(new []
            {
                line.Start,
                line.End
            });

            return lineRenderer;
        }
    }
}