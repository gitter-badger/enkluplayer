﻿using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using Jint;
using Jint.Native;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
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
            var result = _engine.Run<Vec3>("vec3(1, 2, 3)");

            Assert.IsTrue(result.GetType() == typeof(Vec3));
            Assert.IsTrue(Math.Abs(1f - result.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(2f - result.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(3f - result.z) < Mathf.Epsilon);

            result = _engine.Run<Vec3>("vec3(0, 0, 0).set(4, 5, 6)");

            Assert.IsTrue(result.GetType() == typeof(Vec3));
            Assert.IsTrue(Math.Abs(4f - result.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(5f - result.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(6f - result.z) < Mathf.Epsilon);
        }

        [Test]
        public void Vec3Mult()
        {
            var result = _engine.Run<Vec3>(@"v.mult(3, vec3(1, 1, 1))");
            
            Assert.IsTrue(Math.Abs(3f - result.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(3f - result.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(3f - result.z) < Mathf.Epsilon);
        }
        
        [Test]
        public void Vec3Add()
        {
            var result = _engine.Run<Vec3>(@"v.add(vec3(2, 2, 2), vec3(1, 1, 1))");
            
            Assert.IsTrue(Math.Abs(3f - result.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(3f - result.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(3f - result.z) < Mathf.Epsilon);
        }
        
        [Test]
        public void Vec3Dot()
        {
            var program = @"v.dot(vec3(0, 0, 1), vec3(1, 0, 0))";
            var result = _engine.Run(program).AsNumber();
            
            Assert.IsTrue(result < Mathf.Epsilon);
        }
        
        [Test]
        public void Vec3Cross()
        {
            var result = _engine.Run(@"var c = v.cross(v.right, v.up);v.dot(c, v.right)").AsNumber();
            
            Assert.IsTrue(result < Mathf.Epsilon);
        }
        
        [Test]
        public void Vec3Len()
        {
            var result = _engine.Run(@"v.len(vec3(1, 0, 0))").AsNumber();
            
            Assert.IsTrue(Math.Abs(result - 1f) < Mathf.Epsilon);
        }

        [Test]
        public void Vec3Normalize()
        {
            var normalized = _engine.Run<Vec3>(@"v.normalize(vec3(10, 0, 0))");
            
            Assert.IsTrue(Math.Abs(normalized.x - 1f) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(normalized.Magnitude - 1f) < Mathf.Epsilon);
        }

        [Test]
        public void Vec3Distance()
        {
            Vector3 a = new Vector3(-2.2f, 6, -3);
            Vector3 b = new Vector3(8.7f, -2, 0);

            string aStr = string.Format("vec3({0}, {1}, {2})", a.x, a.y, a.z);
            string bStr = string.Format("vec3({0}, {1}, {2})", b.x, b.y, b.z);

            double result = _engine.Run<double>(string.Format("v.distance({0}, {1})", aStr, bStr));
            Assert.IsTrue(Math.Abs(result - Vector3.Distance(a, b)) < Mathf.Epsilon);
        }

        [Test]
        public void Vec3DistanceSqr()
        {
            Vector3 a = new Vector3(-2.2f, 6, -3);
            Vector3 b = new Vector3(8.7f, -2, 0);

            string aStr = string.Format("vec3({0}, {1}, {2})", a.x, a.y, a.z);
            string bStr = string.Format("vec3({0}, {1}, {2})", b.x, b.y, b.z);

            float result = (float) _engine.Run<double>(string.Format("v.distanceSqr({0}, {1})", aStr, bStr));
            Assert.IsTrue(Math.Abs(result - (a - b).sqrMagnitude) < Mathf.Epsilon);
        }
    }
}