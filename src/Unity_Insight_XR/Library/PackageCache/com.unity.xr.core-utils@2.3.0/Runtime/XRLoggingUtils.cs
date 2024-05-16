using System;
using System.Linq;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Utility methods for logging.
    /// </summary>
    /// <remarks>
    /// These methods mirror the standard <see cref="Debug"/> log methods, but do not log
    /// anything if tests are being run via command line (using `-runTests`).
    ///
    /// See [Running tests from the command line](https://docs.unity3d.com/Packages/com.unity.test-framework@latest?subfolder=/manual/reference-command-line.html)
    /// for information about running tests. 
    /// </remarks>
    public static class XRLoggingUtils
    {
        static readonly bool k_DontLogAnything;

        static XRLoggingUtils()
        {
            k_DontLogAnything = Environment.GetCommandLineArgs().Contains("-runTests");
        }

        /// <summary>
        /// Same as <see cref="Debug.Log(object, UnityEngine.Object)"/>, but does not print anything if tests are being run.
        /// </summary>
        /// <param name="message">Log message for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void Log(string message, UnityEngine.Object context = null)
        {
            if(!k_DontLogAnything)
                Debug.Log(message, context);
        }

        /// <summary>
        /// Same as <see cref="Debug.LogWarning(object, UnityEngine.Object)"/>, but does not print anything if tests are being run.
        /// </summary>
        /// <param name="message">Warning message for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void LogWarning(string message, UnityEngine.Object context = null)
        {
            if(!k_DontLogAnything)
                Debug.LogWarning(message, context);
        }

        /// <summary>
        /// Same as <see cref="Debug.LogError(object, UnityEngine.Object)"/>, but does not print anything if tests are being run.
        /// </summary>
        /// <param name="message">Error message for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void LogError(string message, UnityEngine.Object context = null)
        {
            if(!k_DontLogAnything)
                Debug.LogError(message, context);
        }

        /// <summary>
        /// Same as <see cref="Debug.LogException(Exception, UnityEngine.Object)"/>, but does not print anything if tests are being run.
        /// </summary>
        /// <param name="exception">Runtime Exception.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void LogException(Exception exception, UnityEngine.Object context = null)
        {
            if(!k_DontLogAnything)
                Debug.LogException(exception, context);
        }
    }
}
