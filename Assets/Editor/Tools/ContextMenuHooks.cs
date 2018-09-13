using System.Text;
using CreateAR.EnkluPlayer.IUX;
using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Easy context menu wins.
    /// </summary>
    public static class ContextMenuHooks
    {
        /// <summary>
        /// Copies path to clipboard.
        /// </summary>
        [MenuItem("Assets/Copy Path", false, 1)]
        private static void CopyPath()
        {
            Copy(AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        /// <summary>
        /// Copies guid to clipboard.
        /// </summary>
        [MenuItem("Assets/Copy Guid", false, 0)]
        private static void CopyGuid()
        {
            Copy(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject)));
        }
        
        /// <summary>
        /// Copies text to clipboard.
        /// </summary>
        /// <param name="text">Text to copy.</param>
        private static void Copy(string text)
        {
            var editor = new TextEditor
            {
                text = text
            };

            editor.SelectAll();
            editor.Copy();
        }
    }
}
