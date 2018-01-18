Shader "Unlit/WorldScan"
{
	Properties
	{
		_Color("Color", Color) = (1.0, 0.2, 0.1, 1.0)
		_Resolution("Resolution", Float) = 1
		_Smoothness("Smoothness", Float) = 0.2
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"Render" = "Transparent"
			"IgnoreProjector" = "True"
		}
		LOD 200

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
			};

			uniform float4 _Color;
			uniform float _Resolution;
			uniform float _Smoothness;

			v2f vert(appdata v) {
				v2f o;

				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				fixed4 col = _Color;

				float x = smoothstep(1 - _Smoothness, 1, frac(_Resolution * i.worldPos.x));
				float y = smoothstep(1 - _Smoothness, 1, frac(_Resolution * i.worldPos.y));
				float z = smoothstep(1 - _Smoothness, 1, frac(_Resolution * i.worldPos.z));

				col.a = x * (1 - x) + y * (1 - y) + z * (1 - z);

				return col;
			}

			ENDCG
		}
	}
}