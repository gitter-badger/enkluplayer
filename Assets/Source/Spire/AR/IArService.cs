using UnityEngine;

namespace CreateAR.SpirePlayer.AR
{
    public interface IArService
    {
        Camera Camera { get; }
        ArAnchor[] Anchors { get; }
        
        void Setup(Camera camera, ArServiceConfiguration config);
        void Teardown();
    }
}