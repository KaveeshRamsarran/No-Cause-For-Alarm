Shader "NCFA/Emissive"
{
    Properties { [HDR] _EmissionColor("Emission",Color)=(3,1,0,1) _BaseColor("Base",Color)=(1,1,1,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            float4 _EmissionColor;
            float4 vert(float4 positionOS:POSITION):SV_POSITION{return TransformObjectToHClip(positionOS.xyz);}
            half4 frag():SV_Target{return half4(_EmissionColor.rgb,1);}
            ENDHLSL
        }
    }
}
