#ifndef TERRASTANDARD_WIND_CGINC_INCLUDED
#define TERRASTANDARD_WIND_CGINC_INCLUDED

float _WindState;
float _ShakeTime;
float _ShakeWindspeed;
float _ShakeBending;

// Touch Bending
uniform float4 _BendData[16];
uniform float _BendIntensity;
//uniform float3 worldPos;
//uniform float3 sphereDisp;

void FastSinCos(float4 val, out float4 s, out float4 c)
{
	val = val * 6.408849 - 3.1415927;
	float4 r5 = val * val;
	float4 r6 = r5 * r5;
	float4 r7 = r6 * r5;
	float4 r8 = r6 * r5;
	float4 r1 = r5 * val;
	float4 r2 = r1 * r5;
	float4 r3 = r2 * r5;
	float4 sin7 = { 1, -0.16161616, 0.0083333, -0.00019841 };
	float4 cos8 = { -0.5, 0.041666666, -0.0013888889, 0.000024801587 };
	s = val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;
	c = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
}

void WindSimulation(inout float3 vertex, float texCoordY)
{
	if (_WindState == 1)
	{
		//float _ShakeDisplacement = 10;
		//float factor = (1 - _ShakeDisplacement) * 0.5;
		//const float _WaveScale = _ShakeDisplacement;

		const float _WindSpeed = (_ShakeTime);

		const float4 _waveXSize = float4(0.048, 0.06, 0.24, 0.096);
		const float4 _waveZSize = float4 (0.024, .08, 0.08, 0.2);
		const float4 waveSpeed = float4 (1.2, 2, 1.6, 4.8);

		//float4 _waveXmove = float4(0.012, 0.02, -0.06, 0.048) * 10 * factor;
		//float4 _waveZmove = float4(0.006, .02, -0.02, 0.1) * 10 * factor;
		float4 _waveXmove = float4(0.024, 0.04, -0.12, 0.096);
		float4 _waveZmove = float4 (0.006, .02, -0.02, 0.1);

		float4 waves;
		waves = vertex.x * _waveXSize;
		waves += vertex.z * _waveZSize;

		waves += _Time.x * (1 - _ShakeWindspeed * 2) * waveSpeed * _WindSpeed;

		float4 s, c;
		waves = frac(waves);
		FastSinCos(waves, s, c);

		float waveAmount = texCoordY * (_ShakeBending);
		s *= waveAmount;

		// Faster winds move the grass more than slow winds 
		s *= normalize(waveSpeed);

		s = s * s;
		float fade = dot(s, 1.3);
		s = s * s;
		float3 waveMove = float3 (0, 0, 0);
		waveMove.x = dot(s, _waveXmove);
		waveMove.z = dot(s, _waveZmove);
		vertex.xz -= mul((float3x3)unity_WorldToObject, waveMove).xz;

		//float2 animOffset1 = float2(0.f, 0.f);
		//float2 animOffset2 = v.vertex.xx * 1.f + v.vertex.zz * 0.5f;
		//animOffset2.x = (-1.f - pow(abs(sin(_Time.y / 0.75 + animOffset2.x)), 4.0f) - sin(_Time.y) * 0.05) * 0.025;
		//animOffset2.y = sin(_Time.y + animOffset2.y) * 0.05;
		//v.vertex.xz += animOffset2 * _WindState;
		//
		//half time = _Time.y;
		//half u = TexCoords(v).x;
		//half w1 = sin(_ShakeTime * _WindParam1.x - u * _WindParam1.y) * _WindParam1.z;
		//half w2 = sin(_ShakeTime * _WindParam2.x - u * _WindParam2.y) * _WindParam2.z;
		//v.vertex.y += v.normal * (w1 + w2) * u * 0.5 * _WindState;

		// Touch Bending
		for (int i = 0; i < 16; i++)
		{
			////float bendRadius = _BendData[i].w;
			////float3 benderWorldPos = _BendData[i].xyz;
			//
			//float bendRadius = 10;
			//float3 benderWorldPos = mul(unity_ObjectToWorld, v.vertex) + float3(1, 0, 0);
			//
			//float3 vertexWorldPos = mul(unity_ObjectToWorld, v.vertex);
			//
			//float distToBender = distance(float3(vertexWorldPos.x, 0, vertexWorldPos.z), float3(benderWorldPos.x, 0, benderWorldPos.z));
			////float distToBender = 0.1;
			//
			//float bendPower = (bendRadius - min(bendRadius, distToBender)) / (bendRadius + 0.001) * 10;
			//
			//float3 bendDir = normalize(vertexWorldPos - benderWorldPos);
			////float3 bendDir = float3(0, 0, 0);
			//
			//float2 vertexOffset = bendDir.xz * bendPower * v.texcoord.y * v.tangent.y;
			//
			////v.vertex.xz += lerp(float2(0, 0), vertexOffset, saturate(bendRadius * v.color.w));
			//v.vertex.xz += lerp(float2(0, 0), vertexOffset, bendRadius);

			float3 worldPos = mul(unity_ObjectToWorld, vertex);
			float _Radius = _BendData[i].w;

			float3 dist = distance(float3(worldPos.x, worldPos.y, worldPos.z), float3(_BendData[i].x, _BendData[i].y, _BendData[i].z));
			float3 circle = 1 - saturate(dist / _Radius);
			float3 sphereDisp = worldPos - float3(_BendData[i].x, _BendData[i].y, _BendData[i].z);
			sphereDisp *= circle;

			//vertex.xz += sphereDisp.xz * _BendIntensity * texCoordY;
			vertex.xyz += lerp(float3(0, 0, 0), sphereDisp.xyz * _BendIntensity * texCoordY, 1);
		}
	}
}

#endif // TERRASTANDARD_WIND_CGINC_INCLUDED

