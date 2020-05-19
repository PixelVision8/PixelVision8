sampler2D screen: register(s0);
sampler colorPallete: register(s1);

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(screen, coords);
    return tex2D(colorPallete, float2(color.r, 0.5f));
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}