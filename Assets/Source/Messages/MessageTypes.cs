﻿namespace CreateAR.SpirePlayer
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
        // State.
        ///////////////////////////////////////////////////////////////////////
        public const int STATE = 0;

        ///////////////////////////////////////////////////////////////////////
        // Initialization
        ///////////////////////////////////////////////////////////////////////
        public const int READY = 1;
        public const int AUTHORIZED = 2;

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
        public const int ASSET_ADDED = 2010;
        public const int ASSET_UPDATED = 2011;
        public const int ASSET_REMOVED = 2012;

        ///////////////////////////////////////////////////////////////////////
        // Play State
        ///////////////////////////////////////////////////////////////////////
        public const int PLAY = 3000;
        public const int MUSIC = 3010;

        ///////////////////////////////////////////////////////////////////////
        // Hierarchy State
        ///////////////////////////////////////////////////////////////////////
        public const int HIERARCHY = 4000;
        public const int SELECT_CONTENT = 4001;
    }
}