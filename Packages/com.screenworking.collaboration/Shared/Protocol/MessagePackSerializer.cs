using System;
using System.IO;
using System.Text.Json;
using ScreenWorking.Collaboration.Editor.Models;

namespace ScreenWorking.Collaboration.Shared.Protocol
{
    /// <summary>
    /// Protocol serializer providing binary wire formatting and fallback JSON serialization for diagnostic logging.
    /// </summary>
    public static class MessagePackSerializerWrapper
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Serializes an operation into JSON UTF-8 byte array.
        /// </summary>
        public static byte[] Serialize(CollaborationOperation op)
        {
            if (op == null) return Array.Empty<byte>();
            return JsonSerializer.SerializeToUtf8Bytes(op, JsonOptions);
        }

        /// <summary>
        /// Deserializes JSON UTF-8 byte array back into a <see cref="CollaborationOperation"/>.
        /// </summary>
        public static CollaborationOperation Deserialize(byte[] data)
        {
            if (data == null || data.Length == 0) return null;
            return JsonSerializer.Deserialize<CollaborationOperation>(data, JsonOptions);
        }

        /// <summary>
        /// Formats an operation payload into a diagnostic string.
        /// </summary>
        public static string ToJsonString(CollaborationOperation op)
        {
            if (op == null) return string.Empty;
            return JsonSerializer.Serialize(op, JsonOptions);
        }
    }
}
