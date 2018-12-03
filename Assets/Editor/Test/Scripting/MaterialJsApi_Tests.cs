using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class MaterialJsApi_Tests
    {
        private ElementSchema _schema;
        private IRenderer _renderer;
        private TestMaterial _material;

        private MaterialJsApi _materialJsApi;
        
        [SetUp]
        public void Setup()
        {
            _schema = new ElementSchema();
            _material = new TestMaterial();
            _renderer = new TestRenderer(_material);
        }

        // TODO: Unity 2018.2 upgrade - Enable this test
//        [Test]
//        public void HasSchema()
//        {
//            _schema.Set("material._Alpha", 0.9f);
//            _schema.Set("material._Daytime", 1);
//            _schema.Set("material._Center", new Vec3(1, 2, 3));
//            _schema.Set("material._Color", new Col4(0.1f, 0.2f, 0.3f, 1));
//            
//            _materialJsApi = new MaterialJsApi(_schema, _renderer);
//
//            Assert.AreEqual(0.9f, _material.GetFloat("_Alpha"));
//            Assert.AreEqual(1, _material.GetInt("_Daytime"));
//            Assert.AreEqual(new Vec3(1, 2, 3), _material.GetVector("_Center"));
//            Assert.AreEqual(new Col4(0.1f, 0.2f, 0.3f, 1), _material.GetColor("_Color"));
//        }

        [Test]
        public void NoSchema()
        {
            _materialJsApi = new MaterialJsApi(_schema, _renderer);
            
            // Test setting via the API
            _materialJsApi.setFloat("_Alpha", 0.9f);
            Assert.AreEqual(0.9f, _schema.GetOwn("material._Alpha", -1f).Value);
            
            _materialJsApi.setInt("_Daytime", 1);
            Assert.AreEqual(1, _schema.GetOwn("material._Daytime", -1).Value);
            
            _materialJsApi.setVector("_Center", new Vec3(1, 2, 3));
            Assert.AreEqual(new Vec3(1, 2, 3), _schema.GetOwn("material._Center", Vec3.Zero).Value);
            
            _materialJsApi.setColor("_Color", new Col4(0.1f, 0.2f, 0.3f, 1));
            Assert.AreEqual(new Col4(0.1f, 0.2f, 0.3f, 1), _schema.GetOwn("material._Color", Col4.White).Value);
            
            // Test setting via Schema
            _schema.Set("material._Alpha", 0.2f);
            _schema.Set("material._Daytime", 0);
            _schema.Set("material._Center", new Vec3(3, 2, 1));
            _schema.Set("material._Color", new Col4(1, 2, 3, 1));
            
            Assert.AreEqual(0.2f, _materialJsApi.getFloat("_Alpha"));
            Assert.AreEqual(0, _materialJsApi.getInt("_Daytime"));
            Assert.AreEqual(new Vec3(3, 2, 1), _materialJsApi.getVector("_Center"));
            Assert.AreEqual(new Col4(1f, 2f, 3f, 1), _materialJsApi.getColor("_Color"));
        }
    }
}