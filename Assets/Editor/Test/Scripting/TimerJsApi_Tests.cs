using System;
using Assets.Source.Player.Scripting;
using CreateAR.Commons.Unity.Http;
using CreateAR.EnkluPlayer.Editor;
using CreateAR.EnkluPlayer.Scripting;
using Jint;
using Jint.Native;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    // TODO: Figure out why these tests can't be run multiple times without failure or contain both tests active together?!

    [TestFixture]
    public class TimerJsApi_Tests
    {
        private EditorBootstrapper _bootstrapper;
        private Engine _engine;

        [SetUp]
        public void Setup()
        {
            _engine = ScriptingHostFactory.NewEngine(false);

            _bootstrapper = new EditorBootstrapper();

            _engine.SetValue("require", new Func<string, JsValue>(
                value => JsValue.FromObject(_engine, new TimerJsInterface(_bootstrapper))
            ));
        }

        [TearDown]
        public void Teardown()
        {
            _bootstrapper = null;
            _engine = null;
        }

        //[Test]
        //public void SetTimeout()
        //{
        //    _engine.Run(
        //        "var timers = require('timers');" +
        //        "var result = false;" +

        //        "function callback() { result = true; }" +

        //        "timers.setTimeout(callback, 1);"
        //    );

        //    var output = _engine.Run<bool>("result;");
        //    Assert.IsFalse(output);

        //    _bootstrapper.Update();

        //    output = _engine.Run<bool>("result;"); 
        //    Assert.IsTrue(output);

        //    _bootstrapper.Update();
        //}

        //[Test]
        //public void ClearTimeout()
        //{
        //    _engine.Run(
        //        "var timers = require('timers');" +
        //        "var result = false;" +

        //        "function callback() { result = true; }" +

        //        "var id = timers.setTimeout(callback, 1);" +
        //        "timers.clearTimeout(id);"
        //    );

        //    var output = _engine.Run<bool>("result2;");
        //    Assert.IsFalse(output);

        //    _bootstrapper.Update();

        //    output = _engine.Run<bool>("result2;");
        //    Assert.IsFalse(output);
        //}
    }
}