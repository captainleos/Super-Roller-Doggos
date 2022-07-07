#ifndef TERRAFORMER_TESSELLATION_CGINC_INCLUDED
#define TERRAFORMER_TESSELLATION_CGINC_INCLUDED

//#include "TerraFormer_StochasticSampling_Vert.cginc"

//float _Phong;
float _EdgeLength;
float _Displacement1, _Displacement2, _Displacement3, _Displacement4, _Displacement5, _Displacement6, _Displacement7, _Displacement8;
float _HeightShift1, _HeightShift2, _HeightShift3, _HeightShift4, _HeightShift5, _HeightShift6, _HeightShift7, _HeightShift8;

/*
float TessellatorWithSnow
(
	fixed4 splatmap,
	float3 worldNormal,
	float2 splatUV,
	float2 splatUV0,
	float2 splatUV1,
	float2 splatUV2,
	float2 splatUV3,
	sampler2D _Noise,
	float _NoiseTiling1,
	float _NoiseTiling2,
	float _NoiseTiling3,
	float _NoiseTiling4,
	float _TilingRemover1,
	float _TilingRemover2,
	float _TilingRemover3,
	float _TilingRemover4,
	sampler2D _Mask0,
	sampler2D _Mask1,
	sampler2D _Mask2,
	sampler2D _Mask3,
	inout fixed3 vertexColor,
	fixed3 vertexNormal,
	float3 worldPos,
	float _SnowStartHeight,
	float _HeightFalloff,
	float _SnowAmount,
	float _SnowAngle,
	float _NormalInfluence,
	float _SnowPower,
	float _SnowThickness,
	float _SnowDamping
)
{
	//fixed splatSum = dot(splatmap, fixed4(1, 1, 1, 1));
	//half t = dot(worldNormal, normalize(half3(0, 1, 0)));
	
	float4 uv0 = float4(splatUV * splatUV0, 0, 0);
	float4 uv1 = float4(splatUV * splatUV1, 0, 0);
	float4 uv2 = float4(splatUV * splatUV2, 0, 0);
	float4 uv3 = float4(splatUV * splatUV3, 0, 0);
	float noise1 = tex2Dlod(_Noise, float4(splatUV * _NoiseTiling1, 0, 0)).r;
	float noise2 = tex2Dlod(_Noise, float4(splatUV * _NoiseTiling2, 0, 0)).r;
	float noise3 = tex2Dlod(_Noise, float4(splatUV * _NoiseTiling3, 0, 0)).r;
	float noise4 = tex2Dlod(_Noise, float4(splatUV * _NoiseTiling4, 0, 0)).r;
	if(_TilingRemover1 > 0.0f) uv0 = TilingRemoverVert(_TilingRemover1, uv0, noise1);
	if(_TilingRemover2 > 0.0f) uv1 = TilingRemoverVert(_TilingRemover2, uv1, noise2);
	if(_TilingRemover3 > 0.0f) uv2 = TilingRemoverVert(_TilingRemover3, uv2, noise3);
	if(_TilingRemover4 > 0.0f) uv3 = TilingRemoverVert(_TilingRemover4, uv3, noise4);

	float height1, height2, height3, height4;
	height1 = tex2Dlod(_Mask0, uv0).b + _HeightShift1;
	height2 = tex2Dlod(_Mask1, uv1).b + _HeightShift2;
	height3 = tex2Dlod(_Mask2, uv2).b + _HeightShift3;
	height4 = tex2Dlod(_Mask3, uv3).b + _HeightShift4;

	float d1 = height1 * splatmap.r * _Displacement1;
	float d2 = height2 * splatmap.g * _Displacement2;
	float d3 = height3 * splatmap.b * _Displacement3;
	float d4 = height4 * splatmap.a * _Displacement4;
	float displacement = 0;

//#ifdef _HEIGHTMAPBLENDING
//	displacement += heightlerp(d2, height2, d1, height1, splatmap.r);
//	displacement += heightlerp(d3, height3, d2, height2, splatmap.g);
//	displacement += heightlerp(d4, height4, d3, height3, splatmap.b);
//	displacement += d4;
//#else
	displacement += d1;
	displacement += d2;
	displacement += d3;
	displacement += d4;
//#endif

	//float scale = 0.0002;
	//float4 _WorldS = float4(scale, scale, scale, 0);
	//float4 _WorldT = float4(1, 1, 1, 0);
	//float3 worldPos = mul(unity_ObjectToWorld, v.vertex) * _WorldS.xyz + _WorldT.xyz;
	//float3 normal = abs(vertexNormal);
	//normal /= normal.x + normal.y + normal.z + 1e-3f;
	//
	//displacement  = splatmap.r * _Displacement1 *
	//(normal.x * tex2Dlod(_Mask0,  float4(_Splat0_ST.xy * worldPos.zy + _Splat0_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask0, float4(_Splat0_ST.xy * worldPos.xz + _Splat0_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask0, float4(_Splat0_ST.xy * worldPos.xy + _Splat0_ST.zw, 0, 0))).b;
	//
	//displacement += splatmap.g * _Displacement2 *
	//(normal.x * tex2Dlod(_Mask1, float4(_Splat1_ST.xy * worldPos.zy + _Splat1_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask1, float4(_Splat1_ST.xy * worldPos.xz + _Splat1_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask1, float4(_Splat1_ST.xy * worldPos.xy + _Splat1_ST.zw, 0, 0))).b;
	//
	//displacement += splatmap.b * _Displacement3 *
	//(normal.x * tex2Dlod(_Mask2, float4(_Splat2_ST.xy * worldPos.zy + _Splat2_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask2, float4(_Splat2_ST.xy * worldPos.xz + _Splat2_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask2, float4(_Splat2_ST.xy * worldPos.xy + _Splat2_ST.zw, 0, 0))).b;
	//
	//displacement += splatmap.a * _Displacement4 *
	//(normal.x * tex2Dlod(_Mask3, float4(_Splat3_ST.xy * worldPos.zy + _Splat3_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask3, float4(_Splat3_ST.xy * worldPos.xz + _Splat3_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask3, float4(_Splat3_ST.xy * worldPos.xy + _Splat3_ST.zw, 0, 0))).b;

	
#ifdef _PROCEDURALPUDDLES
	//float noise = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling, 0, 0)).r * _NoiseIntensity;
	//if (t > (1 - _Slope)) displacement = lerp(displacement, displacement - noise - _WaterHeight, _Slope * noise);
#endif

#ifdef _PROCEDURALSNOW
	//float3 worldNormal = mul(unity_ObjectToWorld, float4(vertexNormal, 0.0)).xyz;
	//float4 pos = UnityObjectToClipPos(v.vertex);
	//float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
	//float dist = distance(_WorldSpaceCameraPos, v.vertex.xyz);
	half differ = (worldPos.y - _SnowStartHeight);
	half cFactor = saturate(differ / (_HeightFalloff) + 0.5);
	vertexColor.rgb = vertexNormal;
	vertexColor = lerp(0, 1, cFactor);

	//float snowDisplacement = tex2Dlod(_SnowDiffuse, float4(v.texcoord.x * _SnowTile, v.texcoord.y * _SnowTile, 0, 0)).a;
	float snowAmount = (_SnowAmount * (clamp(worldNormal.y + _SnowAngle, 0, 1)) + clamp(vertexNormal.y + _NormalInfluence, 0, 1) * _SnowAmount * .25) * _SnowPower;
	snowAmount *= clamp((worldPos.y - _SnowStartHeight) * .0125, 0, 1);
	snowAmount = clamp(pow(snowAmount , 6) * 256, 0, 1);

	//displacement = lerp(displacement, (snowAmount * _SnowThickness) + (displacement / (_SnowThickness + 1)), snowAmount);
	//if (snowAmount > _SnowDamping) displacement += snowAmount * _SnowThickness;
	if (snowAmount > _SnowDamping) displacement = (snowAmount * _SnowThickness) + (displacement / ((_SnowThickness / 1) + 1)) - (_SnowThickness * _SnowDamping);
#endif

	//fixed splatSum = dot(splat_control, fixed4(1, 1, 1, 1));
	//float dist = distance(_WorldSpaceCameraPos, v.vertex.xyz);
	//displacement = lerp(t0.a * _Displacement1, t1.a * _Displacement2, t);
	//displacement -= (1 - splatSum) * dist * 0.005;
	//displacement -= fadeout;

	return displacement;
}

float Tessellator
(
	fixed4 splatmap,
	float3 worldNormal,
	float2 splatUV,
	float2 splatUV0,
	float2 splatUV1,
	float2 splatUV2,
	float2 splatUV3,
	sampler2D _Noise,
	float _NoiseTiling1,
	float _NoiseTiling2,
	float _NoiseTiling3,
	float _NoiseTiling4,
	float _TilingRemover1,
	float _TilingRemover2,
	float _TilingRemover3,
	float _TilingRemover4,
	sampler2D _Mask0,
	sampler2D _Mask1,
	sampler2D _Mask2,
	sampler2D _Mask3
)
{
	//fixed splatSum = dot(splatmap, fixed4(1, 1, 1, 1));
	//half t = dot(worldNormal, normalize(half3(0, 1, 0)));
	
	float4 uv0 = float4(splatUV * splatUV0, 0, 0);
	float4 uv1 = float4(splatUV * splatUV1, 0, 0);
	float4 uv2 = float4(splatUV * splatUV2, 0, 0);
	float4 uv3 = float4(splatUV * splatUV3, 0, 0);
	float noise1 = tex2Dlod(_Noise, float4(splatUV * _NoiseTiling1, 0, 0)).r;
	float noise2 = tex2Dlod(_Noise, float4(splatUV * _NoiseTiling2, 0, 0)).r;
	float noise3 = tex2Dlod(_Noise, float4(splatUV * _NoiseTiling3, 0, 0)).r;
	float noise4 = tex2Dlod(_Noise, float4(splatUV * _NoiseTiling4, 0, 0)).r;
	if(_TilingRemover1 > 0.0f) uv0 = TilingRemoverVert(_TilingRemover1, uv0, noise1);
	if(_TilingRemover2 > 0.0f) uv1 = TilingRemoverVert(_TilingRemover2, uv1, noise2);
	if(_TilingRemover3 > 0.0f) uv2 = TilingRemoverVert(_TilingRemover3, uv2, noise3);
	if(_TilingRemover4 > 0.0f) uv3 = TilingRemoverVert(_TilingRemover4, uv3, noise4);

	float height1, height2, height3, height4;
	height1 = tex2Dlod(_Mask0, uv0).b + _HeightShift1;
	height2 = tex2Dlod(_Mask1, uv1).b + _HeightShift2;
	height3 = tex2Dlod(_Mask2, uv2).b + _HeightShift3;
	height4 = tex2Dlod(_Mask3, uv3).b + _HeightShift4;

	float d1 = height1 * splatmap.r * _Displacement1;
	float d2 = height2 * splatmap.g * _Displacement2;
	float d3 = height3 * splatmap.b * _Displacement3;
	float d4 = height4 * splatmap.a * _Displacement4;
	float displacement = 0;

//#ifdef _HEIGHTMAPBLENDING
//	displacement += heightlerp(d2, height2, d1, height1, splatmap.r);
//	displacement += heightlerp(d3, height3, d2, height2, splatmap.g);
//	displacement += heightlerp(d4, height4, d3, height3, splatmap.b);
//	displacement += d4;
//#else
	displacement += d1;
	displacement += d2;
	displacement += d3;
	displacement += d4;
//#endif

	//float scale = 0.0002;
	//float4 _WorldS = float4(scale, scale, scale, 0);
	//float4 _WorldT = float4(1, 1, 1, 0);
	//float3 worldPos = mul(unity_ObjectToWorld, v.vertex) * _WorldS.xyz + _WorldT.xyz;
	//float3 normal = abs(vertexNormal);
	//normal /= normal.x + normal.y + normal.z + 1e-3f;
	//
	//displacement  = splatmap.r * _Displacement1 *
	//(normal.x * tex2Dlod(_Mask0,  float4(_Splat0_ST.xy * worldPos.zy + _Splat0_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask0, float4(_Splat0_ST.xy * worldPos.xz + _Splat0_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask0, float4(_Splat0_ST.xy * worldPos.xy + _Splat0_ST.zw, 0, 0))).b;
	//
	//displacement += splatmap.g * _Displacement2 *
	//(normal.x * tex2Dlod(_Mask1, float4(_Splat1_ST.xy * worldPos.zy + _Splat1_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask1, float4(_Splat1_ST.xy * worldPos.xz + _Splat1_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask1, float4(_Splat1_ST.xy * worldPos.xy + _Splat1_ST.zw, 0, 0))).b;
	//
	//displacement += splatmap.b * _Displacement3 *
	//(normal.x * tex2Dlod(_Mask2, float4(_Splat2_ST.xy * worldPos.zy + _Splat2_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask2, float4(_Splat2_ST.xy * worldPos.xz + _Splat2_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask2, float4(_Splat2_ST.xy * worldPos.xy + _Splat2_ST.zw, 0, 0))).b;
	//
	//displacement += splatmap.a * _Displacement4 *
	//(normal.x * tex2Dlod(_Mask3, float4(_Splat3_ST.xy * worldPos.zy + _Splat3_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask3, float4(_Splat3_ST.xy * worldPos.xz + _Splat3_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask3, float4(_Splat3_ST.xy * worldPos.xy + _Splat3_ST.zw, 0, 0))).b;

	
#ifdef _PROCEDURALPUDDLES
	//float noise = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling, 0, 0)).r * _NoiseIntensity;
	//if (t > (1 - _Slope)) displacement = lerp(displacement, displacement - noise - _WaterHeight, _Slope * noise);
#endif

	return displacement;
}
*/

#endif // TERRAFORMER_TESSELLATION_CGINC_INCLUDED

