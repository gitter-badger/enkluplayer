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
        }


        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            // update renderers
            for (var i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];
                var lineRenderer = _renderer[i];
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
            instance.transform.SetParent(transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;

            var lineRenderer = instance.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.SetPositions(new []
            {
                line.Start,
                line.End
            });

            return lineRenderer;
        }
    }
}