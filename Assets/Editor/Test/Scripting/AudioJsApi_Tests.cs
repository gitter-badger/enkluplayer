using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Test.Scripting;
using NUnit.Framework;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
	[TestFixture]
	public class AudioJsApi_Tests
	{
		private ElementSchema _schema;
		private TestAudioSource _audioSource;
		
		private AudioJsApi _audioJsApi;

		[SetUp]
		public void Setup()
		{
			_schema = new ElementSchema();
			_audioSource = new TestAudioSource();			
		}

		[Test]
		public void HasSchema()
		{
			_schema.Set("audio.volume", 1f);
			
			_audioJsApi = new AudioJsApi(_schema, _audioSource);
			_audioJsApi.Setup();
			
			// Check values from schema were set
			Assert.AreEqual(1f, _audioSource.Volume);
		}

		[Test]
		public void NoSchema()
		{
			_audioJsApi = new AudioJsApi(_schema, _audioSource);
			_audioJsApi.Setup();

			// Set via API
			_audioJsApi.volume = 0.7f;
			
			// Verify schema was generated
			Assert.AreEqual(0.7f, _schema.GetOwn("audio.volume", 0f).Value);
			
			// Set via new schema
			_schema.Set("audio.volume", 0.2f);
			
			// Verify API values match schema
			Assert.AreEqual(0.2f, _audioJsApi.volume);
		}
	}
}