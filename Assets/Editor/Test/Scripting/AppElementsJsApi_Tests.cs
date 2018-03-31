using System;
using CreateAR.SpirePlayer.IUX;
using Jint;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace CreateAR.SpirePlayer.Test.Scripting
{
    [Ignore("Run in playmode only.")]
    [TestFixture]
    public class AppElementsJsApi_Tests
    {
        private Engine _engine;
        
        [Inject]
        public AppElementsJsApi Elements { get; set; }

        [SetUp]
        public void Setup()
        {
            _engine = new Engine(options =>
            {
                options.CatchClrExceptions(exception => { throw exception; });
                options.AllowClr();
            });

            Main.Inject(this);
        }
    
        [Test]
        public void Foo()
        {
            Debug.Log("Test");
        }
    }
}