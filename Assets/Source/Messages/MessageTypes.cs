using System.Runtime.Serialization;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application-wide message types.
    /// </summary>
    public static class MessageTypes
    {
        ///////////////////////////////////////////////////////////////////////
        // Error.
        ///////////////////////////////////////////////////////////////////////
        public const int FATAL_ERROR = -1;
        
        ///////////////////////////////////////////////////////////////////////
        // Initialization
        ///////////////////////////////////////////////////////////////////////
        public const int READY = 1;
        public const int AUTHORIZED = 2;
        public const int RESTART = 3;
        public const int STATUS = 4;
        public const int DEFAULT_STATE = 5;

        // Assets
        public const int ASSET_LIST = 10;
        public const int ASSET_ADD = 11;
        public const int ASSET_REMOVE = 12;
        public const int ASSET_UPDATE = 13;

        // Scripts
        public const int SCRIPT_LIST = 20;
        public const int SCRIPT_ADD = 21;
        public const int SCRIPT_REMOVE = 22;
        public const int SCRIPT_UPDATE = 23;

        // Content
        public const int CONTENT_LIST = 30;
        public const int CONTENT_ADD = 31;
        public const int CONTENT_REMOVE = 32;
        public const int CONTENT_UPDATE = 33;

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

        ///////////////////////////////////////////////////////////////////////
        // Preview State
        ///////////////////////////////////////////////////////////////////////
        public const int PREVIEW = 1000;

        ///////////////////////////////////////////////////////////////////////
        // Edit State
        ///////////////////////////////////////////////////////////////////////
        public const int EDIT = 2000;

        ///////////////////////////////////////////////////////////////////////
        // Play State
        ///////////////////////////////////////////////////////////////////////
        public const int PLAY = 3000;
        public const int MUSIC = 3010;
        public const int SCENE_CREATE = 3100;
        public const int SCENE_UPDATE = 3101;
        public const int SCENE_DELETE = 3102;
        public const int ELEMENT_CREATE = 3200;
        public const int ELEMENT_UPDATE = 3201;
        public const int ELEMENT_DELETE = 3202;

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