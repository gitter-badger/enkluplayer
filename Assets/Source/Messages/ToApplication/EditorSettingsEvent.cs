using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Event containing initial settings from the editor.
    /// </summary>
    public class EditorSettingsEvent
    {
        /// <summary>
        /// Whether the mesh scan is visible.
        /// </summary>
        public bool MeshScan { get; set; }
        
        /// <summary>
        /// Whether the grid is visible.
        /// </summary>
        public bool Grid { get; set; }
        
        /// <summary>
        /// Whether the element gizmos are visible.
        /// </summary>
        public bool ElementGizmos { get; set; }
        
        /// <summary>
        /// Whether the hierarchy lines are visible.
        /// </summary>
        public bool HierarchyLines { get; set; }
        
        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[EditorSettingsEvent MeshScan={0}, Grid={1}, ElementGizmos={2}, HierarchyLines={3}]",
                MeshScan, Grid, ElementGizmos, HierarchyLines);
        }
    }
}