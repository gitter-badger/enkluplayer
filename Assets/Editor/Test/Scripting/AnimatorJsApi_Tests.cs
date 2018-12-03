using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
	[TestFixture]
	public class AnimatorJsApi_Tests
	{
		private ElementSchema _schema;
		private Animator _animator;

		private AnimatorJsApi _animatorJsApi;
		
		[SetUp]
		public void Setup()
		{
			_schema = new ElementSchema();
			_animator = new Animator();
		}

		[Test]
		public void HasSchema()
		{
			_schema.Set("animator.Open", true);
			_schema.Set("animator.Speed", 2.26f);
			_schema.Set("animator.Attack", 2);
			
			_animatorJsApi = new AnimatorJsApi(_schema, _animator);
			
			Assert.AreEqual(true, _animatorJsApi.getBool("Open"));
			Assert.AreEqual(2.26f, _animatorJsApi.getFloat("Speed"));
			Assert.AreEqual(2, _animatorJsApi.getInteger("Attack"));
		}

		[Test]
		public void NoSchema()
		{
			
		}
	}
}
