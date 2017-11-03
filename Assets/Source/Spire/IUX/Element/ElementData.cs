using System.Linq;

namespace CreateAR.SpirePlayer.UI
{
    public class ElementData
    {
        public string Id;
        public ElementData[] Children = new ElementData[0];
        public ElementSchemaData Schema = new ElementSchemaData();
        
        public ElementData()
        {
            
        }

        internal ElementData(ElementData data)
        {
            Id = data.Id;
            Children = data.Children.ToArray();
            Schema = new ElementSchemaData(data.Schema);
        }
    }
}