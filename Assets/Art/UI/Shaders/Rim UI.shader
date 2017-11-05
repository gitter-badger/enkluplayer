Shader "Spire Player/UI Rim"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Strength ("Color strength", Range(0.01, 10)) = 1
		_RimStrength ("Rim strength", Range(0, 10)) = 1.0
	}
	SubShader
	{
		// Subshader Tags
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Back
		ZTest Always
		ZWrite Off
		
		// Render Pass
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "RimCG.cginc"	
			#pragma target 3.0
			
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
	
			////// Uniform user variable definition
			uniform fixed4 _Color;
			uniform fixed _Strength;
			uniform fixed _RimStrength;

			////// Vertex shader
			Vert2Frag vert(VertexInput vertIn)
			{
				return vertRim(vertIn);
			}

			////// Fragment shader
			float4 frag(Vert2Frag fragIn) : SV_Target
			{
				return fragRim(fragIn, _Color, _Strength, _RimStrength);
			}

			ENDCG
		}
	}
}
