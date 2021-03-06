﻿namespace CreateAR.EnkluPlayer
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
        public const int PLAY_CRITICAL_ERROR = -100;
        public const int ARSERVICE_EXCEPTION = -10;
        public const int ARSERVICE_INTERRUPTED = -11;
        public const int VERSION_UPGRADE = -3;
        public const int VERSION_MISMATCH = -2;
        public const int FATAL_ERROR = -1;
        
        ///////////////////////////////////////////////////////////////////////
        // Initialization
        ///////////////////////////////////////////////////////////////////////
        public const int APPLICATION_INITIALIZED = 1;
        public const int RECV_CREDENTIALS = 2;
        public const int RECV_APP_INFO = 3;
        public const int INITIAL_EDITOR_SETTINGS = 4001;
        public const int RECV_ENV_INFO = 4;
        public const int LOAD_APP = 5;
        public const int USER_PROFILE = 6;
        public const int LOGIN = 7;
        public const int FLOOR_FOUND = 8;
        public const int AR_SETUP = 9;
        public const int LOGIN_COMPLETE = 1000001;
        public const int SIGNOUT = 1000002;
        public const int HOLOLOGIN = 1000003;
        public const int DEVICE_REGISTRATION_COMPLETE = 1000004;
        public const int ANCHOR_AUTOEXPORT = 1000005;
        public const int ANCHOR_RESETPRIMARY = 1000006;
        public const int DEVICE_REGISTRATION = 1000007;
        public const int ENV_INFO_UPDATE = 1000008;
        
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
        // Play State
        ///////////////////////////////////////////////////////////////////////
        public const int PLAY = 3000;
        public const int MUSIC = 3010;

        public const int SCENE_CREATE = 3100;
        public const int SCENE_UPDATE = 3101;
        public const int SCENE_DELETE = 3102;
        public const int FILE_UPDATE = 3110;

        public const int BRIDGE_HELPER_REPARENT = 3990;
        public const int BRIDGE_HELPER_SELECT = 3991;
        public const int BRIDGE_HELPER_FOCUS = 3992;
        public const int BRIDGE_HELPER_PREVIEW_ACTION = 3993;
        public const int BRIDGE_HELPER_REFRESH_ELEMENT_SCRIPTS = 3994;
        public const int BRIDGE_HELPER_GIZMO_TRANSLATION = 3995;
        public const int BRIDGE_HELPER_GIZMO_ROTATION = 3996;
        public const int BRIDGE_HELPER_GIZMO_SCALE = 3997;
        public const int BRIDGE_HELPER_TRANSFORM_SPACE = 3998;
        public const int BRIDGE_HELPER_SETTING = 4000;

        ///////////////////////////////////////////////////////////////////////
        // Tools
        ///////////////////////////////////////////////////////////////////////
        public const int TOOLS = 5000;
        public const int BUGREPORT = 5020;

        ///////////////////////////////////////////////////////////////////////
        // User Interface
        ///////////////////////////////////////////////////////////////////////
        public const int WIDGET_FOCUS = 10100;
        public const int WIDGET_UNFOCUS = 10101;

        public const int BUTTON_ACTIVATE = 10200;
    }
}