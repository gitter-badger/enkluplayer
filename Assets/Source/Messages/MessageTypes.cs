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
        // Initialization
        ///////////////////////////////////////////////////////////////////////
        public const int READY = 1;
        public const int AUTHORIZED = 2;

        ///////////////////////////////////////////////////////////////////////
        // Preview State
        ///////////////////////////////////////////////////////////////////////
        public const int PREVIEW_ASSET = 100;
    }
}