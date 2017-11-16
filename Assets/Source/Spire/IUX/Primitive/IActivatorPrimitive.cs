namespace CreateAR.SpirePlayer.UI
{
    public interface IActivatorPrimitive : IInteractivePrimitive
    {
        IWidget Frame { get; }
        bool FillImageVisible { get; set; }
        void SetStabilityRotation(float degress);
        void SetActivationFill(float percent);
        void SetAimScale(float aimScale);
        void SetAimColor(Col4 aimColor);
        void ShowActivateVFX();
    }
}
