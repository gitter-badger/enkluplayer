using System.Collections.ObjectModel;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// POCO that contains data for rendering a line.
    /// </summary>
    public class LineData
    {
        /// <summary>
        /// Allows creation of unique ids.
        /// </summary>
        private static uint IDS = 0;

        /// <summary>
        /// Session unique id.
        /// </summary>
        public readonly uint Id = ++IDS;

        /// <summary>
        /// Line start point.
        /// </summary>
        public Vector3 Start;

        /// <summary>
        /// Line end point.
        /// </summary>
        public Vector3 End;

        /// <summary>
        /// Thickness of line.
        /// </summary>
        public float Thickness;

        /// <summary>
        /// True iff line should be drawn.
        /// </summary>
        public bool Enabled;
    }

    /// <summary>
    /// Draws a collection of lines.
    /// </summary>
    public interface ILineManager
    {
        /// <summary>
        /// True iff lines should be rendered.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Collection of lines.
        /// </summary>
        ReadOnlyCollection<LineData> Lines { get; }

        /// <summary>
        /// Add a line.
        /// </summary>
        /// <param name="line">The line.</param>
        void Add(LineData line);

        /// <summary>
        /// Removes a line.
        /// </summary>
        /// <param name="line">The line.</param>
        void Remove(LineData line);
    }
}