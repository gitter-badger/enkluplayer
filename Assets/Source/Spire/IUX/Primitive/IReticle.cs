using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreateAR.SpirePlayer.UI;

namespace CreateAR.SpirePlayer
{
    public interface IReticle : IElement
    {
        float Rotation { get; set; }
        float Scale { get; set; }
        float Spread { get; set; }
        float CenterAlpha { get; set; }
    }
}
