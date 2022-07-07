#ifndef TERRAFORMER_STOCHASTICSAMPLING_CGINC_INCLUDED
#define TERRAFORMER_STOCHASTICSAMPLING_CGINC_INCLUDED

float sum(float2 v)
{
	return v.x + v.y;
}

float sum(float3 v)
{
	return v.x + v.y + v.z;
}

float sum(float4 v)
{
	return v.x + v.y + v.z + v.w;
}

float4 TilingRemoverVert(float _TilingRemover, float4 uv, float k)
{
	float index = k * _TilingRemover;
	float i = floor(index);
	float2 offa = sin(float2(3.0, 7.0) * i);
	return float4(uv.xy + offa, 0, 0);
}

float4 TilingRemoverVert(float _TilingRemover, float4 uv, float k, sampler2D samp)
{
	float index = k * _TilingRemover;

	float i = floor(index);
	float f = frac(index);

	float2 offa = sin(float2(3.0, 7.0) * i);
	float2 offb = sin(float2(3.0, 7.0) * (i + 1.0)); // can replace with any other hash
	
	//float4 cola = tex2Dgrad(samp, uv + offa, 0, 0);
	//float4 colb = tex2Dgrad(samp, uv + offb, 0, 0);

	float2 uva = uv.xy + offa;
	float2 uvb = uv.xy + offb;

	float4 cola = tex2Dlod(samp, float4(uva, 0, 0));
	float4 colb = tex2Dlod(samp, float4(uvb, 0, 0));
	
	return lerp(cola, colb, smoothstep(0.2, 0.8, f - 0.1 * sum(cola - colb)));
}

float4 TilingRemoverVert2(sampler2D samp, float4 uv, float _TilingRemover)
{
	// sample variation pattern    
    float k = tex2Dlod(samp, 0.005 * uv).x; // cheap (cache friendly) lookup
    
    // compute index    
    float index = k * _TilingRemover;
    float i = floor( index );
    float f = frac( index );

    // offsets for the different virtual patterns    
    float2 offa = sin(float2(3.0, 7.0) * (i + 0.0)); // can replace with any other hash    
    float2 offb = sin(float2(3.0, 7.0) * (i + 1.0)); // can replace with any other hash    
    
    // sample the two closest virtual patterns    
    float4 cola = tex2Dlod(samp, uv + float4(offa, 0, 0)).xxxx;
    float4 colb = tex2Dlod(samp, uv + float4(offb, 0, 0)).xxxx;

    // interpolate between the two virtual patterns    
    return lerp( cola, colb, smoothstep(0.2, 0.8 , f - 0.1 * sum(cola - colb)));
}

float4 TilingRemoverVert(float _TilingRemover, sampler2D samp, float4 uv)
{
	float k = tex2Dlod(samp, 0.005 * uv).r;

	float index = k * _TilingRemover;
	float i = floor(index);
	//float f = frac(index);
	float2 offa = sin(float2(3.0, 7.0) * (i + 0.0)); // can replace with any other hash
	//float2 offb = sin(float2(3.0, 7.0) * (i + 1.0)); // can replace with any other hash
	//float4 cola = tex2Dgrad(samp, uv + offa, 0, 0);
	//float4 colb = tex2Dgrad(samp, uv + offb, 0, 0);
	float2 uva = uv.xy + offa;
	//float2 uvb = uv.xy + offb;

	return float4(uva, 0, 0);

	//float4 cola = tex2Dlod(samp, float4(uva, 0, 0));
	////float4 colb = tex2Dlod(samp, float4(uvb, 0, 0));
	//
	////return lerp(cola, colb, smoothstep(0.2, 0.8, f - 0.1 * sum(cola - colb)));
	//return cola;
}

float _FractalDivergance = 1.0;
    // One way to avoid tex2D tile repetition one using one small tex2D to cover a huge area.
    // Based on Voronoise (https://www.shadertoy.com/view/Xd23Dh), a random offset is applied to
    // the tex2D UVs per Voronoi cell. Distance to the cell is used to smooth the transitions
    // between cells.
    // More info here: http://www.iquilezles.org/www/articles/tex2Drepetition/tex2Drepetition.htm
    fixed4 hash4( fixed2 p ) { return frac(sin(fixed4( 1.0+dot(p,fixed2(37.0,17.0)),
                                                  2.0+dot(p,fixed2(11.0,47.0)),
                                                  3.0+dot(p,fixed2(41.0,29.0)),
                                                  4.0+dot(p,fixed2(23.0,31.0)))) * _FractalDivergance); }

