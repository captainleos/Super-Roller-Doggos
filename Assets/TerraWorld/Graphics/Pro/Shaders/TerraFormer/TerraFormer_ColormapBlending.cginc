#ifndef TERRAFORMER_COLORMAPBLENDING_CGINC_INCLUDED
#define TERRAFORMER_COLORMAPBLENDING_CGINC_INCLUDED

sampler2D _ColorMap;
float _BlendingDistance;
float _Blend;

fixed3 ColormapBlending (float2 uv, fixed3 worldSpaceCameraPos, fixed3 worldPos, fixed4 mixedDiffuse, fixed4 _LightingColor)
{
	fixed3 colormap = tex2D (_ColorMap, uv);
	float cameraDistance = distance(worldSpaceCameraPos, worldPos);

	if (cameraDistance < _BlendingDistance)
	{
		float fadeout = pow(cameraDistance / _BlendingDistance, 1.0);
		return (lerp((((1 - _Blend) * mixedDiffuse) + (_Blend * fixed4(colormap, 0.0))), colormap, fadeout)) * _LightingColor;
	}
	else
		return colormap * _LightingColor;
}

#endif // TERRAFORMER_COLORMAPBLENDING_CGINC_INCLUDED

