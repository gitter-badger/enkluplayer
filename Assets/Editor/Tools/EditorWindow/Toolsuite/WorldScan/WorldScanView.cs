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
            public DateTime Updated;
            public Action Import;
            public Action Delete;

            public string Id { get; private set; }

            public WorldScanRecord(string id)
            {
                Id = id;
            }

            public void Update(Body file)
            {
                Updated = DateTime.Parse(file.UpdatedAt);
            }
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
                        var existingElements = _table.Elements;
                        var files = response.Payload.Body.ToList();
                        if (existingElements.Length == files.Count)
                        {
                            var returnEarly = true;
                            for (int i = 0, ilen = files.Count; i < ilen; i++)
                            {
                                var found = false;
                                var file = files[i];
                                for (int j = 0, jlen = existingElements.Length; j < jlen; j++)
                                {
                                    var element = (WorldScanRecord) existingElements[j];
                                    if (element.Id == file.Id)
                                    {
                                        // update!
                                        found = true;

                                        element.Update(file);
                                    }
                                }

                                if (!found)
                                {
                                    returnEarly = false;
                                    break;
                                }
                            }

                            if (returnEarly)
                            {
                                return;
                            }
                        }

                        var elements = files.Select(file =>
                            {
                                var record = new WorldScanRecord(file.Id)
                                {
                                    Import = Download(file),
                                    Delete = Delete(file)
                                };
                                record.Update(file);

                                return record;
                            })
                            .ToList();
                        elements.Sort((a, b) => DateTime.Compare(b.Updated, a.Updated));
                        
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
        /// Deletes a file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        private Action Delete(Body file)
        {
            return () =>
            {
                EditorApplication
                    .Api
                    .Files
                    .DeleteFile(file.Id)
                    .OnSuccess(response =>
                    {
                        if (null != response.Payload
                            && response.Payload.Success)
                        {
                            Refresh();
                        }
                        else
                        {
                            EditorUtility.DisplayDialog(
                                "Error",
                                "Could not delete file: " + (null == response.Payload
                                    ? "Unknown."
                                    : response.Payload.Error),
                                "Okay");
                        }
                    })
                    .OnFailure(exception =>
                        EditorUtility.DisplayDialog(
                            "Error",
                            "Could not delete file: " + exception.Message,
                            "Okay"));
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