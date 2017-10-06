using System.IO;
using System.Xml.Serialization;
using UnityEditor;

namespace CreateAR.SpirePlayer
{
    public static class SpireDataMigrator
    {
        [MenuItem("Tools/Spire Data/Migrate Content")]
        public static void BreakUpContent()
        {
            Break(@"C:\Projects\Work\spireplayer-unity\Assets\StreamingAssets\App\SpireDemo\ContentData\TeslaContent.xml");
            Break(@"C:\Projects\Work\spireplayer-unity\Assets\StreamingAssets\App\SpireDemo\ContentData\States.xml");
        }

        private static void Break(string path)
        {
            var serializer = new XmlSerializer(typeof(ContentData[]));
            var bytes = File.ReadAllBytes(path);
            ContentData[] contentData;
            using (var memoryStream = new MemoryStream(bytes))
            {
                contentData = (ContentData[]) serializer.Deserialize(memoryStream);
            }

            foreach (var data in contentData)
            {
                var contentSerializer = new XmlSerializer(typeof(ContentData));
                using (var stream = new MemoryStream())
                {
                    contentSerializer.Serialize(stream, data);

                    var contentBytes = stream.ToArray();
                    var dir = Path.GetDirectoryName(path);
                    File.WriteAllBytes(Path.Combine(dir, data.Id) + ".local", contentBytes);
                }
            }
        }
    }
}