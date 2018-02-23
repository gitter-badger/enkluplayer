using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CreateAR.Commons.Unity.DataStructures;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages.CreateFile;

namespace CreateAR.SpirePlayer
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
        private volatile bool _isAlive = false;

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
        private string _fileId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FileResourceUpdater(
            IBootstrapper bootstrapper,
            IHttpService http,
            string tags)
        {
            _bootstrapper = bootstrapper;
            _http = http;
            _tags = tags;
        }

        /// <summary>
        /// Starts the updater. Write calls will be effectively ignored if
        /// start is not called.
        /// </summary>
        public void Start()
        {
            _isAlive = true;
            _bootstrapper.BootstrapCoroutine(Update());
        }

        /// <summary>
        /// Stops the updater.
        /// </summary>
        public void Stop()
        {
            _isAlive = false;
        }

        /// <summary>
        /// Writes bytes to a File resource. The first time Write is called,
        /// a resource is created. Each subsequent call simply updates the
        /// resources.
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

                    if (string.IsNullOrEmpty(_fileId))
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
                    _http.UrlBuilder.Url("file"),
                    new List<Tuple<string, string>>
                    {
                        Tuple.Create("tags", _tags)
                    },
                    ref bytes)
                .OnSuccess(response =>
                {
                    if (null != response.Payload
                        && response.Payload.Success)
                    {
                        _fileId = response.Payload.Body.Id;

                        Log.Info(this, "Successfully created file.");
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
                    _http.UrlBuilder.Url("file/" + _fileId),
                    new List<Tuple<string, string>>(),
                    ref bytes)
                .OnSuccess(response =>
                {
                    if (null != response.Payload
                        && response.Payload.Success)
                    {
                        Log.Info(this, "Successfully updated file.");
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