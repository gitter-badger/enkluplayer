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
        // Types.
        ///////////////////////////////////////////////////////////////////////
        public const int CONTAINER = 0;
        public const int BUTTON = 10;
        public const int CURSOR = 20;
        public const int CAPTION = 30;
        public const int MENU = 100;
        public const int TEXTCRAWL = 120;
        public const int FLOAT = 130;
        public const int TOGGLE = 140;
        public const int SLIDER = 150;
        
        public const int SELECT = 200;
        public const int GRID = 201;
        public const int OPTION = 210;
        public const int OPTION_GROUP = 211;
    }
}