﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CreateAR.Commons.Unity.DataStructures;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages.CreateFile;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Creates a new File resource then updates it with subsequent writes.
    /// </summary>
    public class FileResourceUpdater
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        private readonly IHttpService _http;

        /// <summary>
        /// Tags to upload with the file.
        /// </summary>
        private readonly string _tags;

        /// <summary>
        /// True iff the update loop should be alive.
        /// </summary>
        private volatile bool _isAlive;

        /// <summary>
        /// True iff a request is out.
        /// </summary>
        private bool _requestOut;

        /// <summary>
        /// The queue.
        /// </summary>
        private byte[] _queue;

        /// <summary>
        /// Id of the file to update.
        /// </summary>
        public string FileId { get; private set; }

        /// <summary>
        /// Called when file is successfully updated.
        /// </summary>
        public event Action<string> OnFileUrlChanged;

        /// <summary>
        /// Called when file is successfully created.
        /// </summary>
        public event Action<string> OnFileCreated;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FileResourceUpdater(
            IBootstrapper bootstrapper,
            IHttpService http,
            string tags,
            string fileId = null)
        {
            _bootstrapper = bootstrapper;
            _http = http;
            _tags = tags;

            if (!string.IsNullOrEmpty(fileId))
            {
                FileId = fileId;
            }
        }

        /// <summary>
        /// Starts the updater. Write calls will be effectively ignored if
        /// start is not called.
        /// 
        /// This method must be called from the main thread.
        /// </summary>
        public void Start()
        {
            _isAlive = true;
            _bootstrapper.BootstrapCoroutine(Update());
        }

        /// <summary>
        /// Stops the updater.
        /// 
        /// This method may be called from any thread.
        /// </summary>
        public void Stop()
        {
            _isAlive = false;
        }

        /// <summary>
        /// Writes bytes to a File resource. The first time Write is called,
        /// a resource is created. Each subsequent call simply updates the
        /// resources.
        /// 
        /// This method may be called from any thread.
        /// </summary>
        /// <param name="bytes">The bytes to write to the resource.</param>
        public void Write(byte[] bytes)
        {
            if (null == bytes)
            {
                return;
            }

            Interlocked.Exchange(ref _queue, bytes);
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Update()
        {
            while (_isAlive)
            {
                if (!_requestOut && null != _queue)
                {
                    _requestOut = true;

                    var bytes = _queue;
                    Interlocked.CompareExchange(ref _queue, null, bytes);

                    if (string.IsNullOrEmpty(FileId))
                    {
                        Create(bytes);
                    }
                    else
                    {
                        Update(bytes);
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Calls the Create File Resource endpoint.
        /// </summary>
        /// <param name="bytes">Bytes to write.</param>
        private void Create(byte[] bytes)
        {
            Log.Info(this,
                "Creating new file ({0} bytes).",
                bytes.Length);

            _http
                .PostFile<Response>(
                    _http.Urls.Url("trellis://file"),
                    new List<CreateAR.Commons.Unity.DataStructures.Tuple<string, string>>
                    {
                        CreateAR.Commons.Unity.DataStructures.Tuple.Create("tags", _tags)
                    },
                    ref bytes)
                .OnSuccess(response =>
                {
                    if (null != response.Payload
                        && response.Payload.Success)
                    {
                        FileId = response.Payload.Body.Id;
                        
                        Log.Info(this, "Successfully created file.");

                        if (null != OnFileCreated)
                        {
                            OnFileCreated(response.Payload.Body.Id);
                        }

                        if (null != OnFileUrlChanged)
                        {
                            OnFileUrlChanged(response.Payload.Body.RelUrl);
                        }
                    }
                    else
                    {
                        Log.Error(this,
                            "Could not create file : {0}",
                            null == response.Payload
                                ? response.NetworkError
                                : response.Payload.Error);
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not create file : {0}.",
                        exception.Message);
                })
                .OnFinally(_ =>
                {
                    _requestOut = false;
                });
        }

        /// <summary>
        /// Called the File Resource Update endpoint.
        /// </summary>
        /// <param name="bytes">Bytes to write.</param>
        private void Update(byte[] bytes)
        {
            Log.Info(this,
                "Updating existing file ({0} bytes).",
                bytes.Length);

            _http
                .PutFile<Trellis.Messages.UpdateFile.Response>(
                    _http.Urls.Url("trellis://file/" + FileId),
                    new List<CreateAR.Commons.Unity.DataStructures.Tuple<string, string>>(),
                    ref bytes)
                .OnSuccess(response =>
                {
                    if (null != response.Payload
                        && response.Payload.Success)
                    {
                        Log.Info(this, "Successfully updated file.");

                        if (null != OnFileUrlChanged)
                        {
                            OnFileUrlChanged(response.Payload.Body.RelUrl);
                        }
                    }
                    else
                    {
                        Log.Error(this,
                            "Could not create file : {0}",
                            null == response.Payload
                                ? "Unknown."
                                : response.Payload.Error);
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not create file : {0}.",
                        exception.Message);
                })
                .OnFinally(_ =>
                {
                    _requestOut = false;
                });
        }
    }
}