fixed4 TilingRemoverVert3( sampler2D samp, in fixed4 uv, fixed v )
{
    fixed4 p = floor( uv );
    fixed4 f = frac( uv );
 
    // derivatives (for correct mipmapping)
    fixed4 _ddx = ddx( uv );
    fixed4 _ddy = ddy( uv );
 
    fixed4 va = fixed4(0.0,0.0,0.0,0.0);
    fixed w1 = 0.0;
    fixed w2 = 0.0;
 
    [unroll(3)]
    for (int j = -1; j <= 1; j++)
    {
        [unroll(3)]
        for (int i = -1; i <= 1; i++)
        {
            fixed2 g = fixed2(fixed(i), fixed(j));
            fixed4 o = hash4(p + g);
            fixed2 r = g - f + o.xy;
            fixed d = dot(r, r);
            fixed w = exp(-5.0*d);
            //fixed4 c = tex2Dlod(samp, uv + v * o.zw, _ddx, _ddy).xyzw;
			fixed4 c = tex2Dlod(samp, float4(uv.xy + v * o.zw, 0, 0));
            va += w * c;
            w1 += w;
            w2 += w * w;
        }
    }

    // normal averaging --> lowers contrasts
    //return va/w1;
    // contrast preserving average
    fixed mean = 0;// tex2DGrad( samp, uv, ddx*16.0, ddy*16.0 ).x;
    fixed4 res = mean + (va-w1*mean)/sqrt(w2);
    return lerp( va/w1, res, v );
}


float2 TilingRemoverSurf(float _TilingRemover, float2 uv,  float k)
{
	float index = k * _TilingRemover;
	float i = floor(index);
	float2 offa = sin(float2(3, 7) * i);
	return uv + offa;
}

float2 TilingRemoverSurf(float _TilingRemover, float2 uv,  float k, sampler2D samp)
{
	float index = k * _TilingRemover;
	
	float i = floor(index);
	float f = frac(index);

	float2 offa = sin(float2(3.0, 7.0) * i);
	float2 offb = sin(float2(3.0, 7.0) * (i + 1.0)); // can replace with any other hash
	
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);

	float2 uva = uv.xy + offa;
	float2 uvb = uv.xy + offb;

	float4 cola = tex2Dgrad(samp, uva, dx, dy);
	float4 colb = tex2Dgrad(samp, uvb, dx, dy);

	return lerp(cola, colb, smoothstep(0.2, 0.8, f - 0.1 * sum(cola - colb)));
}

float2 TilingRemoverSurf2(sampler2D samp, float2 uv, float _TilingRemover)
{
    // sample variation pattern    
    float k = tex2D(samp, 0.005 * uv).x; // cheap (cache friendly) lookup
    
    // compute index    
    float index = k * _TilingRemover;
    float i = floor( index );
    float f = frac( index );

    // offsets for the different virtual patterns    
    float2 offa = sin(float2(3.0, 7.0) * (i + 0.0)); // can replace with any other hash    
    float2 offb = sin(float2(3.0, 7.0) * (i + 1.0)); // can replace with any other hash    

    // compute derivatives for mip-mapping    
    float2 dx = ddx(uv);
	float2 dy = ddy(uv);
    
    // sample the two closest virtual patterns    
    float3 cola = tex2Dgrad( samp, uv + offa, dx, dy ).xxx;
    float3 colb = tex2Dgrad( samp, uv + offb, dx, dy ).xxx;

    // interpolate between the two virtual patterns    
    return lerp( cola, colb, smoothstep(0.2, 0.8 , f - 0.1 * sum(cola - colb)));
}

float2 TilingRemoverSurf(float _TilingRemover, sampler2D samp, float2 uv)
{
	float k = tex2D(samp, 0.005 * uv).r;
	float index = k * _TilingRemover;
	float i = floor(index);
	//float f = frac(index);
	float2 offa = sin(float2(3.0, 7.0) * (i + 0.0)); // can replace with any other hash
	//float2 offb = sin(float2(3.0, 7.0) * (i + 1.0)); // can replace with any other hash
	//float2 dx = ddx(uv), dy = ddy(uv);
	//float4 cola = tex2Dgrad(samp, uv + offa, dx, dy);
	//float4 colb = tex2Dgrad(samp, uv + offb, dx, dy);
	float2 uva = uv + offa;

	return uva;

	////float2 uvb = uv + offb;
	//float4 cola = tex2D(samp, uva);
	////float4 colb = tex2D(samp, uvb);
	//uvNormal = uva;
	////uvNormal = lerp(uva, uvb, smoothstep(0.2, 0.8, f - 0.1 * sum(uva - uvb)));
	//
	////return lerp(cola, colb, smoothstep(0.2, 0.8, f - 0.1 * sum(cola - colb)));
	//return cola;
}

#endif // TERRAFORMER_STOCHASTICSAMPLING_CGINC_INCLUDED

