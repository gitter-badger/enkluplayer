using System;
using System.Collections.Generic;
using System.IO;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer.Util
{
    /// <summary>
    /// Takes care of saving N versions of a file, deleting the oldest versions
    /// when a specified limit is reached.
    /// </summary>
    public class VersionedFileWriter
    {
        /// <summary>
        /// Folder files should go in.
        /// </summary>
        private readonly string _folder;

        /// <summary>
        /// File name.
        /// </summary>
        private readonly string _fileName;

        /// <summary>
        /// Extension of files.
        /// </summary>
        private readonly string _extension;

        /// <summary>
        /// Number of versions.
        /// </summary>
        private readonly int _numVersions;

        /// <summary>
        /// Current version.
        /// </summary>
        private int _version = 0;

        /// <summary>
        /// Past writes.
        /// </summary>
        private readonly Queue<string> _fileHistory = new Queue<string>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public VersionedFileWriter(
            string folder,
            string fileName,
            string extension,
            int numVersions)
        {
            _folder = folder;
            _fileName = fileName;
            _extension = extension;
            _numVersions = numVersions;

            if (_numVersions <= 0)
            {
                throw new ArgumentException("numVersions must be greater than 0.");
            }
        }

        /// <summary>
        /// Writes data synchronously.
        /// </summary>
        /// <param name="data">The data to write.</param>
        public void Write(byte[] data)
        {
            var path = Path.Combine(
                _folder,
                string.Format("{0}.{1}.{2}",
                    _fileName,
                    _version++,
                    _extension));

            // write
            try
            {
                File.WriteAllBytes(path, data);
            }
            catch (Exception exception)
            {
                Log.Error(this,
                    "Could not write to {0} : {1}.",
                    path,
                    exception.Message);
                return;
            }

            _fileHistory.Enqueue(path);

            // delete versions
            while (_fileHistory.Count > _numVersions)
            {
                File.Delete(_fileHistory.Dequeue());
            }
        }
    }
}