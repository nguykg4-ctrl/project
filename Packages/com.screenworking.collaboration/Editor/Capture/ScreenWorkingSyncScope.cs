using System;

namespace ScreenWorking.Collaboration.Editor.Capture
{
    /// <summary>
    /// Thread-static suppression scope context used to prevent echo loops when applying remote operations locally.
    /// </summary>
    public struct ScreenWorkingSyncScope : IDisposable
    {
        [ThreadStatic]
        private static bool isSuppressingLocalCapture;

        /// <summary>
        /// Gets a value indicating whether local change tracking is currently suppressed.
        /// </summary>
        public static bool IsSuppressingLocalCapture => isSuppressingLocalCapture;

        /// <summary>
        /// Suppresses local change capture for the duration of the using block.
        /// </summary>
        /// <returns>An IDisposable scope token.</returns>
        public static ScreenWorkingSyncScope SuppressLocalCapture()
        {
            isSuppressingLocalCapture = true;
            return new ScreenWorkingSyncScope();
        }

        /// <summary>
        /// Exits the suppression scope.
        /// </summary>
        public void Dispose()
        {
            isSuppressingLocalCapture = false;
        }
    }
}
