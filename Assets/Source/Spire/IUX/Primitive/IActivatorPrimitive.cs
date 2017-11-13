using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreateAR.SpirePlayer.UI
{
    public interface IActivatorPrimitive : IPrimitive
    {
        void SetStabilityRotation(float degress);
        void SetActivationFill(float percent);
    }
}
