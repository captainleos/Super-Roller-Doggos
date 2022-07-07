#ifndef TERRAFORMER_SPLATMAPCOMMON_CGINC_INCLUDED
#define TERRAFORMER_SPLATMAPCOMMON_CGINC_INCLUDED

//// Since 2018.3 we changed from _TERRAIN_NORMAL_MAP to _NORMALMAP to save 1 keyword.
//// Since 2019.2 terrain keywords are changed to  local keywords so it doesn't really matter. You can use both.
//#if defined(_NORMALMAP) && !defined(_TERRAIN_NORMAL_MAP)
//    #define _TERRAIN_NORMAL_MAP
//#elif !defined(_NORMALMAP) && defined(_TERRAIN_NORMAL_MAP)
//    #define _NORMALMAP
//#endif

struct Input
{
	float2 uv_Control : TEXCOORD0;
	float4 color : COLOR;
	float3 worldNormal;
	float3 worldPos;

#ifndef TERRAIN_BASE_PASS
	UNITY_FOG_COORDS(0) // needed because finalcolor oppresses fog code generation.
#endif

	INTERNAL_DATA
};

float _Phong;
half _Metallic0;
half _Metallic1;
half _Metallic2;
half _Metallic3;
half _Smoothness0;
half _Smoothness1;
half _Smoothness2;
half _Smoothness3;

sampler2D _Control;
//float4 _Control_ST;
float4 _Control_TexelSize;
//sampler2D _Control0;
//sampler2D _Control1;

UNITY_DECLARE_TEX2D(_Splat0);
UNITY_DECLARE_TEX2D(_Splat1);
UNITY_DECLARE_TEX2D(_Splat2);
UNITY_DECLARE_TEX2D(_Splat3);

float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
sampler2D _Mask0, _Mask1, _Mask2, _Mask3;

//#ifdef TERRAIN_SPLAT_ADDPASS
//    UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap4);
//	UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap5);
//	UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap6);
//	UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap7);
//	float _TilingRemover5, _TilingRemover6, _TilingRemover7, _TilingRemover8;
//	float _NoiseTiling5, _NoiseTiling6, _NoiseTiling7, _NoiseTiling8;
//	fixed4 _LayerColor5, _LayerColor6, _LayerColor7, _LayerColor8;
//	half _LayerAO5, _LayerAO6, _LayerAO7, _LayerAO8;
//#else
    UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap0);
	UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap1);
	UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap2);
	UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap3);
	float _TilingRemover1, _TilingRemover2, _TilingRemover3, _TilingRemover4;
	float _NoiseTiling1, _NoiseTiling2, _NoiseTiling3, _NoiseTiling4;
	fixed4 _LayerColor1, _LayerColor2, _LayerColor3, _LayerColor4;
	half _LayerAO1, _LayerAO2, _LayerAO3, _LayerAO4;
//#endif

fixed4 _LightingColor;
//sampler2D _Noise;
float _NoiseTiling;
//float _NoiseIntensity;

UNITY_DECLARE_TEX2D_NOSAMPLER(_WaterMask);

//float _WeightOpacity;

struct appdata
{
	float4 vertex : POSITION;
	float4 color : COLOR;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float2 texcoord : TEXCOORD0;
	float2 texcoord1 : TEXCOORD1;
	float2 texcoord2 : TEXCOORD2;
	float3 worldNormal : TEXCOORD3;
};

float4 tessEdge(appdata v0, appdata v1, appdata v2)
{
	#ifdef _TESSELLATION
		return UnityEdgeLengthBasedTessCull(v0.vertex, v1.vertex, v2.vertex, _EdgeLength, 16 * 1.5f);
		//return UnityDistanceBasedTess (v0.vertex, v1.vertex, v2.vertex, 30, 100, _EdgeLength * 1.28);
	#else
		return float4(1, 1, 1, 1);
	#endif
}

//#ifdef _NORMALMAP
    sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
    float _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;
//#endif

#if defined(TERRAIN_BASE_PASS) && defined(UNITY_PASS_META)
    // When we render albedo for GI baking, we actually need to take the ST
    float4 _MainTex_ST;
#endif

