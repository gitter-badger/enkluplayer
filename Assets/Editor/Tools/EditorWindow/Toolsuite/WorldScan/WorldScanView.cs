using System;
using System.IO;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Editor;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages.GetMyFilesByTags;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// View for world scans.
    /// </summary>
    public class WorldScanView : IEditorView
    {
        /// <summary>
        /// Record for a world scan.
        /// </summary>
        public class WorldScanRecord
        {
            public string Name;
            public DateTime Updated;
            public Action Download;
        }
        
        /// <summary>
        /// Table to display scans in.
        /// </summary>
        private readonly TableComponent _table = new TableComponent();

        /// <summary>
        /// Position of scroll bar.
        /// </summary>
        private Vector2 _scrollPosition;

        /// <inheritdoc cref="IEditorView"/>
        public event Action OnRepaintRequested;
        
        /// <inheritdoc cref="IEditorView"/>
        public void Draw()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Refresh"))
                    {
                        Refresh();
                    }
                }
                GUILayout.EndHorizontal();

                _table.Draw();
            }
            GUILayout.EndScrollView();

            Repaint();
        }

        /// <summary>
        /// Refreshes world scans.
        /// </summary>
        public void Refresh()
        {
            EditorApplication
                .Api
                .Files
                .GetMyFilesByTags("worldscan")
                .OnSuccess(response =>
                {
                    if (response.NetworkSuccess && response.Payload.Success)
                    {
                        var elements = response
                            .Payload
                            .Body
                            .Select(file => new WorldScanRecord
                            {
                                Name = Path.GetFileName(file.RelUrl),
                                Updated = DateTime.Parse(file.UpdatedAt),
                                Download = Download(file)
                            })
                            .ToList();
                        elements.Sort((a, b) => DateTime.Compare(a.Updated, b.Updated));

                        _table.Elements = elements.ToArray();

                        Repaint();
                    }
                })
                .OnFailure(exception => Log.Error(this, exception));
        }

        /// <summary>
        /// Creates an action for downloading a file.
        /// </summary>
        /// <param name="file">The file to download.</param>
        /// <returns></returns>
        private Action Download(Body file)
        {
            return () =>
            {
                Log.Info(this, "Download {0}.", file.RelUrl);

                var http = EditorApplication.Http;
                var url = http
                    .UrlBuilder
                    .Url(file.RelUrl)
                    .Replace("/v1", "");
                http
                    .Download(url)
                    .OnSuccess(response =>
                    {
                        if (response.NetworkSuccess)
                        {
                            Log.Info(this, "Downloaded {0} bytes.", response.Payload.Length);

                            var source = Encoding.UTF8.GetString(response.Payload);
                            EditorUtility.DisplayProgressBar(
                                "Importing",
                                "Please wait...",
                                0.25f);

                            EditorApplication
                                .ObjImporter
                                .Import(source, action =>
                                {
                                    action(new GameObject("Import"));

                                    EditorUtility.ClearProgressBar();
                                });
                        }
                        else
                        {
                            Log.Warning(this, "Could not download : {0}.", response.NetworkError);
                        }
                    })
                    .OnFailure(exception =>
                    {
                        Log.Error(this, "Could not download : {0}.", exception);
                    });
            };
        }

        /// <summary>
        /// Safely calls repaint event.
        /// </summary>
        private void Repaint()
        {
            if (null != OnRepaintRequested)
            {
                OnRepaintRequested();
            }
        }
    }
}