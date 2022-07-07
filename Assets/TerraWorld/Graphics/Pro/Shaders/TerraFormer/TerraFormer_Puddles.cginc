#ifndef TERRAFORMER_PUDDLES_CGINC_INCLUDED
#define TERRAFORMER_PUDDLES_CGINC_INCLUDED

float _Slope, _WaterHeight, _SlopeMin;
fixed4 _PuddleColor;
float _Refraction;
float _PuddleSmoothness;
float _PuddleMetallic;

void Puddles (half puddle, inout fixed3 albedo, inout fixed3 normal, inout float smoothness, inout float metallic, float _Smoothness, float _Metallic, fixed3 finalCol, float waterMask, float slopeDivider)
{
	puddle *= waterMask;

	if (puddle < _Slope / slopeDivider && puddle > _SlopeMin)
	{
		albedo = (finalCol * _Refraction) + _PuddleColor;
		normal = fixed3(0, 0, 1);
		smoothness = _PuddleSmoothness;
		metallic = _PuddleMetallic;
	}
	else
	{
		albedo = finalCol.rgb;
		smoothness = _Smoothness * 4;
	}
}

#endif // TERRAFORMER_PUDDLES_CGINC_INCLUDED

