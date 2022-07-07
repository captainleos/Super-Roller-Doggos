Shader "TerraUnity/Horizon Fog"
{
    Properties
    {
        [HDR] _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _ConeData ("Radius (X), Height (Y), Start Offset (Z), Half Angle Cos (W)", Vector) = (1,4,0,0.7071067)
        _TextureData ("Texture Speed (X,Y,Z), Texture Scale (W)", Vector) = (10,-0.1,0,1)
        _Visibility ("Visibility", Float) = 1.0
        _BIsForwardLighting ("Is Forward Lighting", Float) = 0
    }
    
    SubShader
    {
        Tags { "Queue"="Overlay-1" "IgnoreProjector"="True" "RenderType"="Transparent" }
		//Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

        //Blend Off                           // No Blend
        Blend SrcAlpha OneMinusSrcAlpha     // Alpha blending
        //Blend One One                       // Additive
        //Blend One OneMinusDstColor          // Soft Additive
        //Blend DstColor Zero                 // Multiplicative
        //Blend DstColor SrcColor             // 2x Multiplicative

        Cull Front
        Lighting Off
        ZWrite Off
        ZTest Always
        //Fog { Color (0,0,0,0) }
        LOD 200
	
        Pass
        {	
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma target 3.0
            //#pragma only_renderers d3d9
            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float4 _Color;

            #define SAMPLE_METHOD
            #define CAPPED_CONE

            //==============================
            // CONE INTERSECTION
            //==============================
            inline float IntersectCone(float3 rayOrigin, float3 rayDirection, float coneRadius, float coneHeight, float topCap, float4x4 transformMatrix, inout float t1, inout float t2)
            {
                // Quadratic:
                // ax^2 + bx + c = 0
                // t1 = (-b + sqrt(b^2 - 4ac)) / 2a
                // t2 = (-b - sqrt(b^2 - 4ac)) / 2a

                float k = coneRadius / coneHeight;
                float ksquared = k*k;
                float A = (rayDirection.x*rayDirection.x)+(rayDirection.z*rayDirection.z)-((rayDirection.y*rayDirection.y)*ksquared);
                float B = 2 * ((rayOrigin.x*rayDirection.x) + (rayOrigin.z*rayDirection.z) - ((rayOrigin.y*rayDirection.y)*ksquared));
                float C = (rayOrigin.x*rayOrigin.x) + (rayOrigin.z*rayOrigin.z) - ((rayOrigin.y*rayOrigin.y)*ksquared);

                // output intersection data
                float discriminant = (B*B)-(4*A*C);

                if(discriminant <= 0) return 0.0f;
                
                t1 = (-B + sqrt(discriminant)) / (2*A);
                t2 = (-B - sqrt(discriminant)) / (2*A);

                float returnVal = 1.0f;

                // intersect with base
                float baseW = -coneHeight;
                float tBase = -((rayOrigin.y - baseW) / rayDirection.y);
#if defined(CAPPED_CONE)
                float tBase2 = -((rayOrigin.y + topCap) / rayDirection.y);
#endif

                // get local y of both intersection points
                float y1 = rayOrigin.y + (rayDirection.y * t1);
                float y2 = rayOrigin.y + (rayDirection.y * t2);

#if defined(CAPPED_CONE)

                returnVal -= step(rayOrigin.y, -topCap) * step(y1, -topCap) * step(0, y1);

                //if (rayOrigin.y > -topCap && y1 > -topCap && y1 < 0)
                //{
                //    return 0f;
                //}

                returnVal -= step(-topCap, rayOrigin.y) * step(y2, -topCap) * step(0, y2);

                //else if (rayOrigin.y < -topCap && y2 > -topCap && y2 < 0)
                //{
                //    return 0f;
                //}
#endif
                
                //t1 = t1 * step(y1, 0) + tBase * step(0, y1);
                //t1 = t1 * step(-coneHeight, y1) + tBase * step(y1, -coneHeight);
                //t2 = t2 * step(y2, 0) + tBase * step(0, y2);
                //t2 = t2 * step(-coneHeight, y2) + tBase * step(y2, -coneHeight);
                
                t1 = y1 > 0           ? tBase : t1;
                t1 = y1 < -coneHeight ? tBase : t1;
                t2 = y2 > 0           ? tBase : t2;
                t2 = y2 < -coneHeight ? tBase : t2;

#if defined(CAPPED_CONE)
                if (y2 > -topCap && y2 < 0) t2 = tBase2;
                if (y1 > -topCap && y1 < 0) t1 = tBase2;
#endif

                t1 = max(t1, 0);
                t2 = max(t2, 0);

                return saturate(returnVal);
            }

            struct VSOutput
            {
                float4 vPos         : SV_POSITION;
                float3 vWorldPos    : TEXCOORD0;
                float4 vScreenPos   : TEXCOORD1;
                float3 vLocalPos    : TEXCOORD2;
                float3 vViewPos     : TEXCOORD3;
                float3 vLocalCamPos : TEXCOORD4; // constant not available in pixel shader so have to send it through here
            };

            float4 _ConeData;
            float4 _TextureData;
            float _Visibility;
            float _BIsForwardLighting;

            VSOutput vert (appdata_full IN)
            {
                VSOutput OUT;

                OUT.vLocalPos = IN.vertex.xyz;
                OUT.vWorldPos.xyz = mul((float4x4)unity_ObjectToWorld, float4(IN.vertex.xyz, 1.0f)).xyz;
                OUT.vViewPos = mul((float4x4)UNITY_MATRIX_MV, float4(IN.vertex.xyz, 1.0f)).xyz;
                OUT.vLocalCamPos = mul((float4x4)unity_WorldToObject, (float4(_WorldSpaceCameraPos, 1.0f))).xyz;
                OUT.vPos = UnityObjectToClipPos(IN.vertex);
                OUT.vScreenPos = ComputeScreenPos(OUT.vPos);
      
                return OUT;
            }

            float4 frag (VSOutput IN) : COLOR
            {
                float3 eyeDirectionWorld = normalize(IN.vWorldPos - _WorldSpaceCameraPos);
                float3 eyeDirectionLocal = normalize(IN.vLocalPos - IN.vLocalCamPos);
                //float3 eyeVecLocal = -eyeDirectionLocal;

                float3 rayOrigin = IN.vLocalCamPos;
                float3 rayDirection = eyeDirectionLocal;
                float t1, t2;

                float bValidIntersection = IntersectCone(rayOrigin, rayDirection, _ConeData.x, _ConeData.y, _ConeData.z, unity_WorldToObject, t1, t2);

                // Get distance from eye to depth buffer fragment
                float2 screenUV = IN.vScreenPos.xy / IN.vScreenPos.w;
      
                float4 depthTexture = tex2D(_CameraDepthTexture, screenUV);
                float uniformDistance = DECODE_EYEDEPTH(depthTexture.r);
                float3 viewEyeDirection = normalize(IN.vViewPos);
                float scaleFactor = (uniformDistance / viewEyeDirection.z);
                float distanceToDepthFragment = length(viewEyeDirection * scaleFactor);

                // Calculate new t1/t2 using depth buffer
                float tFar = t1;//max(t1, t2);
                float tNear = t2;//min(t1, t2);
      
                // Calculate amount of volume being blocked by depth buffer
                tFar = min(tFar, distanceToDepthFragment);
                tNear = min(tNear, distanceToDepthFragment);

#if defined(SAMPLE_METHOD)
                float intensity = 0;
                int sampleCount = 20;
                float invSampleCount = 1.0f/(float)sampleCount;
                float sampleRange = tFar - tNear;
                float sampleStep = sampleRange * invSampleCount;
                float totalUniformIntensity = pow(saturate(sampleRange / _Visibility), 1);
                float invConeHeight = 1.0f / _ConeData.y;
                float invConeHeightSquared = 1.0f / (_ConeData.y * _ConeData.y);

                for(int s=0; s<sampleCount; s++)
                {
                    float tSample = tNear + (sampleStep * (float)s);
                    float3 samplePosWorld = _WorldSpaceCameraPos + (eyeDirectionWorld * tSample);
                    float3 samplePosLocal = IN.vLocalCamPos + (eyeDirectionLocal * tSample);

                    // Calculate falloff
                    float distanceToTopSquared = dot(samplePosLocal, samplePosLocal);
                    float heightAttenuation = saturate(distanceToTopSquared * invConeHeightSquared);
          
                    // Calculate angle attenuation
                    //float3 lightDirectionLocal = normalize(samplePosLocal);
                    //float cosAngle = -lightDirectionLocal.y; // dot(lightDirectionLocal, float3(0,-1,0));
                    //float angleAttenuation = smoothstep(_ConeData.w, 1.0f, cosAngle);
                    //angleAttenuation = 1.0f - pow(1.0f - angleAttenuation, 3);

                    // Texture
                    float2 xyOffset = float2(_TextureData.x, _TextureData.y) * _Time.y;
                    float2 zyOffset = float2(_TextureData.z, _TextureData.z) * _Time.y;
                    float scale = _TextureData.w;
                    float noiseValue = tex2D(_MainTex, samplePosWorld.xy*scale + xyOffset).r + tex2D(_MainTex, samplePosWorld.zy*scale + zyOffset).r;

                    // Calculate distance attenuation
                    //float distanceAttenuation = 1.0f - saturate(tSample / 50.0f);

                    // Calculate sample intensity
                    //intensity += (invSampleCount * noiseValue);// * (1f - heightAttenuation * _Visibility.y));// * angleAttenuation);// * distanceAttenuation;
					intensity += (invSampleCount * noiseValue) * (1 - heightAttenuation * 0.5) * (heightAttenuation * 0.5);
					//intensity += (invSampleCount * noiseValue) * (heightAttenuation * 0.5);
                }

                intensity *= totalUniformIntensity;// * cap;//clamp(-IN.vLocalPos.y*10-(_StartFade*10-1),minClamp,1);// * clamp(pow(falloffRange,_FalloffPower)*_Falloff,1,1000000);
#else // (!)defined(SAMPLE_METHOD)
                float volumeAmount = tFar - tNear;

                float intensity = saturate(volumeAmount / _Visibility);
#endif // defined(SAMPLE_METHOD)

                //intensity *= step(100, abs(tFar - tNear));

                intensity *= bValidIntersection;

                return float4(_Color.rgb, intensity * _Color.a * 0.5);
            }
            ENDCG
        }
	} 
}

