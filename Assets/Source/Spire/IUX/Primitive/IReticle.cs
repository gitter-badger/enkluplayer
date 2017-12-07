namespace CreateAR.SpirePlayer.UI
{
    public interface IReticle
    {
        float Rotation { get; set; }
        float Scale { get; set; }
        float Spread { get; set; }
        float CenterAlpha { get; set; }
    }
}