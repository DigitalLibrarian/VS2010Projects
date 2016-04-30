float4x4 WVP;
float4x4 World;
float4x4 View;
float4x4 Proj;
float3 CameraPos;
float3 LightPosition;
float3 LightDiffuseColorIntensity;
float LightDistanceSquared;
float3 DiffuseColor;
float VoxelScale;


struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 RealPosition : POSITION0;
	float4 Position : TEXCOORD0;
	float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input, float4 chunkSpacePos : POSITION1, 
                                float4 instanceColor : COLOR1)
{
    VertexShaderOutput output;
	
	float4 modelPos = input.Position;
	float4 pos = chunkSpacePos + (modelPos * (1));
	pos.w = 1;
	pos = mul(pos, WVP);

	output.Position = pos;
	output.RealPosition = output.Position;
	output.Color = instanceColor;
	
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
  // Phong relfection is ambient + light-diffuse + spec highlights.
  // I = Ia*ka*Oda + fatt*Ip[kd*Od(N.L) + ks(R.V)^n]
  // and http://en.wikipedia.org/wiki/Phong_shading
  // Get light direction for this fragment
  float3 lightDir = normalize(input.Position - LightPosition);
  float3 normal = normalize(cross(ddx(input.Position), ddy(input.Position)));

  // Note: Non-uniform scaling not supported
  float diffuseLighting = saturate(dot(normal, -lightDir)); // per pixel diffuse lighting
 
  // Introduce fall-off of light intensity
  diffuseLighting *= (LightDistanceSquared / dot(LightPosition - input.Position, LightPosition - input.Position));
  // Using Blinn half angle modification for performance over correctness
  float3 h = normalize(normalize(CameraPos - input.Position) - lightDir);
 
  return float4(saturate(
	input.Color +
    (DiffuseColor * LightDiffuseColorIntensity * diffuseLighting * 0.6)
    ), 1);
}

technique Instancing
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
