using Newtonsoft.Json;

namespace Source.Messages.ToApplication
{
    /// <summary>
    /// Event containing initial settings from the editor.
    /// </summary>
    public class EditorSettingsEvent
    {
        /// <summary>
        /// Whether the mesh scan is visible.
        /// </summary>
        [JsonProperty("MeshScan")]
        public bool MeshScan { get; set; }
        
        /// <summary>
        /// Whether the grid is visible.
        /// </summary>
        [JsonProperty("Grid")]
        public bool Grid { get; set; }
        
        /// <summary>
        /// Whether the element gizmos are visible.
        /// </summary>
        [JsonProperty("ElementGizmos")]
        public bool ElementGizmos { get; set; }
        
        /// <summary>
        /// Whether the hierarchy lines are visible.
        /// </summary>
        [JsonProperty("HierarchyLines")]
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