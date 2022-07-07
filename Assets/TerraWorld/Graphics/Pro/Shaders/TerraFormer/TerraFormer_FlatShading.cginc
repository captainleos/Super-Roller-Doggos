#ifndef TERRAFORMER_FLATSHADING_CGINC_INCLUDED
#define TERRAFORMER_FLATSHADING_CGINC_INCLUDED

float _FlatShadingState;
float _FlatShadingAngleMin;
float _FlatShadingAngleMax;
float _FlatShadingStrengthTerrain;

void FlatShadingVert (inout float3 vertex, float3 normal, float3 worldNormal, float2 texCoord)
{
	if (_FlatShadingState == 1)
    {
		if (1 - worldNormal.y >= _FlatShadingAngleMin && 1 - worldNormal.y <= _FlatShadingAngleMax)
			vertex.xyz += normal * frac(sin(dot(texCoord, float2(12.9898, 78.233))) * 43758.5453123) * _FlatShadingStrengthTerrain;
	}
}

void FlatShadingFrag (inout float3 normal, half3 worldPos, half3 worldT, half3 worldB, half3 worldN)
{
	if (_FlatShadingState == 1)
	{
		half3 flatWorldNormal = normalize(cross(ddy(worldPos), ddx(worldPos)));
		half3x3 w2tRotation = half3x3(worldT, worldB, worldN);
		normal = mul(w2tRotation, flatWorldNormal);
	}
}

#endif // TERRAFORMER_FLATSHADING_CGINC_INCLUDED

