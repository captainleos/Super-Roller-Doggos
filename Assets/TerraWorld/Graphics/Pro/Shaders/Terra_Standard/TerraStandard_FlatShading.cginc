#ifndef TERRASTANDARD_FLATSHADING_CGINC_INCLUDED
#define TERRASTANDARD_FLATSHADING_CGINC_INCLUDED

float _FlatShadingState;
float _FlatShadingAngleMin;
float _FlatShadingAngleMax;
float _FlatShadingStrengthObject;

void FlatShading (inout float3 vertex, float3 normal, float3 worldNormal, float2 texCoord)
{
    if (_FlatShadingState == 1)
    {
        if (1 - worldNormal.y >= _FlatShadingAngleMin && 1 - worldNormal.y <= _FlatShadingAngleMax)
		    vertex.xyz += normal * frac(sin(dot(texCoord, float2(12.9898, 78.233))) * 43758.5453123) * _FlatShadingStrengthObject;
	}
}

#endif // TERRASTANDARD_FLATSHADING_CGINC_INCLUDED

