using System;
using System.Collections;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// ILogTarget implementation that forwards to Unity.
    /// </summary>
    public class LogglyLogTarget : MonoBehaviour, ILogTarget
    {
        /// <summary>
        /// The log formatter.
        /// </summary>
        private readonly ILogFormatter _formatter = new DefaultLogFormatter
        {
            Level = false,
            Timestamp = true,
            TypeName = true
        };
        
        /// <inheritdoc />
        public void OnLog(LogLevel level, object caller, string message)
        {
            var loggingForm = new WWWForm();

            //Add log message to WWWForm
            loggingForm.AddField("LEVEL", level.ToString());
            loggingForm.AddField("Message", _formatter.Format(level, caller, message));
            loggingForm.AddField("Stack_Trace", Environment.StackTrace);
            loggingForm.AddField("Device_Model", SystemInfo.deviceModel);
            StartCoroutine(SendData(loggingForm));
        }

        /// <summary>
        /// Sends data.
        /// </summary>
        /// <param name="form">The form to send.</param>
        /// <returns></returns>
        private IEnumerator SendData(WWWForm form)
        {
            yield return new WWW(
                "http://logs-01.loggly.com/inputs/1f0810f5-db28-4ea3-aeea-ec83d8cb3c0f/tag/AssetImportUnityServer",
                form);
        }
    }
}
