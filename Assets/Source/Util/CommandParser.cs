using System;

namespace CreateAR.EnkluPlayer
{
    public static class CommandParser
    {
        public static bool Value(char flag, string command, out string value)
        {
            var index = command.IndexOf("-" + flag, StringComparison.Ordinal);
            if (-1 == index)
            {
                value = string.Empty;
                return false;
            }

            var intermediateValue = command.Substring(index + 2).TrimStart(' ');

            // TODO: handle quotes

            var endIndex = intermediateValue.IndexOf(' ');
            if (-1 == endIndex)
            {
                value = intermediateValue;
            }
            else
            {
                value = intermediateValue.Substring(0, endIndex);
            }

            return true;
        }

        public static bool Toggle(string longFlag, string command)
        {
            return command.Contains(" --" + longFlag);
        }
    }
}
