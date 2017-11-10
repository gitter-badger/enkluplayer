using UnityEngine;

namespace CreateAR.SpirePlayer.AR
{
    public interface IArService
    {
        Camera Camera { get; set;}
        ArAnchor[] Anchors { get; }
        
        void Setup(ArServiceConfiguration config);
        void Teardown();
    }
}