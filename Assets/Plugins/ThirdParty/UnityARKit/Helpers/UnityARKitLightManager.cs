using UnityEngine;
using UnityEngine.Rendering;

public class UnityARKitLightManager : MonoBehaviour {

	Light [] lightsInScene;
	SphericalHarmonicsL2 shl;

#if UNITY_IOS
    
    // Use this for initialization
    void Start () {
		//find all the lights in the scene
		lightsInScene = FindAllLights();
		shl = new SphericalHarmonicsL2 ();

        //subscribe to event informing us of light changes from AR
	    UnityEngine.XR.iOS.UnityARSessionNativeInterface.ARFrameUpdatedEvent += UpdateLightEstimations;

	}

	void OnDestroy()
	{
	    UnityEngine.XR.iOS.UnityARSessionNativeInterface.ARFrameUpdatedEvent -= UpdateLightEstimations;
	}
		

	Light []  FindAllLights()
	{
		return FindObjectsOfType<Light> ();
	}



	void UpdateLightEstimations(UnityEngine.XR.iOS.UnityARCamera camera)
	{
		if (camera.lightData.arLightingType == UnityEngine.XR.iOS.LightDataType.LightEstimate) {
			UpdateBasicLightEstimation (camera.lightData.arLightEstimate);
		} 
		else if (camera.lightData.arLightingType == UnityEngine.XR.iOS.LightDataType.DirectionalLightEstimate) 
		{
			UpdateDirectionalLightEstimation (camera.lightData.arDirectonalLightEstimate);
		}
	}

	void UpdateBasicLightEstimation(UnityEngine.XR.iOS.UnityARLightEstimate uarle)
	{
		foreach (Light l in lightsInScene)
		{
			//fix ambient light
			// Convert ARKit intensity to Unity intensity
			// ARKit ambient intensity ranges 0-2000
			// Unity ambient intensity ranges 0-8 (for over-bright lights)
			float newai = uarle.ambientIntensity;
			l.intensity = newai / 1000.0f;

			//Unity Light has functionality to filter the light color to correct temperature
			//https://docs.unity3d.com/ScriptReference/Light-colorTemperature.html
			l.colorTemperature = uarle.ambientColorTemperature;
		}


	
	}

	void UpdateDirectionalLightEstimation(UnityEngine.XR.iOS.UnityARDirectionalLightEstimate uardle)
	{
		for (int colorChannel = 0; colorChannel < 3; colorChannel++) {
			for (int index = 0; index < 9; index++) {
				shl [colorChannel, index] = uardle.sphericalHarmonicsCoefficients [(colorChannel * 9) + index];
			}
		}

		if (LightmapSettings.lightProbes != null) {
			int probeCount = LightmapSettings.lightProbes.count;

			//we have at least one light probe in the scene
			if (probeCount > 0) {

				//Replace all the baked probes in the scene with our generated Spherical Harmonics
				SphericalHarmonicsL2[] bakedProbes = LightmapSettings.lightProbes.bakedProbes;

				for (int i = 0; i < probeCount; i++) {
					bakedProbes [i] = shl;
				}
			}
		}

		//for objects unaffected by any lightprobes, set up ambient probe
		RenderSettings.ambientProbe = shl;
		RenderSettings.ambientMode = AmbientMode.Custom;
	}

#endif
}