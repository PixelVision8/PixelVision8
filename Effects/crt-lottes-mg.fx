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
//   by Timothy Lottes
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

#define hardPix -10.0
#define hardScan -10.0
#define shadowMask 0
#define hardBloomPix -4.0
#define hardBloomScan -3
#define maskDark 0.5
#define maskLight 1.5
#define bloomAmount 0.15
#define warp float2(0.008,0.01)

float brightboost = 1.0;
float2 textureSize;
float2 videoSize;
float2 outputSize;

//Uncomment to reduce instructions with simpler linearization
//(fixes HD3000 Sandy Bridge IGP)
#define SIMPLE_LINEAR_GAMMA 2
#define DO_BLOOM 1

// ------------- //

Texture2D decal;
sampler2D DecalSampler = sampler_state
{
	Texture = <decal>;
};

float4x4 modelViewProj;

//------------------------------------------------------------------------

// sRGB to Linear.
// Assuing using sRGB typed textures this should not be needed.
float3 ToSrgb(float3 c)
{

   return sqrt(c);

}


// Nearest emulated sample given floating point position and texel offset.
// Also zero's off screen.
float3 Fetch(float2 pos, float2 off, float2 texture_size){
  pos=(floor(pos*texture_size.xy+off)+float2(0.5,0.5))/texture_size.xy;
  return brightboost * pow(tex2D(DecalSampler,pos.xy).rgb, 2);
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
float Scan(float2 pos,float off, float2 texture_size){
  float dst=Dist(pos, texture_size).y;
  return Gaus(dst+off,hardScan);}

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

// Shadow mask
float3 Mask(float2 pos){
  float3 mask = maskDark;

  // VGA style shadow mask.
  pos.xy=floor(pos.xy*float2(1.0,0.5));
  pos.x+=pos.y*3.0;
  pos.x=frac(pos.x/6.0);

  if(pos.x<0.333)mask.r=maskLight;
  else if(pos.x<0.666)mask.g=maskLight;
  else mask.b=maskLight;

  return mask;
}

float4 crt_lottes(float2 texture_size, float2 video_size, float2 tex, sampler2D s0)
{

float2 pos=Warp(tex.xy*(texture_size.xy/video_size.xy))*(video_size.xy/texture_size.xy);
float3 outColor = Tri(pos, texture_size);

#ifdef DO_BLOOM
  //Add Bloom
  outColor.rgb+=Bloom(pos, textureSize)*bloomAmount;
#endif


  return float4(ToSrgb(outColor.rgb),1.0);
}

float4 main_fragment(VertexShaderOutput VOUT) : COLOR0
{
	return crt_lottes(textureSize, videoSize, VOUT.texCoord, DecalSampler);
}

technique
{
    pass
	{
	    PixelShader = compile PS_SHADERMODEL main_fragment();
	}
}
