using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Test.UI
{
    public class DummyColorConfig : IColorConfig
    {
        public string CurrentProfileName { get; private set; }
        public ColorProfile CurrentProfile { get; private set; }
        public List<ColorProfile> Profiles { get; private set; }
        public bool TryGetColor(VirtualColor virtualColor, out Col4 color)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetColor(string virtualColorStr, out Col4 color)
        {
            throw new System.NotImplementedException();
        }

        public Col4 GetColor(VirtualColor virtualColor)
        {
            throw new System.NotImplementedException();
        }

        public Col4 Colorize(Col4 targetColor, VirtualColor virtualColor)
        {
            throw new System.NotImplementedException();
        }
    }
}