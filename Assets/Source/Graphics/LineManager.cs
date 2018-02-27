using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Renders a collection of lines.
    /// 
    /// TODO: Move away from LineRenderer, bake out single mesh for all lines.
    /// </summary>
    public class LineManager : MonoBehaviour
    {
        public class LineData
        {
            private static uint IDS = 0;

            public readonly uint Id = ++IDS;

            public Vector3 Start;
            public Vector3 End;
            public float Thickness;
        }

        private readonly List<LineData> _lines = new List<LineData>();
        private readonly List<LineRenderer> _renderer = new List<LineRenderer>();

        public void Add(LineData line)
        {
            if (_lines.Contains(line))
            {
                return;
            }

            _lines.Add(line);
            _renderer.Add(NewRenderer(line));
        }

        public void Remove(LineData line)
        {
            var index = _lines.IndexOf(line);
            if (-1 != index)
            {
                _lines.RemoveAt(index);
                _renderer.RemoveAt(index);
            }
        }

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