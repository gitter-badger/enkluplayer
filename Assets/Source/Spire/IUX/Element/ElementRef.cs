namespace CreateAR.SpirePlayer.UI
{
    public class ElementRef
    {
        public string Id;
        public ElementRef[] Children = new ElementRef[0];
        public ElementSchemaData Schema = new ElementSchemaData();

        public override string ToString()
        {
            return string.Format("[ElementRef Id={0}, ChildCount={1}]",
                Id,
                Children.Length);
        }
    }
}