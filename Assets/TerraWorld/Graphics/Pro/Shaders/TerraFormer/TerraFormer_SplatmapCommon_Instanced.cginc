#ifndef TERRAFORMER_SPLATMAPCOMMON_INSTANCED_CGINC_INCLUDED
#define TERRAFORMER_SPLATMAPCOMMON_INSTANCED_CGINC_INCLUDED

struct appdata
{
	float4 vertex : POSITION;
	float4 color : COLOR;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float2 texcoord : TEXCOORD0;
	float2 texcoord1 : TEXCOORD1;
	float2 texcoord2 : TEXCOORD2;
	//float3 worldNormal : TEXCOORD3;
};

struct Input
{
	float4 color : COLOR;
	float3 worldNormal;
	float3 worldPos;
	float4 tc;

#ifndef TERRAIN_BASE_PASS
	UNITY_FOG_COORDS(0) // needed because finalcolor oppresses fog code generation.
#endif

	INTERNAL_DATA
};

half _Metallic0;
half _Metallic1;
half _Metallic2;
half _Metallic3;
half _Smoothness0;
half _Smoothness1;
half _Smoothness2;
half _Smoothness3;

sampler2D _Control;
float4 _Control_TexelSize;

UNITY_DECLARE_TEX2D(_Splat0);
UNITY_DECLARE_TEX2D(_Splat1);
UNITY_DECLARE_TEX2D(_Splat2);
UNITY_DECLARE_TEX2D(_Splat3);
float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;

UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap0);
UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap1);
UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap2);
UNITY_DECLARE_TEX2D_NOSAMPLER(_Maskmap3);

float _TilingRemover1, _TilingRemover2, _TilingRemover3, _TilingRemover4;
float _NoiseTiling1, _NoiseTiling2, _NoiseTiling3, _NoiseTiling4;
fixed4 _LayerColor1, _LayerColor2, _LayerColor3, _LayerColor4;
fixed4 _LightingColor;
half _LayerAO1, _LayerAO2, _LayerAO3, _LayerAO4;

//UNITY_DECLARE_TEX2D_NOSAMPLER(_Noise);
float _NoiseTiling;
//float _NoiseIntensity;

UNITY_DECLARE_TEX2D_NOSAMPLER(_WaterMask);
float2 splatUV;

#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X)
    sampler2D _TerrainHeightmapTexture;
    sampler2D _TerrainNormalmapTexture;
    float4    _TerrainHeightmapRecipSize;   // float4(1.0f/width, 1.0f/height, 1.0f/(width-1), 1.0f/(height-1))
    float4    _TerrainHeightmapScale;       // float4(hmScale.x, hmScale.y / (float)(kMaxHeight), hmScale.z, 0.0f)
#endif

UNITY_INSTANCING_BUFFER_START(Terrain)
    UNITY_DEFINE_INSTANCED_PROP(float4, _TerrainPatchInstanceData) // float4(xBase, yBase, skipScale, ~)
UNITY_INSTANCING_BUFFER_END(Terrain)

//#ifdef _NORMALMAP
    sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
    float _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;
//#endif

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

#if defined(TERRAIN_BASE_PASS) && defined(UNITY_PASS_META)
    // When we render albedo for GI baking, we actually need to take the ST
    float4 _MainTex_ST;
#endif

