using Jint;
using Jint.Native;
using NUnit.Framework;
using System;
using Assets.Source.Player.Scripting;
using CreateAR.EnkluPlayer.Scripting;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class MessagesJsApi_Tests
    {
        private Engine _engine;

        [SetUp]
        public void Setup()
        {
            _engine = JintUtil.NewEngine(false);

            _engine.SetValue("require", new Func<string, JsValue>(
                value => JsValue.FromObject(_engine, new MessagingJsInterface(new JsMessageRouter()))
            ));
        }

        [Test]
        public void Dispatch()
        {
            var output = (float) _engine.Run(
                "var callbackCount = 0;" +

                // Define callbacks
                "function cb1() { callbackCount += 3 }" +
                "function cb2() { callbackCount += 5 }" +

                // Register callbacks and invoke
                "var messages = require('messages');" +
                "messages.on('test', cb1);" +
                "messages.on('test', cb2);" +
                "messages.dispatch('test');" +

                // Remove one and invoke again
                "messages.off('test', cb2);" +
                "messages.dispatch('test');" +

                // Crudely output the result
                "callbackCount;"
            ).AsNumber();
            
            Assert.IsTrue(Mathf.Approximately(output, 11));
        }
    }
}