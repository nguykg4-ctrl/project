using UnityEditor;
using UnityEngine;

namespace ScreenWorking.Collaboration.Editor.Adapters
{
    /// <summary>
    /// Normalizes Unity Editor API version differences between Unity 2022.3 LTS and Unity 6 (6000.0+).
    /// </summary>
    public static class EditorApiAdapter
    {
        /// <summary>
        /// Gets a string representation of the current Unity Editor version.
        /// </summary>
        public static string UnityVersion => Application.unityVersion;

        /// <summary>
        /// Returns whether the current environment is running Unity 6 or newer.
        /// </summary>
        public static bool IsUnity6
        {
            get
            {
#if UNITY_6000_0_OR_NEWER
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Converts a Unity Object to its GlobalObjectId string format safely across versions.
        /// </summary>
        public static string GetGlobalObjectIdString(UnityEngine.Object target)
        {
            if (target == null) return string.Empty;
            return GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
        }
    }
}
