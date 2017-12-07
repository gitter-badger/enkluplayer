namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// References an <c>ElementData</c> instance. May contain child references
    /// and overrides for schema.
    /// </summary>
    public class ElementRef
    {
        /// <summary>
        /// Unique id of the <c>ElementData</c> to reference.
        /// </summary>
        public string Id;

        /// <summary>
        /// List of child references.
        /// </summary>
        public ElementRef[] Children = new ElementRef[0];

        /// <summary>
        /// Schema overrides. These take precedence over the referenced
        /// <c>ElementData</c>.
        /// </summary>
        public ElementSchemaData Schema = new ElementSchemaData();

        /// <summary>
        /// Useful ToString method.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[ElementRef Id={0}, ChildCount={1}]",
                Id,
                Children.Length);
        }
    }
}