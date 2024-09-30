#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

float alphaValue = 1;
float3 lightPosition = float3(1000, 1000, 1000);
float time = 0;

//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
    Texture = (texDiffuseMap);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

texture texDiffuseMap2;
sampler2D diffuseMap2 = sampler_state
{
    Texture = (texDiffuseMap2);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

texture texColorMap;
sampler2D colorMap = sampler_state
{
    Texture = (texColorMap);
    ADDRESSU = WRAP; //pasarlo a clamp si falla la idea
    ADDRESSV = WRAP; //pasarlo a clamp si falla la idea
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

texture texNormalMap1;
sampler2D normalMap1 = sampler_state
{
    Texture = (texNormalMap1);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

texture texNormalMap2;
sampler2D normalMap2 = sampler_state
{
    Texture = (texNormalMap2);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

struct VS_INPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
};

VS_OUTPUT vs_RenderTerrain(VS_INPUT input)
{
    VS_OUTPUT output;

    //Proyectar posicion
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    //Enviar Texcoord directamente
    output.Texcoord = input.Texcoord;

    //todo: que le pase el inv trasp. word
    float3x4 matInverseTransposeWorld = World;//modifique esta linea recordatorio
    output.WorldPos = worldPosition.xyz;
    output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;

    return output;
}

struct PS_INPUT
{
    float2 Texcoord : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
};

//Pixel Shader
float4 ps_RenderTerrain(PS_INPUT input) : COLOR0
{
    float3 c = tex2D(colorMap, input.Texcoord).rgb;
    
    //calculo de las normales
    float3 normal1 = tex2D(normalMap1, input.Texcoord * 20).rgb * 2.0 - 1.0; // Convertir de [0,1] a [-1,1]
    float3 normal2 = tex2D(normalMap2, input.Texcoord * 20).rgb * 2.0 - 1.0; // Convertir de [0,1] a [-1,1]
    float3 blendedNormal = normalize(lerp(normal1, normal2, c.r));//agregado para prueba
    
    //calculo de las luces
    float3 N = normalize(blendedNormal);
    float3 L = normalize(lightPosition - input.WorldPos);
    float kd = saturate(0.4 + 0.6 * saturate(dot(N, L)));
    
    float3 tex1 = tex2D(diffuseMap, input.Texcoord * 20).rgb;
    float3 tex2 = tex2D(diffuseMap2, input.Texcoord * 20).rgb;
    
    /*
    float3 clr;
    float3 baseTexture;
    
    if (c.b > c.g && c.b > c.r)
    {
        clr = tex2;
    }
    else
    {
        baseTexture = lerp(tex1, tex2, c.r);
        clr = lerp(baseTexture, c, 0.7);
    }*/
    
    //float3 clr = lerp(baseTexture, tex2, saturate(c.b * 2.0)); // Mezclar hacia diffuseMap2 si el azul es fuerte
    float blendFactor = saturate(c.b * 1.5); // Multiplicamos para hacer más sensible la mezcla al azul
    float3 baseTexture = lerp(tex1, tex2, blendFactor); // Si c.b es alto, usa más tex2 (diffuseMap2)
    float3 clr = lerp(baseTexture, c, 0.7);
    
    //clr.b = saturate(clr.b * 1.5); //agregado por mi para pruebas, al azul lo vuelve mas azul
    clr * 0.6; // //agregado por mi para pruebas, volverlo mas opaco

        return float4(clr * kd, 1);
    }

technique RenderTerrain
{
    pass Pass_0
    {
        VertexShader = compile VS_SHADERMODEL vs_RenderTerrain();
        PixelShader = compile PS_SHADERMODEL ps_RenderTerrain();
    }
}
