using System.IO;
using System.Text;
using UnityEditor;

namespace CreateAR.SpirePlayer.Test
{
    /// <summary>
    /// Runs an obj exporter.
    /// </summary>
    public static class ObjExporterCommand
    {
        /// <summary>
        /// Exports game objects as obj.
        /// </summary>
        [MenuItem("Tools/Export as Obj")]
        private static void Run()
        {
            var path = EditorUtility.SaveFilePanel("Save", ".", "Export", "obj");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            using (var stream = File.OpenWrite(path))
            {
                var export = new ObjExporter().Export(Selection.gameObjects);
                var bytes = Encoding.UTF8.GetBytes(export);

                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}