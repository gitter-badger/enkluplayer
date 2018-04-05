namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application-wide message types.
    /// </summary>
    public static class MessageTypes
    {
        public const int RESTART = -1000;

        ///////////////////////////////////////////////////////////////////////
        // Errors.
        ///////////////////////////////////////////////////////////////////////
        public const int ARSERVICE_EXCEPTION = -10;
        public const int ARSERVICE_INTERRUPTED = -11;

        public const int FATAL_ERROR = -1;

        public const int STATUS = 0;

        ///////////////////////////////////////////////////////////////////////
        // Initialization
        ///////////////////////////////////////////////////////////////////////
        public const int APPLICATION_INITIALIZED = 1;
        public const int RECV_CREDENTIALS = 2;
        public const int RECV_APP_INFO = 3;
        public const int RECV_ENV_INFO = 4;
        public const int LOAD_APP = 5;
        public const int USER_PROFILE = 6;
        public const int LOGIN = 7;
        public const int FLOOR_FOUND = 8;

        // Assets
        public const int RECV_ASSET_LIST = 10;
        public const int RECV_ASSET_ADD = 11;
        public const int RECV_ASSET_REMOVE = 12;
        public const int RECV_ASSET_UPDATE = 13;
        public const int RECV_ASSET_UPDATE_STATS = 14;

        // Scripts
        public const int RECV_SCRIPT_LIST = 20;
        public const int RECV_SCRIPT_ADD = 21;
        public const int RECV_SCRIPT_REMOVE = 22;
        public const int RECV_SCRIPT_UPDATE = 23;

        // Materials
        public const int MATERIAL_LIST = 40;
        public const int MATERIAL_ADD = 41;
        public const int MATERIAL_REMOVE = 42;
        public const int MATERIAL_UPDATE = 43;

        // Shaders
        public const int SHADER_LIST = 50;
        public const int SHADER_ADD = 51;
        public const int SHADER_REMOVE = 52;
        public const int SHADER_UPDATE = 53;

        ///////////////////////////////////////////////////////////////////////
        // Global
        ///////////////////////////////////////////////////////////////////////
        public const int LOADPROGRESS = 100;
        public const int APPLICATION_SUSPEND = 110;
        public const int APPLICATION_RESUME = 111;

        ///////////////////////////////////////////////////////////////////////
        // Preview State
        ///////////////////////////////////////////////////////////////////////
        public const int PREVIEW = 1000;
        
        ///////////////////////////////////////////////////////////////////////
        // Play State
        ///////////////////////////////////////////////////////////////////////
        public const int PLAY = 3000;
        public const int MUSIC = 3010;

        public const int SCENE_CREATE = 3100;
        public const int SCENE_UPDATE = 3101;
        public const int SCENE_DELETE = 3102;

        public const int BRIDGE_HELPER_REPARENT = 3990;

        ///////////////////////////////////////////////////////////////////////
        // Hierarchy State
        ///////////////////////////////////////////////////////////////////////
        public const int HIERARCHY = 4000;
        public const int HIERARCHY_LIST = 4001;
        public const int HIERARCHY_ADD = 4002;
        public const int HIERARCHY_REMOVE = 4003;
        public const int HIERARCHY_UPDATE = 4004;
        public const int HIERARCHY_SELECT = 4005;

        ///////////////////////////////////////////////////////////////////////
        // Tools
        ///////////////////////////////////////////////////////////////////////
        public const int TOOLS = 5000;
        public const int MESHCAPTURE = 5010;

        ///////////////////////////////////////////////////////////////////////
        // User Interface
        ///////////////////////////////////////////////////////////////////////
        public const int WIDGET_FOCUS = 10100;
        public const int WIDGET_UNFOCUS = 10101;

        public const int BUTTON_ACTIVATE = 10200;
    }
}