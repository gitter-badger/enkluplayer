namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Injectable form.
    /// </summary>
    public class InjectableMonoBehaviourUIElement : MonoBehaviourUIElement
    {
        /// <inheritdoc />
        public override void Created()
        {
            base.Created();
            
            Main.Inject(this);
        }
    }
}