void SplatmapVert(inout appdata_full v, out Input data)
{
    UNITY_INITIALIZE_OUTPUT(Input, data);

#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X)

    float2 patchVertex = v.vertex.xy;
    float4 instanceData = UNITY_ACCESS_INSTANCED_PROP(Terrain, _TerrainPatchInstanceData);

    float4 uvscale = instanceData.z * _TerrainHeightmapRecipSize;
    float4 uvoffset = instanceData.xyxy * uvscale;
    uvoffset.xy += 0.5f * _TerrainHeightmapRecipSize.xy;
    float2 sampleCoords = (patchVertex.xy * uvscale.xy + uvoffset.xy);

    float hm = UnpackHeightmap(tex2Dlod(_TerrainHeightmapTexture, float4(sampleCoords, 0, 0)));
    v.vertex.xz = (patchVertex.xy + instanceData.xy) * _TerrainHeightmapScale.xz * instanceData.z;  //(x + xBase) * hmScale.x * skipScale;
    v.vertex.y = hm * _TerrainHeightmapScale.y;
    v.vertex.w = 1.0f;

    v.texcoord.xy = (patchVertex.xy * uvscale.zw + uvoffset.zw);
    v.texcoord3 = v.texcoord2 = v.texcoord1 = v.texcoord;

    #ifdef TERRAIN_INSTANCED_PERPIXEL_NORMAL
        v.normal = float3(0, 1, 0); // TODO: reconstruct the tangent space in the pixel shader. Seems to be hard with surface shader especially when other attributes are packed together with tSpace.
        data.tc.zw = sampleCoords;
    #else
        float3 nor = tex2Dlod(_TerrainNormalmapTexture, float4(sampleCoords, 0, 0)).xyz;
        v.normal = 2.0f * nor - 1.0f;
    #endif
#endif

    // Flat Shading
#ifdef _FLATSHADING
	float3 worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz;
    FlatShadingVert(v.vertex.xyz, v.normal, worldNormal, v.texcoord.xy);
#endif

    v.tangent.xyz = cross(v.normal, float3(0,0,1));
    v.tangent.w = -1;

    data.tc.xy = v.texcoord;
#ifdef TERRAIN_BASE_PASS
    #ifdef UNITY_PASS_META
        data.tc.xy = v.texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
    #endif
#else
    float4 pos = UnityObjectToClipPos(v.vertex);
    UNITY_TRANSFER_FOG(data, pos);
#endif
}

#ifndef TERRAIN_BASE_PASS

