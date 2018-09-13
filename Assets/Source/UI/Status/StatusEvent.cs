namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Event to set a status.
    /// </summary>
    public class StatusEvent
    {
        /// <summary>
        /// The type of status.
        /// </summary>
        public enum StatusType
        {
            Replace,
            Push
        }

        /// <summary>
        /// Message to display.
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// Duration.
        /// </summary>
        public readonly float DurationSeconds;

        /// <summary>
        /// Type.
        /// </summary>
        public readonly StatusType Type;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StatusEvent(string message)
            : this(message, 0f, StatusType.Replace)
        {
            //
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public StatusEvent(string message, float durationSeconds)
            : this(message, durationSeconds, StatusType.Replace)
        {
            //
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public StatusEvent(string message, float durationSeconds, StatusType type)
        {
            Message = message;
            DurationSeconds = durationSeconds;
            Type = type;
        }

        /// <summary>
        /// Useful ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Message;
        }
    }
}