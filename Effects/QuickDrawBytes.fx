sampler main: register(s0);
sampler colorPallete: register(s1);
sampler screen: register(s2);
float imageWidth;

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(screen, float2(coords.x, coords.y));
    int subpart = int(coords.x * imageWidth) % 4;

    return tex2D(colorPallete, float2(color[subpart], 0.5f));
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}