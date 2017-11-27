
////// Input structs
struct VertexInput
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Vert2Frag
{
	fixed4 pos : SV_POSITION;
	float3 posWorld : TEXCOORD0;
	float4 posLocal : TEXCOORD1;
	float3 normal : TEXCOORD3;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

////// Shader functions
Vert2Frag vertRim(VertexInput vertIn)
{
	Vert2Frag output;

	UNITY_SETUP_INSTANCE_ID(vertIn);
	UNITY_TRANSFER_INSTANCE_ID(vertIn, output); // necessary only if you want to access instanced properties in the fragment Shader.
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	output.pos = UnityObjectToClipPos(vertIn.vertex);
	output.posWorld = mul(unity_ObjectToWorld, vertIn.vertex);
	output.posLocal = vertIn.vertex;
	output.normal = vertIn.normal;

	return(output);
}

// Fragent shader
float4 fragRim(Vert2Frag fragIn, fixed4 color, fixed strength, fixed rimStrength)
{
	// Color
	fixed4 final = color * strength;

	// Rim
	float3 viewDir = normalize(ObjSpaceViewDir(fragIn.posLocal).xyz);
	float3 normalDirection = fragIn.normal;
	float rim;
	rim = 1 - dot(viewDir, normalDirection);
	rim = pow(rim, rimStrength);

	final.w *= rim;

	return final;
}