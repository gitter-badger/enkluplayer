using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Jint;
using Jint.Native;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test.Scripting
{
    [TestFixture]
    public class VecJsApi_Tests
    {
        private Engine _engine;

        [SetUp]
        public void Setup()
        {
            _engine = new Engine(options =>
            {
                options.CatchClrExceptions(exception => { throw exception; });
                options.AllowClr();
            });

            _engine.SetValue("v", Vec3Methods.Instance);
            _engine.SetValue("vec3", new Func<float, float, float, Vec3>(Vec3Methods.create));
        }

        [Test]
        public void Vec3()
        {
            var result = Run<Vec3>("vec3(1, 2, 3)");
            
            Assert.IsTrue(Math.Abs(1f - result.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(2f - result.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(3f - result.z) < Mathf.Epsilon);
        }

        [Test]
        public void Vec3Mult()
        {
            var result = Run<Vec3>(@"v.mult(3, vec3(1, 1, 1))");
            
            Assert.IsTrue(Math.Abs(3f - result.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(3f - result.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(3f - result.z) < Mathf.Epsilon);
        }
        
        [Test]
        public void Vec3Add()
        {
            var result = Run<Vec3>(@"v.add(vec3(2, 2, 2), vec3(1, 1, 1))");
            
            Assert.IsTrue(Math.Abs(3f - result.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(3f - result.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(3f - result.z) < Mathf.Epsilon);
        }
        
        [Test]
        public void Vec3Dot()
        {
            var program = @"v.dot(vec3(0, 0, 1), vec3(1, 0, 0))";
            var result = Run(program).AsNumber();
            
            Assert.IsTrue(result < Mathf.Epsilon);
        }
        
        [Test]
        public void Vec3Cross()
        {
            var result = Run(@"var c = v.cross(v.right, v.up);v.dot(c, v.right)").AsNumber();
            
            Assert.IsTrue(result < Mathf.Epsilon);
        }
        
        [Test]
        public void Vec3Len()
        {
            var result = Run(@"v.len(vec3(1, 0, 0))").AsNumber();
            
            Assert.IsTrue(Math.Abs(result - 1f) < Mathf.Epsilon);
        }

        public void Vec3Normalize()
        {
            var normalized = Run<Vec3>(@"v.normalize(vec3(10, 0, 0))");
            
            Assert.IsTrue(Math.Abs(normalized.x - 1f) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(normalized.Magnitude - 1f) < Mathf.Epsilon);
        }

        private JsValue Run(string program)
        {
            return _engine.GetValue(_engine.Execute(program).GetCompletionValue());
        }

        private T Run<T>(string program)
        {
            return _engine.GetValue(_engine.Execute(program).GetCompletionValue()).To<T>();
        }
    }
}