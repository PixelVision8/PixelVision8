sampler main: register(s0);
sampler colorPallete: register(s1);
sampler screen: register(s2);
sampler paletteBank: register(s3);

float imageWidth;

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(screen, float2(coords.x, coords.y));
    int subpart = int(coords.x * imageWidth) % 4;
    
    float bankIndex = tex2D(paletteBank, float2(color[subpart], 0)).r * 255;
    float4 newColor = tex2D(colorPallete, float2(color[subpart], bankIndex));
    
    //if (bankIndex > 0) newColor = float4(0, 0, 0, 0);

    return newColor;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}