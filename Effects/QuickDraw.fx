sampler2D screen: register(s0);
sampler colorPallete: register(s1);

float4 maskColor;
float4 outColor;

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    float4 inColor = tex2D(screen, coords);

    //if (inColor.b == 1 && inColor.g == 1 && inColor.r == 1)
    //    return maskColor;
    //else
    // After 256, the color wraps over to green
    return tex2D(colorPallete, float2(inColor.r, inColor.g * 256));

}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}