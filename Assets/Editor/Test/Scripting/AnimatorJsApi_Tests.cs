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
		private TestAnimator _animator;
		
		private AnimatorJsApi _animatorJsApi;
		
		[SetUp]
		public void Setup()
		{
			_schema = new ElementSchema();
			_animator = new TestAnimator(new AnimatorControllerParameter[]
			{
				new AnimatorControllerParameter()
				{
					name = "Open",
					defaultBool = false,
				},
				new AnimatorControllerParameter()
				{
					name = "Speed",
					defaultFloat = 0f,
				},
				new AnimatorControllerParameter()
				{
					name = "Attack",
					defaultInt = 0,
				}
			});
		}

		[Test]
		public void HasSchema()
		{
			_schema.Set("animator.Open", true);
			_schema.Set("animator.Speed", 2.26f);
			_schema.Set("animator.Attack", 2);
			
			_animatorJsApi = new AnimatorJsApi(_schema, _animator);
			_animatorJsApi.Setup();
			
			// Check values from schema were set
			Assert.AreEqual(true, _animatorJsApi.getBool("Open"));
			Assert.AreEqual(2.26f, _animatorJsApi.getFloat("Speed"));
			Assert.AreEqual(2, _animatorJsApi.getInteger("Attack"));
		}

		[Test]
		public void NoSchema()
		{
			_animatorJsApi = new AnimatorJsApi(_schema, _animator);
			_animatorJsApi.Setup();
			
			// Set via API
			_animatorJsApi.setBool("Open", true);
			_animatorJsApi.setFloat("Speed", 2.26f);
			_animatorJsApi.setInteger("Attack", 2);
			
			// Verify schema was generated
			Assert.AreEqual(true, _schema.GetOwn("animator.Open", false).Value);
			Assert.AreEqual(2.26f, _schema.GetOwn("animator.Speed", 0f).Value);
			Assert.AreEqual(2, _schema.GetOwn("animator.Attack", 0).Value);
			
			// Set via new schema
			_schema.Set("animator.Open", false);
			_schema.Set("animator.Speed", 5.42f);
			_schema.Set("animator.Attack", 4);
			
			// Verify API values match schema
			Assert.AreEqual(false, _animatorJsApi.getBool("Open"));
			Assert.AreEqual(5.42f, _animatorJsApi.getFloat("Speed"));
			Assert.AreEqual(4, _animatorJsApi.getInteger("Attack"));
		}
	}
}
