namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application-wide message types.
    /// </summary>
    public static class ElementTypes
    {
        ///////////////////////////////////////////////////////////////////////
        // Error.
        ///////////////////////////////////////////////////////////////////////
        public const int FATAL_ERROR = -1;

        ///////////////////////////////////////////////////////////////////////
        // Primitive Types.
        ///////////////////////////////////////////////////////////////////////
        public const int CONTAINER = 0;
        public const int ACTIVATOR = 1;
        
        public const int BUTTON = 10;
        public const int BUTTON_READY_STATE = 11;
        public const int BUTTON_ACTIVATING_STATE = 12;
        public const int BUTTON_ACTIVATED_STATE = 13;

        public const int CURSOR = 20;

        public const int CAPTION = 30;
    }
}

