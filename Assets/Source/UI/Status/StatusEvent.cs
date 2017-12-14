namespace CreateAR.SpirePlayer
{
    public class StatusEvent
    {
        public enum StatusType
        {
            Replace,
            Push
        }

        public readonly string Message;
        public readonly float DurationSeconds;
        public readonly StatusType Type;

        public StatusEvent(string message)
            : this(message, 0f, StatusType.Replace)
        {
            //
        }

        public StatusEvent(string message, float durationSeconds)
            : this(message, durationSeconds, StatusType.Replace)
        {
            //
        }

        public StatusEvent(string message, float durationSeconds, StatusType type)
        {
            Message = message;
            DurationSeconds = durationSeconds;
            Type = type;
        }

        public override string ToString()
        {
            return Message;
        }
    }
}