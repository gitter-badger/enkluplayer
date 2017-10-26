﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Gets and sets files.
    /// </summary>
    public class FileManager : IFileManager
    {
        /// <summary>
        /// Object that describes how an <c>IFileSystem</c> is configured.
        /// </summary>
        private class FileSystemConfiguration
        {
            /// <summary>
            /// Protocol this <c>IFileSystem</c> handles.
            /// </summary>
            public string Protocol;

            /// <summary>
            /// How these files are deserialized.
            /// </summary>
            public ISerializer Serializer;

            /// <summary>
            /// <c>IFileSystem</c> implementation.
            /// </summary>
            public IFileSystem FileSystem;
        }

        /// <summary>
        /// Null <c>IFileSystem</c> implementation which allows us to avoid null checks.
        /// </summary>
        private readonly NullFileSystem _nullFileSystem = new NullFileSystem();

        /// <summary>
        /// Null <c>ISerializer</c> implementation which allows us to avoid null checks.
        /// </summary>
        private readonly NullSerializer _nullSerializer = new NullSerializer();

        /// <summary>
        /// List of configured file systems.
        /// </summary>
        private readonly List<FileSystemConfiguration> _configurations = new List<FileSystemConfiguration>();

        /// <summary>
        /// Configures an <c>IFileSystem</c>.
        /// </summary>
        /// <param name="protocol">The protocol this <c>IFileSystem</c> will handle.</param>
        /// <param name="serializer">An object for serializing/deserializing.</param>
        /// <param name="fileSystem">The <c>IFileSystem</c> to use for all operations.</param>
        public void Register(
            string protocol,
            ISerializer serializer,
            IFileSystem fileSystem)
        {
            if (!new Regex(@"^\w+://$").IsMatch(protocol ?? string.Empty))
            {
                throw new ArgumentException(string.Format(
                    "Invalid protocol {0}.",
                    protocol));
            }

            _configurations.Add(new FileSystemConfiguration
            {
                Protocol = protocol,
                Serializer = serializer,
                FileSystem = fileSystem
            });
        }

        /// <inheritdoc cref="IFileManager"/>
        public bool Unregister(string protocol)
        {
            for (int i = 0, len = _configurations.Count; i < len; i++)
            {
                var config = _configurations[i];
                if (config.Protocol == protocol)
                {
                    _configurations.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves a file.
        /// </summary>
        /// <param name="uri">Uri of the File to retrieve.</param>
        /// <returns></returns>
        public IAsyncToken<File<T>> Get<T>(string uri)
        {
            var token = new AsyncToken<File<T>>();
            var configuration = Configuration(uri);

            configuration
                .FileSystem
                .Get(uri)
                .OnSuccess(file =>
                {
                    // deserialize
                    object @object;

                    var bytes = file.Data;
                    try
                    {
                        configuration.Serializer.Deserialize(
                            typeof(T),
                            ref bytes,
                            out @object);
                    }
                    catch (Exception exception)
                    {
                        token.Fail(exception);
                        return;
                    }

                    token.Succeed(new File<T>(
                        file.Uri,
                        (T) @object));
                })
                .OnFailure(token.Fail);

            return token;
        }
        
        /// <summary>
        /// Sets a file.
        /// </summary>
        /// <param name="file">The file to set.</param>
        /// <returns>The file that was set.</returns>
        public IAsyncToken<File<T>> Set<T>(File<T> file)
        {
            var configuration = Configuration(file.Uri);
            
            // serialize
            byte[] bytes;
            try
            {
                configuration.Serializer.Serialize(file.Data, out bytes);
            }
            catch (Exception exception)
            {
                return new AsyncToken<File<T>>(exception);
            }

            var token = new AsyncToken<File<T>>();

            configuration
                .FileSystem
                .Set(new File<byte[]>(
                    file.Uri,
                    bytes))
                .OnSuccess(_ => token.Succeed(file))
                .OnFailure(token.Fail);

            return token;
        }
        
        /// <summary>
        /// Retrieves the configuration for a uri.
        /// </summary>
        private FileSystemConfiguration Configuration(string uri)
        {
            // see if it matches a protocol
            for (int i = 0, len = _configurations.Count; i < len; i++)
            {
                var config = _configurations[i];
                if (uri.StartsWith(config.Protocol))
                {
                    return _configurations[i];
                }
            }

            return new FileSystemConfiguration
            {
                FileSystem = _nullFileSystem,
                Protocol = "",
                Serializer = _nullSerializer
            };
        }
    }
}