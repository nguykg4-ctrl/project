using System.Text.Json;
using ScreenWorking.Collaboration.Editor.Models;

namespace ScreenWorking.Collaboration.Shared.Protocol
{
    public static class MessagePackSerializerWrapper
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };

        public static byte[] Serialize(CollaborationOperation op)
        {
            if (op == null) return System.Array.Empty<byte>();
            return JsonSerializer.SerializeToUtf8Bytes(op, JsonOptions);
        }

        public static CollaborationOperation Deserialize(byte[] data)
        {
            if (data == null || data.Length == 0) return null;
            return JsonSerializer.Deserialize<CollaborationOperation>(data, JsonOptions);
        }

        public static string ToJsonString(CollaborationOperation op)
        {
            if (op == null) return string.Empty;
            return JsonSerializer.Serialize(op, JsonOptions);
        }
    }
}
