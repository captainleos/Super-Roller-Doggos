#ifndef TERRASTANDARD_SNOW_CGINC_INCLUDED
#define TERRASTANDARD_SNOW_CGINC_INCLUDED

float _ProceduralSnow;
float _SnowState;
fixed4 _SnowColor;
sampler2D _SnowTexture;
float _SnowTiling;
float4 _SnowDirection;
float _SnowLevel;
float _SnowSmoothness;
float _SnowStartHeight;
float _HeightFalloff;

void ProceduralSnow (inout half3 _outDiffuse, inout half _outSmoothness, float2 _uv, float _worldPosY, float3 _worldNormal)
{
    if (_SnowState == 1)
    {
		if (_ProceduralSnow == 1)
		{
			fixed4 snowColor = tex2D(_SnowTexture, _uv * _SnowTiling) * _SnowColor;
			half differ = (_worldPosY - _SnowStartHeight);
			half cFactor = saturate(differ / (_HeightFalloff) + 0.5);
			float3 color = _worldNormal;
			color = lerp(0, 1, cFactor);
			half snowDot = step(1 - _SnowLevel, dot(_worldNormal, normalize(_SnowDirection))) * color.y;
			//fixed3 snowNormals = UnpackNormalWithScale(tex2D(_SnowNormal, _uv * _SnowTiling), _SnowBumpScale);
			snowDot *= clamp((_worldPosY - _SnowStartHeight) * .0125, 0, 1);
			float snowSmoothness = snowColor.a * _SnowSmoothness;
			_outDiffuse = lerp(_outDiffuse, snowColor, snowDot);
			//_worldNormal = lerp(_worldNormal, normalize(_worldNormal + snowNormals), snowDot);
			_outSmoothness  = lerp(_outSmoothness, _outSmoothness + snowSmoothness, snowDot);
		}
		else
		{
			fixed4 snowColor = tex2D(_SnowTexture, _uv * _SnowTiling) * _SnowColor;
			//half differ = (_worldPosY - _SnowStartHeight);
			//half cFactor = saturate(differ / (_HeightFalloff) + 0.5);
			float3 color = _worldNormal;
			//color = lerp(0, 1, cFactor);
			half snowDot = step(1 - _SnowLevel, dot(_worldNormal, normalize(_SnowDirection))) * color.y;
			//fixed3 snowNormals = UnpackNormalWithScale(tex2D(_SnowNormal, _uv * _SnowTiling), _SnowBumpScale);
			//snowDot *= clamp((_worldPosY - _SnowStartHeight) * .0125, 0, 1);
			float snowSmoothness = snowColor.a * _SnowSmoothness;
			_outDiffuse = lerp(_outDiffuse, snowColor, snowDot);
			//_worldNormal = lerp(_worldNormal, normalize(_worldNormal + snowNormals), snowDot);
			_outSmoothness  = lerp(_outSmoothness, _outSmoothness + snowSmoothness, snowDot);
		}
	}
}

#endif // TERRASTANDARD_SNOW_CGINC_INCLUDED