void SplatmapVert(inout appdata v)
{
	//float textureSize = _TerrainSize / _SplatTiling1;
	//float textureSize = 15;
	//float2 xxx = TRANSFORM_TEX(v.texcoord, _Splat0);

	//float d = tex2Dlod(_Splat0, float4(v.texcoord.xy * _Splat0_ST.xy, 0, 0)).a * _Depth1;
	//float d = tex2Dlod(_Splat0, float4(v.texcoord.xy * textureSize, 0, 0)).a * _Depth1;
	//float d = splat_control.r * tex2Dlod(_Heightmap0, float4(v.texcoord.xy * _SplatTiling1, 0, 0)).r * _Depth1;

	//float fade1 = 1 - splat_control.r;

    float3 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz;

    // Flat Shading
#ifdef _FLATSHADING
    FlatShadingVert(v.vertex.xyz, v.normal, worldNormal, v.texcoord.xy);
#endif

#ifdef _TESSELLATION
	fixed4 splat_control = tex2Dlod(_Control, float4(v.texcoord.xy, 0, 0));
	//fixed splatSum = dot(splat_control, fixed4(1, 1, 1, 1));

	half t = dot(worldNormal, normalize(half3(0, 1, 0)));
	float height1, height2, height3, height4;
	float4 uv0 = float4(v.texcoord.xy * _Splat0_ST.xy, 0, 0);
	float4 uv1 = float4(v.texcoord.xy * _Splat1_ST.xy, 0, 0);
	float4 uv2 = float4(v.texcoord.xy * _Splat2_ST.xy, 0, 0);
	float4 uv3 = float4(v.texcoord.xy * _Splat3_ST.xy, 0, 0);

//#ifdef TERRAIN_SPLAT_ADDPASS
//    float noise1 = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling5, 0, 0)).r;
//	float noise2 = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling6, 0, 0)).r;
//	float noise3 = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling7, 0, 0)).r;
//	float noise4 = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling8, 0, 0)).r;
//
//	if(_TilingRemover5 > 0.0f) uv0 = TilingRemoverVert(_TilingRemover5, uv0, noise1);
//	if(_TilingRemover6 > 0.0f) uv1 = TilingRemoverVert(_TilingRemover6, uv1, noise2);
//	if(_TilingRemover7 > 0.0f) uv2 = TilingRemoverVert(_TilingRemover7, uv2, noise3);
//	if(_TilingRemover8 > 0.0f) uv3 = TilingRemoverVert(_TilingRemover8, uv3, noise4);
//
//	height1 = tex2Dlod(_Mask0, uv0).b + _HeightShift5;
//	height2 = tex2Dlod(_Mask1, uv1).b + _HeightShift6;
//	height3 = tex2Dlod(_Mask2, uv2).b + _HeightShift7;
//	height4 = tex2Dlod(_Mask3, uv3).b + _HeightShift8;
//
//	float d1 = height1 * splat_control.r * _Displacement5;
//	float d2 = height2 * splat_control.g * _Displacement6;
//	float d3 = height3 * splat_control.b * _Displacement7;
//	float d4 = height4 * splat_control.a * _Displacement8;
//#else

    //float noise1 = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling1, 0, 0)).r;
	//float noise2 = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling2, 0, 0)).r;
	//float noise3 = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling3, 0, 0)).r;
	//float noise4 = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling4, 0, 0)).r;
	//
	//if(_TilingRemover1 > 0.0f) uv0 = TilingRemoverVert(_TilingRemover1, uv0, noise1, _Noise);
	//if(_TilingRemover2 > 0.0f) uv1 = TilingRemoverVert(_TilingRemover2, uv1, noise2, _Noise);
	//if(_TilingRemover3 > 0.0f) uv2 = TilingRemoverVert(_TilingRemover3, uv2, noise3, _Noise);
	//if(_TilingRemover4 > 0.0f) uv3 = TilingRemoverVert(_TilingRemover4, uv3, noise4, _Noise);

	if (_TilingRemover1 > 0.0f)
	{
		float noise = SimplexNoise(_NoiseTiling1 * float3(v.texcoord.xy, 0));
		uv0 = TilingRemoverVert(_TilingRemover1, uv0, noise);
	}
	
	if (_TilingRemover2 > 0.0f)
	{
		float noise = SimplexNoise(_NoiseTiling2 * float3(v.texcoord.xy, 0));
		uv1 = TilingRemoverVert(_TilingRemover2, uv1, noise);
	}
	
	if (_TilingRemover3 > 0.0f)
	{
		float noise = SimplexNoise(_NoiseTiling3 * float3(v.texcoord.xy, 0));
		uv2 = TilingRemoverVert(_TilingRemover3, uv2, noise);
	}

	if (_TilingRemover4 > 0.0f)
	{
		float noise = SimplexNoise(_NoiseTiling4 * float3(v.texcoord.xy, 0));
		uv3 = TilingRemoverVert(_TilingRemover4, uv3, noise);
	}

	//if(_TilingRemover1 > 0.0f) uv0 = TilingRemoverVert2(_Noise, uv0, _TilingRemover1);
	//if(_TilingRemover2 > 0.0f) uv1 = TilingRemoverVert2(_Noise, uv1, _TilingRemover2);
	//if(_TilingRemover3 > 0.0f) uv2 = TilingRemoverVert2(_Noise, uv2, _TilingRemover3);
	//if(_TilingRemover4 > 0.0f) uv3 = TilingRemoverVert2(_Noise, uv3, _TilingRemover4);

	height1 = tex2Dlod(_Mask0, uv0).b + _HeightShift1;
	height2 = tex2Dlod(_Mask1, uv1).b + _HeightShift2;
	height3 = tex2Dlod(_Mask2, uv2).b + _HeightShift3;
	height4 = tex2Dlod(_Mask3, uv3).b + _HeightShift4;

	float d1 = height1 * splat_control.r * _Displacement1;
	float d2 = height2 * splat_control.g * _Displacement2;
	float d3 = height3 * splat_control.b * _Displacement3;
	float d4 = height4 * splat_control.a * _Displacement4;
//#endif

	float d = 0;

#ifdef _HEIGHTMAPBLENDING
	d += heightlerp(d2, height2, d1, height1, splat_control.r);
	d += heightlerp(d3, height3, d2, height2, splat_control.g);
	d += heightlerp(d4, height4, d3, height3, splat_control.b);
	d += d4;
#else
	d += d1;
	d += d2;
	d += d3;
	d += d4;

	//float scale = 0.0002;
	//float4 _WorldS = float4(scale, scale, scale, 0);
	//float4 _WorldT = float4(1, 1, 1, 0);
	//float3 worldPos = mul(unity_ObjectToWorld, v.vertex) * _WorldS.xyz + _WorldT.xyz;
	//float3 normal = abs(v.normal);
	//normal /= normal.x + normal.y + normal.z + 1e-3f;
	//
	//d  = splat_control.r * _Displacement1 *
	//(normal.x * tex2Dlod(_Mask0,  float4(_Splat0_ST.xy * worldPos.zy + _Splat0_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask0, float4(_Splat0_ST.xy * worldPos.xz + _Splat0_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask0, float4(_Splat0_ST.xy * worldPos.xy + _Splat0_ST.zw, 0, 0))).b;
	//
	//d += splat_control.g * _Displacement2 *
	//(normal.x * tex2Dlod(_Mask1, float4(_Splat1_ST.xy * worldPos.zy + _Splat1_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask1, float4(_Splat1_ST.xy * worldPos.xz + _Splat1_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask1, float4(_Splat1_ST.xy * worldPos.xy + _Splat1_ST.zw, 0, 0))).b;
	//
	//d += splat_control.b * _Displacement3 *
	//(normal.x * tex2Dlod(_Mask2, float4(_Splat2_ST.xy * worldPos.zy + _Splat2_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask2, float4(_Splat2_ST.xy * worldPos.xz + _Splat2_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask2, float4(_Splat2_ST.xy * worldPos.xy + _Splat2_ST.zw, 0, 0))).b;
	//
	//d += splat_control.a * _Displacement4 *
	//(normal.x * tex2Dlod(_Mask3, float4(_Splat3_ST.xy * worldPos.zy + _Splat3_ST.zw, 0, 0)) +
	//normal.y * tex2Dlod(_Mask3, float4(_Splat3_ST.xy * worldPos.xz + _Splat3_ST.zw, 0, 0)) +
	//normal.z * tex2Dlod(_Mask3, float4(_Splat3_ST.xy * worldPos.xy + _Splat3_ST.zw, 0, 0))).b;
#endif

//#ifdef _PROCEDURALPUDDLES
//	float noise = tex2Dlod(_Noise, float4(v.texcoord.xy * _NoiseTiling, 0, 0)).r * _NoiseIntensity;
//	if (t > (1 - _Slope)) d = lerp(d, d - noise - _WaterHeight, _Slope * noise);
//#endif

	//v.color.a = d;

#ifdef _PROCEDURALSNOW
	//float4 pos = UnityObjectToClipPos(v.vertex);
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
	//float dist = distance(_WorldSpaceCameraPos, v.vertex.xyz);
	half differ = (worldPos.y - _SnowStartHeight);
	half cFactor = saturate(differ / (_HeightFalloff) + 0.5);
	v.color.rgb = v.normal;
	v.color = lerp(0, 1, cFactor);

	//d += SnowWithFlowVert(worldNormal, v.texcoord.xy, v.color.y, worldPos, 1, splat_control);
#endif

	//fixed splatSum = dot(splat_control, fixed4(1, 1, 1, 1));
	//float dist = distance(_WorldSpaceCameraPos, v.vertex.xyz);
	//d = lerp(t0.a * _Displacement1, t1.a * _Displacement2, t);
	//d -= (1 - splatSum) * dist * 0.005;
	//d -= fadeout;

	v.vertex.xyz += v.normal * d;
#endif

//#ifdef _TESSELLATION
//float displacement = 0;
//#ifdef _PROCEDURALSNOW
//	displacement = TessellatorWithSnow(tex2Dlod(_Control, float4(v.texcoord.xy, 0, 0)), worldNormal, v.texcoord.xy, _Splat0_ST.xy, _Splat1_ST.xy, _Splat2_ST.xy, _Splat3_ST.xy, _Noise, _NoiseTiling1, _NoiseTiling2, _NoiseTiling3, _NoiseTiling4, _TilingRemover1, _TilingRemover2, _TilingRemover3, _TilingRemover4, _Mask0, _Mask1, _Mask2, _Mask3, v.color.rgb, v.normal, mul(unity_ObjectToWorld, v.vertex), _SnowStartHeight, _HeightFalloff, _SnowAmount, _SnowAngle, _NormalInfluence, _SnowPower, _SnowThickness, _SnowDamping);
//#else
//	displacement = Tessellator(tex2Dlod(_Control, float4(v.texcoord.xy, 0, 0)), worldNormal, v.texcoord.xy, _Splat0_ST.xy, _Splat1_ST.xy, _Splat2_ST.xy, _Splat3_ST.xy, _Noise, _NoiseTiling1, _NoiseTiling2, _NoiseTiling3, _NoiseTiling4, _TilingRemover1, _TilingRemover2, _TilingRemover3, _TilingRemover4, _Mask0, _Mask1, _Mask2, _Mask3);
//#endif
//	v.vertex.xyz += v.normal * displacement;
//	v.color.a = displacement;
//#endif

	v.tangent.xyz = cross(v.normal, float3(0, 0, 1));
	v.tangent.w = -1;
}

