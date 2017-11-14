using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreateAR.SpirePlayer
{
    public interface IReticlePrimitive
    {
        float Rotation { get; set; }
        float Scale { get; set; }
        float Spread { get; set; }
    }
}
