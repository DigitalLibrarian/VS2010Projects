float4x4 WVP;

// TODO: add effect parameters here.

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;


    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;

    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float4 chunkSpacePos : POSITION1, 
                                float4 instanceColor : COLOR1)
{
    VertexShaderOutput output;

	float4 pos = chunkSpacePos + input.Position;
    output.Position = mul(pos, WVP);

	output.Color = instanceColor;

    // TODO: add your vertex shader code here.

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.

    return input.Color;
}

technique Instancing
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
