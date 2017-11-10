using UnityEngine;

namespace CreateAR.SpirePlayer.AR
{
    public interface IArService
    {
        Camera Camera { get; set;}
        
        void Setup(ArServiceConfiguration config);
        void Teardown();
    }
}