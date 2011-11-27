float4x4 World;
float4x4 View;
float4x4 Projection;

texture inputTex;
int numPlayers;
float2 cyclePos[6];
float2 cycleVel[6];

sampler2D inputSampler = sampler_state
{
	Texture = (inputTex);
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	output.Position = input.Position;
	output.TexCoord = input.TexCoord;
	
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	const float intensity = 10;
	const float inverseScale = 12;

	float2 coord = input.TexCoord;
	for (int i = 0; i < numPlayers; i++) {
		coord -= cyclePos[i];
	
		float dist = length(coord);
		coord -= intensity * cycleVel[i] * smoothstep(0, 1, max(0, 1 - inverseScale * dist));

		coord += cyclePos[i];
	}

	float4 color = tex2D(inputSampler, coord);

	return color;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
