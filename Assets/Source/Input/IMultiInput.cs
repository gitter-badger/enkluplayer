using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public interface IMultiInput
    {
        Camera Camera { get; set; }

        List<InputPoint> Points { get; }
        
        void Update(float dt);
    }
}