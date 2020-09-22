#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
	#define HLSL_4
#endif

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 texCoord : TEXCOORD0;
};

//
// PUBLIC DOMAIN CRT STYLED SCAN-LINE SHADER
//
//   by Timothy Lottes | Modified by Jesse Freeman for Pixel Vision 8
//
// This is more along the style of a really good CGA arcade monitor.
// With RGB inputs instead of NTSC.
// The shadow mask example has the mask rotated 90 degrees for less chromatic aberration.
//
// Left it unoptimized to show the theory behind the algorithm.
//
// It is an example what I personally would want as a display option for pixel art games.
// Please take and use, change, or whatever.
//


#define hardScan -10.0
#define hardBloomPix -4.0
#define hardBloomScan -3
#define bloomAmount 0.15

float2 warp = float2(0.008, 0.01);
float hardPix = - 10.0;
sampler2D screen: register(s0);
sampler colorPallete: register(s1);

float brightboost = 1.0;
float2 textureSize;
float2 videoSize;
float2 outputSize;
float crtOn = 0;

// Nearest emulated sample given floating point position and texel offset.
// Also zero's off screen.
float3 Fetch(float2 pos, float2 off, float2 texture_size){

  // Smooths out the distortion
  pos=(floor(pos*texture_size.xy+off)+float2(0.5,0.5))/texture_size.xy;
  
  // find the color on the scren texture
  float4 inColor = tex2D(screen, pos);

  float4 outColor = tex2D(colorPallete, float2(inColor.r, inColor.g * 256));

  // Convert the color to a palette color and brighten it
  return outColor.rgb;
}

// Distance in emulated pixels to nearest texel.
float2 Dist(float2 pos, float2 texture_size){pos=pos*texture_size.xy;return -(frac(pos)-float2(0.5, 0.5));}

// 1D Gaussian.
float Gaus(float pos,float scale){return exp2(scale*pos*pos);}

// 3-tap Gaussian filter along horz line.
float3 Horz3(float2 pos, float off, float2 texture_size){
  float3 b=Fetch(pos,float2(-1.0,off),texture_size);
  float3 c=Fetch(pos,float2( 0.0,off),texture_size);
  float3 d=Fetch(pos,float2( 1.0,off),texture_size);
  float dst=Dist(pos, texture_size).x;
  // Convert distance to weight.
  float scale=hardPix;
  float wb=Gaus(dst-1.0,scale);
  float wc=Gaus(dst+0.0,scale);
  float wd=Gaus(dst+1.0,scale);
  // Return filtered sample.
  return (b*wb+c*wc+d*wd)/(wb+wc+wd);}

// 5-tap Gaussian filter along horz line.
float3 Horz5(float2 pos, float off, float2 texture_size){
  float3 a=Fetch(pos,float2(-2.0,off),texture_size);
  float3 b=Fetch(pos,float2(-1.0,off),texture_size);
  float3 c=Fetch(pos,float2( 0.0,off),texture_size);
  float3 d=Fetch(pos,float2( 1.0,off),texture_size);
  float3 e=Fetch(pos,float2( 2.0,off),texture_size);
  float dst=Dist(pos, texture_size).x;
  // Convert distance to weight.
  float scale=hardPix;
  float wa=Gaus(dst-2.0,scale);
  float wb=Gaus(dst-1.0,scale);
  float wc=Gaus(dst+0.0,scale);
  float wd=Gaus(dst+1.0,scale);
  float we=Gaus(dst+2.0,scale);
  // Return filtered sample.
  return (a*wa+b*wb+c*wc+d*wd+e*we)/(wa+wb+wc+wd+we);}

// 7-tap Gaussian filter along horz line.
float3 Horz7(float2 pos, float off, float2 texture_size){
  float3 a=Fetch(pos,float2(-3.0,off),texture_size);
  float3 b=Fetch(pos,float2(-2.0,off),texture_size);
  float3 c=Fetch(pos,float2(-1.0,off),texture_size);
  float3 d=Fetch(pos,float2( 0.0,off),texture_size);
  float3 e=Fetch(pos,float2( 1.0,off),texture_size);
  float3 f=Fetch(pos,float2( 2.0,off),texture_size);
  float3 g=Fetch(pos,float2( 3.0,off),texture_size);
  float dst=Dist(pos, texture_size).x;
  // Convert distance to weight.
  float scale=hardBloomPix;
  float wa=Gaus(dst-3.0,scale);
  float wb=Gaus(dst-2.0,scale);
  float wc=Gaus(dst-1.0,scale);
  float wd=Gaus(dst+0.0,scale);
  float we=Gaus(dst+1.0,scale);
  float wf=Gaus(dst+2.0,scale);
  float wg=Gaus(dst+3.0,scale);
  // Return filtered sample.
  return (a*wa+b*wb+c*wc+d*wd+e*we+f*wf+g*wg)/(wa+wb+wc+wd+we+wf+wg);}

// Return scanline weight.
float Scan(float2 pos, float off, float2 texture_size) {
    float dst = Dist(pos, texture_size).y;
    return Gaus(dst + off, hardScan);
}

  // Return scanline weight for bloom.
float BloomScan(float2 pos,float off, float2 texture_size){
  float dst=Dist(pos, texture_size).y;
  return Gaus(dst+off,hardBloomScan);}

// Allow nearest three lines to effect pixel.
float3 Tri(float2 pos, float2 texture_size){
  float3 a=Horz3(pos,-1.0, texture_size);
  float3 b=Horz5(pos, 0.0, texture_size);
  float3 c=Horz3(pos, 1.0, texture_size);
  float wa=Scan(pos,-1.0, texture_size);
  float wb=Scan(pos, 0.0, texture_size);
  float wc=Scan(pos, 1.0, texture_size);
  return a*wa+b*wb+c*wc;}

// Small bloom.
float3 Bloom(float2 pos, float2 texture_size){
  float3 a=Horz5(pos,-2.0, texture_size);
  float3 b=Horz7(pos,-1.0, texture_size);
  float3 c=Horz7(pos, 0.0, texture_size);
  float3 d=Horz7(pos, 1.0, texture_size);
  float3 e=Horz5(pos, 2.0, texture_size);
  float wa=BloomScan(pos,-2.0, texture_size);
  float wb=BloomScan(pos,-1.0, texture_size);
  float wc=BloomScan(pos, 0.0, texture_size);
  float wd=BloomScan(pos, 1.0, texture_size);
  float we=BloomScan(pos, 2.0, texture_size);
  return a*wa+b*wb+c*wc+d*wd+e*we;}

// Distortion of scanlines, and end of screen alpha.
float2 Warp(float2 pos){
	pos=pos*2.0-1.0;
	pos*=float2(1.0+(pos.y*pos.y)*warp.x,1.0+(pos.x*pos.x)*warp.y);
	return pos*0.5+0.5;}

float4 crt_lottes(float2 texture_size, float2 video_size, float2 tex)
{
    
    if (crtOn == 0)
        return float4(Fetch(tex, 0, texture_size),1);

    float2 pos=Warp(tex.xy*(texture_size.xy/video_size.xy))*(video_size.xy/texture_size.xy);

    float3 outColor =  Tri(pos, texture_size) ;
    
    //Add Bloom
    outColor.rgb+=Bloom(pos, textureSize)*bloomAmount;
    
    return float4(outColor.rgb,1.0) * brightboost;
}

float4 main_fragment(VertexShaderOutput VOUT) : COLOR0
{
	return crt_lottes(textureSize, videoSize, VOUT.texCoord);
}

technique
{
    pass
	{
	    PixelShader = compile PS_SHADERMODEL main_fragment();
	}
}
