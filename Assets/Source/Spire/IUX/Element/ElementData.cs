using System.Linq;

namespace CreateAR.SpirePlayer.UI
{
    public class ElementData
    {
        public string Id;
        public ElementData[] Children = new ElementData[0];

        public ElementData()
        {
            
        }

        public ElementData(ElementData data)
        {
            Id = data.Id;
            Children = data.Children.ToArray();
        }
    }
}