//inline fixed4 Triplanar(float3 wp, float3 n, float4 st, sampler2D s)
//{
//	return	n.x * UNITY_SAMPLE_TEX2D(s, st.xy * wp.zy + st.zw) +
//			n.y * UNITY_SAMPLE_TEX2D(s, st.xy * wp.xz + st.zw) +
//			n.z * UNITY_SAMPLE_TEX2D(s, st.xy * wp.xy + st.zw);
//}

#if UNITY_VERSION >= 201930
	#ifdef _ALPHATEST_ON
		sampler2D _TerrainHolesTexture;
		//UNITY_DECLARE_TEX2D_NOSAMPLER(_TerrainHolesTexture);

		void ClipHoles(float2 uv)
		{
			float hole = tex2D(_TerrainHolesTexture, uv).r;
			//float hole = UNITY_SAMPLE_TEX2D_SAMPLER(_TerrainHolesTexture, unity_Lightmap, uv).r;
			clip(hole == 0.0f ? -1 : 1);
		}
	#endif
#endif

#ifndef TERRAIN_BASE_PASS

void SplatmapMix(Input IN, half4 defaultAlpha, out half4 splat_control, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal, out half _Smoothness, out half _Metallic, out half _AO)
{
#if UNITY_VERSION >= 201930
	#ifdef _ALPHATEST_ON
        ClipHoles(IN.uv_Control);
    #endif
#endif

	//float3 worldNormal = WorldNormalVector(IN, mixedNormal);
	//half t = dot(worldNormal, normalize(half3(0, 1, 0)));

	// adjust splatUVs so the edges of the terrain tile lie on pixel centers
    float2 splatUV = (IN.uv_Control * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
    splat_control = tex2D(_Control, splatUV);
    //weight = dot(splat_control, half4(1,1,1,1));
	weight = 1;

    #if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
        clip(weight == 0.0f ? -1 : 1);
    #endif

    // Normalize weights before lighting and restore weights in final modifier functions so that the overal
    // lighting result can be correctly weighted.
    splat_control /= (weight + 1e-3f);

	float height1, height2, height3, height4;
	float2 uv0 = splatUV * _Splat0_ST.xy;
	float2 uv1 = splatUV * _Splat1_ST.xy;
	float2 uv2 = splatUV * _Splat2_ST.xy;
	float2 uv3 = splatUV * _Splat3_ST.xy;

//#ifdef TERRAIN_SPLAT_ADDPASS
//	float noise1 = tex2D(_Noise, splatUV * _NoiseTiling5).r;
//	float noise2 = tex2D(_Noise, splatUV * _NoiseTiling6).r;
//	float noise3 = tex2D(_Noise, splatUV * _NoiseTiling7).r;
//	float noise4 = tex2D(_Noise, splatUV * _NoiseTiling8).r;
//
//	if(_TilingRemover5 > 0.0f) uv0 = TilingRemoverSurf(_TilingRemover5, uv0, noise1);
//	if(_TilingRemover6 > 0.0f) uv1 = TilingRemoverSurf(_TilingRemover6, uv1, noise2);
//	if(_TilingRemover7 > 0.0f) uv2 = TilingRemoverSurf(_TilingRemover7, uv2, noise3);
//	if(_TilingRemover8 > 0.0f) uv3 = TilingRemoverSurf(_TilingRemover8, uv3, noise4);
//
//	fixed4 t0 = UNITY_SAMPLE_TEX2D(_Splat0, uv0) * _LayerColor5;
//	fixed4 t1 = UNITY_SAMPLE_TEX2D(_Splat1, uv1) * _LayerColor6;
//	fixed4 t2 = UNITY_SAMPLE_TEX2D(_Splat2, uv2) * _LayerColor7;
//	fixed4 t3 = UNITY_SAMPLE_TEX2D(_Splat3, uv3) * _LayerColor8;
//
//	float4 maskMap1 = UNITY_SAMPLE_TEX2D_SAMPLER(_Maskmap4, _Splat0, uv0);
//	float4 maskMap2 = UNITY_SAMPLE_TEX2D_SAMPLER(_Maskmap5, _Splat1, uv1);
//	float4 maskMap3 = UNITY_SAMPLE_TEX2D_SAMPLER(_Maskmap6, _Splat2, uv2);
//	float4 maskMap4 = UNITY_SAMPLE_TEX2D_SAMPLER(_Maskmap7, _Splat3, uv3);
//
//	_Metallic  = splat_control.r * maskMap1.r * _Metallic0;
//	_Metallic += splat_control.g * maskMap2.r * _Metallic1;
//	_Metallic += splat_control.b * maskMap3.r * _Metallic2;
//	_Metallic += splat_control.a * maskMap4.r * _Metallic3;
//
//	_Smoothness  = splat_control.r * maskMap1.a * _Smoothness0;
//	_Smoothness += splat_control.g * maskMap2.a * _Smoothness1;
//	_Smoothness += splat_control.b * maskMap3.a * _Smoothness2;
//	_Smoothness += splat_control.a * maskMap4.a * _Smoothness3;
//
//	_AO  = splat_control.r * maskMap1.g * _LayerAO5;
//	_AO += splat_control.g * maskMap2.g * _LayerAO6;
//	_AO += splat_control.b * maskMap3.g * _LayerAO7;
//	_AO += splat_control.a * maskMap4.g * _LayerAO8;
//
//	//fixed4 _Diffuse1 = splat_control.r * t0 * half4(1.0, 1.0, 1.0, maskMap1.a);
//	//fixed4 _Diffuse2 = splat_control.g * t1 * half4(1.0, 1.0, 1.0, maskMap2.a);
//	//fixed4 _Diffuse3 = splat_control.b * t2 * half4(1.0, 1.0, 1.0, maskMap3.a);
//	//fixed4 _Diffuse4 = splat_control.a * t3 * half4(1.0, 1.0, 1.0, maskMap4.a);
//
//	fixed4 _Diffuse1 = splat_control.r * t0 * half4(1.0, 1.0, 1.0, defaultAlpha.r);
//	fixed4 _Diffuse2 = splat_control.g * t1 * half4(1.0, 1.0, 1.0, defaultAlpha.g);
//	fixed4 _Diffuse3 = splat_control.b * t2 * half4(1.0, 1.0, 1.0, defaultAlpha.b);
//	fixed4 _Diffuse4 = splat_control.a * t3 * half4(1.0, 1.0, 1.0, defaultAlpha.a);
//
//	#ifdef _HEIGHTMAPBLENDING
//		height1 = splat_control.r * maskMap1.b;
//		height2 = splat_control.g * maskMap2.b;
//		height3 = splat_control.b * maskMap3.b;
//		height4 = splat_control.a * maskMap4.b;
//		mixedDiffuse  = heightlerp(_Diffuse2, height2, _Diffuse1, height1, splat_control.r);
//		mixedDiffuse += heightlerp(_Diffuse3, height3, _Diffuse2, height2, splat_control.g);
//		mixedDiffuse += heightlerp(_Diffuse4, height4, _Diffuse3, height3, splat_control.b);
//		mixedDiffuse += _Diffuse4;
//	#else
//		mixedDiffuse  = _Diffuse1;
//		mixedDiffuse += _Diffuse2;
//		mixedDiffuse += _Diffuse3;
//		mixedDiffuse += _Diffuse4;
//	#endif
//#else

	//float noise1 = tex2D(_Noise, splatUV * _NoiseTiling1).r;
	//float noise2 = tex2D(_Noise, splatUV * _NoiseTiling2).r;
	//float noise3 = tex2D(_Noise, splatUV * _NoiseTiling3).r;
	//float noise4 = tex2D(_Noise, splatUV * _NoiseTiling4).r;
	//
	//if(_TilingRemover1 > 0.0f) uv0 = TilingRemoverSurf(_TilingRemover1, uv0, noise1, _Noise);
	//if(_TilingRemover2 > 0.0f) uv1 = TilingRemoverSurf(_TilingRemover2, uv1, noise2, _Noise);
	//if(_TilingRemover3 > 0.0f) uv2 = TilingRemoverSurf(_TilingRemover3, uv2, noise3, _Noise);
	//if(_TilingRemover4 > 0.0f) uv3 = TilingRemoverSurf(_TilingRemover4, uv3, noise4, _Noise);

	if (_TilingRemover1 > 0.0f)
	{
		float noise = 1.0 - SimplexNoise(float3(_NoiseTiling1, _NoiseTiling1, _NoiseTiling1) * normalize(IN.worldPos));
		uv0 = TilingRemoverSurf(_TilingRemover1, uv0, noise);
	}

	if (_TilingRemover2 > 0.0f)
	{
		float noise = 1.0 - SimplexNoise(float3(_NoiseTiling2, _NoiseTiling2, _NoiseTiling2) * normalize(IN.worldPos));
		uv1 = TilingRemoverSurf(_TilingRemover2, uv1, noise);
	}

	if (_TilingRemover3 > 0.0f)
	{
		float noise = 1.0 - SimplexNoise(float3(_NoiseTiling3, _NoiseTiling3, _NoiseTiling3) * normalize(IN.worldPos));
		uv2 = TilingRemoverSurf(_TilingRemover3, uv2, noise);
	}

	if (_TilingRemover4 > 0.0f)
	{
		float noise = 1.0 - SimplexNoise(float3(_NoiseTiling4, _NoiseTiling4, _NoiseTiling4) * normalize(IN.worldPos));
		uv3 = TilingRemoverSurf(_TilingRemover4, uv3, noise);
	}

	//if(_TilingRemover1 > 0.0f) uv0 = TilingRemoverSurf2(_Noise, uv0, _TilingRemover1);
	//if(_TilingRemover2 > 0.0f) uv1 = TilingRemoverSurf2(_Noise, uv1, _TilingRemover2);
	//if(_TilingRemover3 > 0.0f) uv2 = TilingRemoverSurf2(_Noise, uv2, _TilingRemover3);
	//if(_TilingRemover4 > 0.0f) uv3 = TilingRemoverSurf2(_Noise, uv3, _TilingRemover4);

	fixed4 t0 = UNITY_SAMPLE_TEX2D(_Splat0, uv0) * _LayerColor1;
	fixed4 t1 = UNITY_SAMPLE_TEX2D(_Splat1, uv1) * _LayerColor2;
	fixed4 t2 = UNITY_SAMPLE_TEX2D(_Splat2, uv2) * _LayerColor3;
	fixed4 t3 = UNITY_SAMPLE_TEX2D(_Splat3, uv3) * _LayerColor4;

	float4 maskMap1 = UNITY_SAMPLE_TEX2D_SAMPLER(_Maskmap0, _Splat0, uv0);
	float4 maskMap2 = UNITY_SAMPLE_TEX2D_SAMPLER(_Maskmap1, _Splat1, uv1);
	float4 maskMap3 = UNITY_SAMPLE_TEX2D_SAMPLER(_Maskmap2, _Splat2, uv2);
	float4 maskMap4 = UNITY_SAMPLE_TEX2D_SAMPLER(_Maskmap3, _Splat3, uv3);

	_Metallic  = splat_control.r * maskMap1.r * _Metallic0;
	_Metallic += splat_control.g * maskMap2.r * _Metallic1;
	_Metallic += splat_control.b * maskMap3.r * _Metallic2;
	_Metallic += splat_control.a * maskMap4.r * _Metallic3;

	_Smoothness  = splat_control.r * maskMap1.a * _Smoothness0;
	_Smoothness += splat_control.g * maskMap2.a * _Smoothness1;
	_Smoothness += splat_control.b * maskMap3.a * _Smoothness2;
	_Smoothness += splat_control.a * maskMap4.a * _Smoothness3;

	_AO  = splat_control.r * maskMap1.g * _LayerAO1;
	_AO += splat_control.g * maskMap2.g * _LayerAO2;
	_AO += splat_control.b * maskMap3.g * _LayerAO3;
	_AO += splat_control.a * maskMap4.g * _LayerAO4;

	//fixed4 _Diffuse1 = splat_control.r * t0; // * half4(1.0, 1.0, 1.0, maskMap1.a);
	//fixed4 _Diffuse2 = splat_control.g * t1; // * half4(1.0, 1.0, 1.0, maskMap2.a);
	//fixed4 _Diffuse3 = splat_control.b * t2; // * half4(1.0, 1.0, 1.0, maskMap3.a);
	//fixed4 _Diffuse4 = splat_control.a * t3; // * half4(1.0, 1.0, 1.0, maskMap4.a);

	fixed4 _Diffuse1 = splat_control.r * t0 * half4(1.0, 1.0, 1.0, defaultAlpha.r);
	fixed4 _Diffuse2 = splat_control.g * t1 * half4(1.0, 1.0, 1.0, defaultAlpha.g);
	fixed4 _Diffuse3 = splat_control.b * t2 * half4(1.0, 1.0, 1.0, defaultAlpha.b);
	fixed4 _Diffuse4 = splat_control.a * t3 * half4(1.0, 1.0, 1.0, defaultAlpha.a);

	#ifdef _HEIGHTMAPBLENDING
		height1 = splat_control.r * maskMap1.b;
		height2 = splat_control.g * maskMap2.b;
		height3 = splat_control.b * maskMap3.b;
		height4 = splat_control.a * maskMap4.b;
		mixedDiffuse  = heightlerp(_Diffuse2, height2, _Diffuse1, height1, splat_control.r);
		mixedDiffuse += heightlerp(_Diffuse3, height3, _Diffuse2, height2, splat_control.g);
		mixedDiffuse += heightlerp(_Diffuse4, height4, _Diffuse3, height3, splat_control.b);
		mixedDiffuse += _Diffuse4;
	#else
		mixedDiffuse  = _Diffuse1;
		mixedDiffuse += _Diffuse2;
		mixedDiffuse += _Diffuse3;
		mixedDiffuse += _Diffuse4;

		//half4 n0 = tex2D(_Normal0, uv0);
		//half4 n1 = tex2D(_Normal1, uv1);
		//half4 n2 = tex2D(_Normal2, uv2);
		//half4 n3 = tex2D(_Normal3, uv3);
		//
		//fixed3 _Normal1 = UnpackNormalWithScale(n0, _NormalScale0) * splat_control.r;
		//fixed3 _Normal2 = UnpackNormalWithScale(n1, _NormalScale1) * splat_control.g;
		//fixed3 _Normal3 = UnpackNormalWithScale(n2, _NormalScale2) * splat_control.b;
		//fixed3 _Normal4 = UnpackNormalWithScale(n3, _NormalScale3) * splat_control.a;
		//fixed3 nrm  = _Normal1;
		//nrm += _Normal2;
		//nrm += _Normal3;
		//nrm += _Normal4;
		//
		//float3 worldNormal = WorldNormalVector(IN, nrm);
		//
		//float scale = 0.0002;
		//float4 _WorldS = float4(scale, scale, scale, 0);
		//float4 _WorldT = float4(1, 1, 1, 0);
		//float3 worldPos = IN.worldPos * _WorldS.xyz + _WorldT.xyz;
		//float3 normal = abs(IN.color);
		//normal /= normal.x + normal.y + normal.z + 1e-3f;
		//
		//mixedDiffuse  = splat_control.r *
		//(normal.x * UNITY_SAMPLE_TEX2D(_Splat0, _Splat0_ST.xy * worldPos.zy + _Splat0_ST.zw) +
		//normal.y  * UNITY_SAMPLE_TEX2D(_Splat0, _Splat0_ST.xy * worldPos.xz + _Splat0_ST.zw) +
		//normal.z  * UNITY_SAMPLE_TEX2D(_Splat0, _Splat0_ST.xy * worldPos.xy + _Splat0_ST.zw));
		//
		//mixedDiffuse += splat_control.g *
		//(normal.x * UNITY_SAMPLE_TEX2D(_Splat1, _Splat1_ST.xy * worldPos.zy + _Splat1_ST.zw) +
		//normal.y  * UNITY_SAMPLE_TEX2D(_Splat1, _Splat1_ST.xy * worldPos.xz + _Splat1_ST.zw) +
		//normal.z  * UNITY_SAMPLE_TEX2D(_Splat1, _Splat1_ST.xy * worldPos.xy + _Splat1_ST.zw));
		//
		//mixedDiffuse += splat_control.b *
		//(normal.x * UNITY_SAMPLE_TEX2D(_Splat2, _Splat2_ST.xy * worldPos.zy + _Splat2_ST.zw) +
		//normal.y  * UNITY_SAMPLE_TEX2D(_Splat2, _Splat2_ST.xy * worldPos.xz + _Splat2_ST.zw) +
		//normal.z  * UNITY_SAMPLE_TEX2D(_Splat2, _Splat2_ST.xy * worldPos.xy + _Splat2_ST.zw));
		//
		//mixedDiffuse += splat_control.a *
		//(normal.x * UNITY_SAMPLE_TEX2D(_Splat3, _Splat3_ST.xy * worldPos.zy + _Splat3_ST.zw) +
		//normal.y  * UNITY_SAMPLE_TEX2D(_Splat3, _Splat3_ST.xy * worldPos.xz + _Splat3_ST.zw) +
		//normal.z  * UNITY_SAMPLE_TEX2D(_Splat3, _Splat3_ST.xy * worldPos.xy + _Splat3_ST.zw));
	#endif
//#endif

//#ifdef _NORMALMAP
	half4 n0 = tex2D(_Normal0, uv0);
	half4 n1 = tex2D(_Normal1, uv1);
	half4 n2 = tex2D(_Normal2, uv2);
	half4 n3 = tex2D(_Normal3, uv3);

	fixed3 _Normal1 = UnpackNormalWithScale(n0, _NormalScale0) * splat_control.r;
	fixed3 _Normal2 = UnpackNormalWithScale(n1, _NormalScale1) * splat_control.g;
	fixed3 _Normal3 = UnpackNormalWithScale(n2, _NormalScale2) * splat_control.b;
	fixed3 _Normal4 = UnpackNormalWithScale(n3, _NormalScale3) * splat_control.a;

	//#ifdef _HEIGHTMAPBLENDING
	//	mixedNormal  = heightlerp(_Normal2, height2, _Normal1, height1, splat_control.r);
	//	mixedNormal += heightlerp(_Normal3, height3, _Normal2, height2, splat_control.g);
	//	mixedNormal += heightlerp(_Normal4, height4, _Normal3, height3, splat_control.b);
	//	mixedNormal += _Normal4;
	//#else
		mixedNormal  = _Normal1;
		mixedNormal += _Normal2;
		mixedNormal += _Normal3;
		mixedNormal += _Normal4;
	//#endif

	mixedNormal.z += 1e-5f; // to avoid nan after normalizing
//#endif
}

#ifndef TERRAIN_SURFACE_OUTPUT
	#define TERRAIN_SURFACE_OUTPUT SurfaceOutput
#endif

void SplatmapFinalColor(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 color)
{
	color *= o.Alpha;

#ifdef TERRAIN_SPLAT_ADDPASS
	UNITY_APPLY_FOG_COLOR(IN.fogCoord, color, fixed4(0,0,0,0));
#else
	UNITY_APPLY_FOG(IN.fogCoord, color);
#endif
}

void SplatmapFinalPrepass(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 normalSpec)
{
	normalSpec *= o.Alpha;
}

void SplatmapFinalGBuffer(Input IN, TERRAIN_SURFACE_OUTPUT o, inout half4 outGBuffer0, inout half4 outGBuffer1, inout half4 outGBuffer2, inout half4 emission)
{
	UnityStandardDataApplyWeightToGbuffer(outGBuffer0, outGBuffer1, outGBuffer2, o.Alpha);
	emission *= o.Alpha;
}

void surf (Input IN, inout SurfaceOutputStandard o)
{
    half4 splat_control;
    half weight;
    fixed4 mixedDiffuse;
	half4 defaultSmoothness = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);
	half _Metallic;
	half _Smoothness;
	half _AO;

	SplatmapMix(IN, defaultSmoothness, splat_control, weight, mixedDiffuse, o.Normal, _Smoothness, _Metallic, _AO);

	fixed3 finalCol;

#ifdef _FLATSHADING
    FlatShadingFrag(o.Normal, IN.worldPos.xyz, WorldNormalVector(IN, half3(1, 0, 0)), WorldNormalVector(IN, half3(0, 1, 0)), WorldNormalVector(IN, half3(0, 0, 1)));
#endif

#ifdef _COLORMAPBLENDING
	finalCol = ColormapBlending(IN.uv_Control, _WorldSpaceCameraPos, IN.worldPos, mixedDiffuse, _LightingColor);
#else
	finalCol = mixedDiffuse * _LightingColor;
#endif

half3 _worldNormal = WorldNormalVector(IN, o.Normal);
float waterMask = 0;

#if defined(_PROCEDURALPUDDLES) || defined(_PROCEDURALSNOW)
	waterMask = 1 - UNITY_SAMPLE_TEX2D_SAMPLER(_WaterMask, _Splat0, IN.uv_Control).a;
#endif

#ifdef _PROCEDURALPUDDLES
	half slope = 1 - dot(_worldNormal, normalize(half3(0, 1, 0)));
	
	//float noise = tex2D(_Noise, IN.uv_Control * _NoiseTiling).r;
	float noise = 1.0 - SimplexNoise(_NoiseTiling * normalize(IN.worldPos));
	
	half puddle = slope * noise;
	//puddle -= splat_control.gba;
	Puddles(puddle, o.Albedo, o.Normal, o.Smoothness, o.Metallic, _Smoothness, _Metallic, finalCol.rgb, waterMask, 3);

	#ifdef _PROCEDURALSNOW
		if (puddle > _Slope || puddle < _SlopeMin)
		{
			SnowWithFlow(o.Albedo, o.Smoothness, o.Normal, _worldNormal, IN.uv_Control, IN.color.y, IN.worldPos.y, finalCol.rgb, _Smoothness, waterMask, splat_control);
			o.Metallic = _Metallic * 4;
		}
	#endif
#else
	#ifdef _PROCEDURALSNOW
		SnowWithFlow(o.Albedo, o.Smoothness, o.Normal, _worldNormal, IN.uv_Control, IN.color.y, IN.worldPos.y, finalCol, _Smoothness, waterMask, splat_control);
	#else
		o.Albedo = finalCol;
		o.Smoothness = _Smoothness * 4;
	#endif

	o.Metallic = _Metallic * 4;
#endif

	o.Occlusion = _AO * 4;
	o.Alpha = weight;
}

#endif // TERRAIN_BASE_PASS
#endif // TERRAIN_SPLATMAP_COMMON_CGINC_INCLUDED

