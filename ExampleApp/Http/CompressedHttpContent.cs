using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ExampleApp.Http {
    /// <summary>
    /// Represents an HTTP response body and content headers that will be compressed.
    /// </summary>
    public sealed class CompressedHttpContent : HttpContent {

        /// <summary>
        /// The encoding type.
        /// </summary>
        private readonly EncodingType _encodingType;

        /// <summary>
        /// The uncompressed HTTP content.
        /// </summary>
        private readonly HttpContent _uncompressedContent;


        /// <summary>
        /// Creates a new <see cref="CompressedHttpContent"/> object.
        /// </summary>
        /// <param name="content">The content to be compressed.</param>
        /// <param name="encodingType">The encoding type to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="content"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="encodingType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="encodingType"/> specifies an unsupported encoding type.</exception>
        /// <remarks>
        /// GZip and Deflate compression are supported.
        /// </remarks>
        public CompressedHttpContent(HttpContent content, string encodingType) {
            if (content == null) {
                throw new ArgumentNullException(nameof(content));
            }
            if (encodingType == null) {
                throw new ArgumentNullException(nameof(encodingType));
            }

            try {
                _encodingType = (EncodingType) Enum.Parse(typeof(EncodingType), encodingType, true);
            }
            catch {
                throw new ArgumentException($"Unsupported encoding type: {encodingType}", nameof(encodingType));
            }
            _uncompressedContent = content;

            // Copy headers from original response.
            foreach (var header in _uncompressedContent.Headers) {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            // Add Content-Encoding header.
            Headers.ContentEncoding.Add(_encodingType.ToString().ToLowerInvariant());
        }


        /// <summary>
        /// Tests if the specified encoding type is supported.
        /// </summary>
        /// <param name="encodingType">The encoding type.</param>
        /// <returns>
        /// A flag that indicates if the specified encoding type is supported.
        /// </returns>
        public static bool IsContentTypeSupported(string encodingType) {
            return !String.IsNullOrWhiteSpace(encodingType) && Enum.TryParse<EncodingType>(encodingType, true, out var _);
        }


        /// <summary>
        /// Serializes the HTTP content to a stream as an asynchronous operation.
        /// </summary>
        /// <param name="stream">The target stream.</param>
        /// <param name="context">Information about the transport (channel binding token, for example). This parameter may be null.</param>
        /// <returns>
        /// The task object representing the asynchronous operation.
        /// </returns>
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context) {
            Stream compressedStream;

            switch (_encodingType) {
                case EncodingType.Deflate:
                    compressedStream = new DeflateStream(stream, CompressionMode.Compress, true);
                    break;
                case EncodingType.GZip:
                default:
                    compressedStream = new GZipStream(stream, CompressionMode.Compress, true);
                    break;
            }

            return _uncompressedContent.CopyToAsync(compressedStream)
                                       .ContinueWith(t => {
                                           compressedStream?.Dispose();
                                       });
        }


        /// <summary>
        /// Determines whether the HTTP content has a valid length in bytes.
        /// </summary>
        /// <param name="length">The length in bytes of the HTTP content.</param>
        /// <returns>
        /// The return value is always <see langword="false"/>.
        /// </returns>
        protected override bool TryComputeLength(out long length) {
            length = -1;
            return false;
        }


        #region [ Inner Types ]

        /// <summary>
        /// Defines supported encoding types.
        /// </summary>
        private enum EncodingType {
            /// <summary>
            /// GZip encoding.
            /// </summary>
            GZip,
            /// <summary>
            /// Deflate encoding.
            /// </summary>
            Deflate
        }

        #endregion

    }
}