void SplatmapMix(Input IN, out half4 splat_control, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal, out half _Smoothness, out half _Metallic, out half _AO)
{
    #if UNITY_VERSION >= 201930
	    #ifdef _ALPHATEST_ON
            ClipHoles(IN.tc.xy);
        #endif
    #endif

    // adjust splatUVs so the edges of the terrain tile lie on pixel centers
    splatUV = (IN.tc.xy * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
    splat_control = tex2D(_Control, splatUV);
    weight = dot(splat_control, half4(1,1,1,1));

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

	//float noise1 = UNITY_SAMPLE_TEX2D_SAMPLER(_Noise, _Splat0, splatUV * _NoiseTiling1).r;
	//float noise2 = UNITY_SAMPLE_TEX2D_SAMPLER(_Noise, _Splat0, splatUV * _NoiseTiling2).r;
	//float noise3 = UNITY_SAMPLE_TEX2D_SAMPLER(_Noise, _Splat0, splatUV * _NoiseTiling3).r;
	//float noise4 = UNITY_SAMPLE_TEX2D_SAMPLER(_Noise, _Splat0, splatUV * _NoiseTiling4).r;
	//
	//if(_TilingRemover1 > 0.0f) uv0 = TilingRemoverSurf(_TilingRemover1, uv0, noise1);
	//if(_TilingRemover2 > 0.0f) uv1 = TilingRemoverSurf(_TilingRemover2, uv1, noise2);
	//if(_TilingRemover3 > 0.0f) uv2 = TilingRemoverSurf(_TilingRemover3, uv2, noise3);
	//if(_TilingRemover4 > 0.0f) uv3 = TilingRemoverSurf(_TilingRemover4, uv3, noise4);

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

	fixed4 _Diffuse1 = splat_control.r * t0; // * half4(1.0, 1.0, 1.0, maskMap1.a);
	fixed4 _Diffuse2 = splat_control.g * t1; // * half4(1.0, 1.0, 1.0, maskMap2.a);
	fixed4 _Diffuse3 = splat_control.b * t2; // * half4(1.0, 1.0, 1.0, maskMap3.a);
	fixed4 _Diffuse4 = splat_control.a * t3; // * half4(1.0, 1.0, 1.0, maskMap4.a);

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

#if defined(INSTANCING_ON) && defined(SHADER_TARGET_SURFACE_ANALYSIS) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
    mixedNormal = float3(0, 0, 1); // make sure that surface shader compiler realizes we write to normal, as UNITY_INSTANCING_ENABLED is not defined for SHADER_TARGET_SURFACE_ANALYSIS.
#endif

#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
    float3 geomNormal = normalize(tex2D(_TerrainNormalmapTexture, IN.tc.zw).xyz * 2 - 1);
//#ifdef _NORMALMAP
    float3 geomTangent = normalize(cross(geomNormal, float3(0, 0, 1)));
    float3 geomBitangent = normalize(cross(geomTangent, geomNormal));
    mixedNormal = mixedNormal.x * geomTangent
                    + mixedNormal.y * geomBitangent
                    + mixedNormal.z * geomNormal;
//#else
//    mixedNormal = geomNormal;
//#endif
    mixedNormal = mixedNormal.xzy;
#endif
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
	half _Metallic;
	half _Smoothness;
	half _AO;
	SplatmapMix(IN, splat_control, weight, mixedDiffuse, o.Normal, _Smoothness, _Metallic, _AO);

	fixed3 finalCol;

#ifdef _FLATSHADING
    FlatShadingFrag(o.Normal, IN.worldPos.xyz, WorldNormalVector(IN, half3(1, 0, 0)), WorldNormalVector(IN, half3(0, 1, 0)), WorldNormalVector(IN, half3(0, 0, 1)));
#endif

#ifdef _COLORMAPBLENDING
	finalCol = ColormapBlending(splatUV, _WorldSpaceCameraPos, IN.worldPos, mixedDiffuse, _LightingColor);
#else
	finalCol = mixedDiffuse * _LightingColor;
#endif

half3 _worldNormal = WorldNormalVector(IN, o.Normal);
float waterMask = 0;

#if defined(_PROCEDURALPUDDLES) || defined(_PROCEDURALSNOW)
	waterMask = 1 - UNITY_SAMPLE_TEX2D_SAMPLER(_WaterMask, _Splat0, splatUV).a;
#endif

#ifdef _PROCEDURALPUDDLES
	half3 slope = 1 - dot(normalize(o.Normal), float3(0, 0, 1));

	//float noise = UNITY_SAMPLE_TEX2D_SAMPLER(_Noise, _Splat0, splatUV * _NoiseTiling).r;
	float noise = 1.0 - SimplexNoise(_NoiseTiling * normalize(IN.worldPos));

	half puddle = slope * noise;
	Puddles(puddle, o.Albedo, o.Normal, o.Smoothness, o.Metallic, _Smoothness, _Metallic, finalCol.rgb, waterMask, 1.5);

	#ifdef _PROCEDURALSNOW
		if (puddle > _Slope || puddle < _SlopeMin)
		{
			SnowWithFlow(o.Albedo, o.Smoothness, o.Normal, _worldNormal, splatUV, IN.color.y, IN.worldPos.y, finalCol.rgb, _Smoothness, waterMask, splat_control);
			o.Metallic = _Metallic * 4;
		}
	#endif
#else
	#ifdef _PROCEDURALSNOW
		SnowWithFlow(o.Albedo, o.Smoothness, o.Normal, _worldNormal, splatUV, IN.color.y, IN.worldPos, finalCol.rgb, _Smoothness, waterMask, splat_control);
	#else
		o.Albedo = finalCol.rgb;
		o.Smoothness = _Smoothness * 4;
	#endif

	o.Metallic = _Metallic * 4;
#endif

	o.Occlusion = _AO * 4;
	o.Alpha = weight;
}

#endif // TERRAIN_BASE_PASS
#endif // TERRAFORMER_SPLATMAPCOMMON_INSTANCED_CGINC_INCLUDED

