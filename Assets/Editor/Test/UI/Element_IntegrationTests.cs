using System.Collections.Generic;
using CreateAR.SpirePlayer.UI;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.UI
{
    [TestFixture]
    public class Element_IntegrationTests
    {
        private ElementFactory _factory;

        private ElementDescription _data = new ElementDescription
        {
            Elements = new[]
            {
                new ElementData
                {
                    Id = "a",
                    Schema = new ElementSchemaData
                    {
                        Ints = new Dictionary<string, int>
                        {
                            {"foo", 12}
                        }
                    }
                }
            },
            Root = new ElementRef()
        };

        [SetUp]
        public void Setup()
        {
            _factory = new ElementFactory();
        }

        [Test]
        public void SchemaGraphUpdate()
        {
            
        }
    }
}