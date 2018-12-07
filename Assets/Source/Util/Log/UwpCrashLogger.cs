#if NETFX_CORE
using System;
using Windows.ApplicationModel.Core;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Handles C# crashes-- cannot handle memory-related crashes.
    /// </summary>
    public static class UwpCrashLogger
    {
        /// <summary>
        /// Simple object with a bit of context so we can track how many
        /// unhandled exceptions are happening within a single session.
        /// </summary>
        public class UwpCrashContext
        {
            /// <inheritdoc />
            public override string ToString()
            {
                return CoreApplication.Id;
            }
        }

        /// <summary>
        /// Static constructor to handle UWP crashes.
        /// </summary>
        static UwpCrashLogger()
        {
            CoreApplication.UnhandledErrorDetected += (sender, eventArgs) =>
            {
                try
                {
                    eventArgs.UnhandledError.Propagate();
                }
                catch (Exception exception)
                {
                    Log.Fatal(new UwpCrashContext(), exception);

                    // throw again if the application _should_ crash from the exception
                }
            };
        }
    }
}
#endif