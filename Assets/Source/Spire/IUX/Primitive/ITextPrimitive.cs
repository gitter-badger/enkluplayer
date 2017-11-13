namespace CreateAR.SpirePlayer.UI
{
    public interface ITextPrimitive : IPrimitive
    {
        string Text { get; set; }
        int FontSize { get; set; }
    }
}
