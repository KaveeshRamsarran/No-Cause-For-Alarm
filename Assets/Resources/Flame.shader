Shader "NCFA/AnimatedFlame"
{
    SubShader
    {
        Tags {"RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline"}
        Pass
        {
            Tags {"LightMode"="SRPDefaultUnlit"}
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Input {float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
            struct Varying {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
            Varying vert(Input v){Varying o;float t=_Time.y;v.positionOS.x+=(sin(t*8)+sin(t*13)*.35)*v.uv.y*v.uv.y*.09;v.positionOS.y*=1+sin(t*11)*.06;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.uv=v.uv;return o;}
            half4 frag(Varying i):SV_Target
            {
                float y=i.uv.y,t=_Time.y;
                float x=(i.uv.x-.5)*2+sin(y*9-t*9)*.08*y;
                float radius=pow(saturate(1-y),.7)*.78*(.6+.4*sin(saturate(y*4)*1.57));
                float edge=abs(x)/max(radius,.001);
                float alpha=(1-smoothstep(.6,1,edge))*smoothstep(0,.06,y)*(1-smoothstep(.88,1,y));
                float core=(1-smoothstep(.08,.48,abs(x)))*(1-smoothstep(.3,.72,y));
                half3 color=lerp(half3(1.8,.25,.015),half3(3.2,2.5,.65),core);
                color=lerp(half3(.1,.35,1.3),color,smoothstep(0,.19,y));
                return half4(color,alpha*.94);
            }
            ENDHLSL
        }
    }
}
