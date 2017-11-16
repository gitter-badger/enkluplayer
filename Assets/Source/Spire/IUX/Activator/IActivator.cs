using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    public interface IActivator : IElement, IInteractive
    {
        IWidget Frame { get; }
        bool FillImageVisible { get; set; }
        void SetStabilityRotation(float degress);
        void SetActivationFill(float percent);
        void SetAimScale(float aimScale);
        void SetAimColor(Col4 aimColor);
        void ShowActivateVFX();
        bool IsTargeted(Ray ray);
        void SetInteractionEnabled(bool isInteractable, bool isFocused);
        float GetBoundingRadius();
        void ChangeState<T>() where T : ActivatorState;
    }
}
