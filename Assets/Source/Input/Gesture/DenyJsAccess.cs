using System;

namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// Restricts method from being called by Js.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DenyJsAccess : Attribute
    {
        //
    }
}