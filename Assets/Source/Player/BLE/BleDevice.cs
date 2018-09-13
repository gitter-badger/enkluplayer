using System.Collections.Generic;
using System.Text;

namespace CreateAR.EnkluPlayer.BLE
{
    public class BleDevice
    {
        public string Id;
        public string Name;
        public Dictionary<string, object> Props;

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("[BleDevice Name={0}, Id={1}]\n{{\n",
                Name,
                Id);

            foreach (var pair in Props)
            {
                var value = pair.Value;
                var stringArray = value as string[];
                if (null != stringArray)
                {
                    builder.AppendFormat("\t{0}={1}\n",
                        pair.Key,
                        string.Join(", ", stringArray));
                }
                else
                {
                    builder.AppendFormat("\t{0}={1}\n",
                        pair.Key,
                        pair.Value);
                }
            }

            builder.AppendFormat("}}");

            return builder.ToString();
        }
    }
}