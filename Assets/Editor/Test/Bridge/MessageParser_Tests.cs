using CreateAR.Spire;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class MessageParser_Tests
    {
        public class Payload
        {
            public string Guid;
        }
        
        [Test]
        public void Basic()
        {
            var parser = new DefaultMessageParser();

            var messageType = "testmessage";
            var payloadGuid = "asd;kj-hasdflkjhasdflkjhsd-009f";

            string outMessageType;
            string outPayloadString;

            var payload = new Payload
            {
                Guid = payloadGuid
            };

            var message = string.Format(
                "{0};{1}",
                messageType,
                JsonUtility.ToJson(payload));

            Assert.IsTrue(
                parser.ParseMessage(
                    message,
                    out outMessageType,
                    out outPayloadString));

            var value = JsonUtility.FromJson<Payload>(outPayloadString);

            Assert.AreEqual(messageType, outMessageType);
            Assert.AreEqual(payloadGuid, value.Guid);
        }
    }
}