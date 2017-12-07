// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge Beta 0.35 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.35;sub:START;pass:START;ps:flbk:Unlit/Transparent Cutout,lico:0,lgpr:1,nrmq:1,limd:0,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:False,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:True,ufog:True,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.945098,fgcg:0.9137255,fgcb:0.8470588,fgca:1,fgde:0.02,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32977,y:32917|emission-13-OUT,clip-123-OUT;n:type:ShaderForge.SFN_Lerp,id:10,x:33037,y:33086|A-98-OUT,B-96-OUT,T-38-OUT;n:type:ShaderForge.SFN_Lerp,id:12,x:33037,y:33086|A-96-OUT,B-100-OUT,T-38-OUT;n:type:ShaderForge.SFN_Lerp,id:13,x:33011,y:33086|A-10-OUT,B-12-OUT,T-38-OUT;n:type:ShaderForge.SFN_Relay,id:38,x:33053,y:33095|IN-61-OUT;n:type:ShaderForge.SFN_TexCoord,id:53,x:33066,y:33086,uv:0;n:type:ShaderForge.SFN_Blend,id:59,x:33037,y:33072,blmd:10,clmp:True|SRC-65-OUT,DST-65-OUT;n:type:ShaderForge.SFN_Lerp,id:61,x:33011,y:33086|A-65-OUT,B-59-OUT,T-132-OUT;n:type:ShaderForge.SFN_Relay,id:65,x:33066,y:33095|IN-77-OUT;n:type:ShaderForge.SFN_Clamp01,id:73,x:33037,y:33061|IN-131-OUT;n:type:ShaderForge.SFN_Step,id:75,x:33037,y:33086|A-95-OUT,B-73-OUT;n:type:ShaderForge.SFN_Posterize,id:77,x:33024,y:33086|IN-53-V,STPS-134-OUT;n:type:ShaderForge.SFN_Multiply,id:79,x:32996,y:33061|A-53-V,B-134-OUT;n:type:ShaderForge.SFN_Frac,id:81,x:33037,y:33061|IN-87-OUT;n:type:ShaderForge.SFN_Lerp,id:83,x:33037,y:33086|A-85-OUT,B-89-OUT,T-101-OUT;n:type:ShaderForge.SFN_Vector1,id:85,x:33011,y:33120,v1:0.7;n:type:ShaderForge.SFN_Relay,id:87,x:33066,y:33095|IN-79-OUT;n:type:ShaderForge.SFN_Step,id:89,x:33011,y:33061|A-133-OUT,B-81-OUT;n:type:ShaderForge.SFN_Multiply,id:91,x:33037,y:33086|A-93-OUT,B-75-OUT;n:type:ShaderForge.SFN_Clamp01,id:93,x:33011,y:33061|IN-83-OUT;n:type:ShaderForge.SFN_Lerp,id:95,x:32996,y:33061|A-53-V,B-77-OUT,T-101-OUT;n:type:ShaderForge.SFN_Multiply,id:96,x:33037,y:33061|A-99-OUT,B-97-OUT;n:type:ShaderForge.SFN_Vector1,id:97,x:33037,y:33120,v1:2;n:type:ShaderForge.SFN_Vector3,id:98,x:33066,y:33079,v1:0,v2:1,v3:0;n:type:ShaderForge.SFN_Vector3,id:99,x:33037,y:33104,v1:0.7,v2:0.7,v3:0;n:type:ShaderForge.SFN_Vector3,id:100,x:33037,y:33104,v1:1,v2:0,v3:0;n:type:ShaderForge.SFN_Vector1,id:101,x:33024,y:33120,v1:1;n:type:ShaderForge.SFN_Lerp,id:102,x:33037,y:33086|A-125-OUT,B-127-OUT,T-122-OUT;n:type:ShaderForge.SFN_Step,id:103,x:33011,y:33086|A-109-U,B-104-OUT;n:type:ShaderForge.SFN_Vector1,id:104,x:33011,y:33120,v1:0.3;n:type:ShaderForge.SFN_Vector1,id:106,x:33024,y:33120,v1:0.7;n:type:ShaderForge.SFN_Vector1,id:108,x:33024,y:33095,v1:0.35;n:type:ShaderForge.SFN_TexCoord,id:109,x:33047,y:33076,uv:0;n:type:ShaderForge.SFN_Step,id:111,x:33037,y:33086|A-106-OUT,B-109-U;n:type:ShaderForge.SFN_Vector1,id:113,x:33047,y:33095,v1:0.65;n:type:ShaderForge.SFN_Step,id:119,x:33037,y:33086|A-108-OUT,B-109-U;n:type:ShaderForge.SFN_Step,id:121,x:33047,y:33086|A-109-U,B-113-OUT;n:type:ShaderForge.SFN_Multiply,id:122,x:33011,y:33086|A-119-OUT,B-121-OUT;n:type:ShaderForge.SFN_Multiply,id:123,x:33024,y:33061|A-91-OUT,B-124-OUT;n:type:ShaderForge.SFN_Add,id:124,x:33011,y:33072|A-103-OUT,B-122-OUT,C-111-OUT;n:type:ShaderForge.SFN_Vector1,id:125,x:33037,y:33120,v1:1;n:type:ShaderForge.SFN_Vector1,id:127,x:33037,y:33061,v1:0.4;n:type:ShaderForge.SFN_Vector1,id:129,x:33037,y:33156,v1:0.8;n:type:ShaderForge.SFN_Lerp,id:131,x:33011,y:33061|A-102-OUT,B-129-OUT,T-111-OUT;n:type:ShaderForge.SFN_Vector1,id:132,x:33358,y:32987,v1:0;n:type:ShaderForge.SFN_Vector1,id:133,x:33037,y:33106,v1:0.55;n:type:ShaderForge.SFN_Vector1,id:134,x:33066,y:33120,v1:10;pass:END;sub:END;*/

Shader "Utility Shaders/Preview/Bar Graph" {
    Properties {
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                float2 node_53 = i.uv0;
                float node_134 = 10.0;
                float node_101 = 1.0;
                float node_77 = floor(node_53.g * node_134) / (node_134 - 1);
                float2 node_109 = i.uv0;
                float node_122 = (step(0.35,node_109.r)*step(node_109.r,0.65));
                float node_111 = step(0.7,node_109.r);
                clip(((saturate(lerp(0.7,step(0.55,frac((node_53.g*node_134))),node_101))*step(lerp(node_53.g,node_77,node_101),saturate(lerp(lerp(1.0,0.4,node_122),0.8,node_111))))*(step(node_109.r,0.3)+node_122+node_111)) - 0.5);
////// Lighting:
////// Emissive:
                float3 node_96 = (float3(0.7,0.7,0)*2.0);
                float node_65 = node_77;
                float node_38 = lerp(node_65,saturate(( node_65 > 0.5 ? (1.0-(1.0-2.0*(node_65-0.5))*(1.0-node_65)) : (2.0*node_65*node_65) )),0.0);
                float3 node_13 = lerp(lerp(float3(0,1,0),node_96,node_38),lerp(node_96,float3(1,0,0),node_38),node_38);
                float3 emissive = node_13;
                float3 finalColor = emissive;
/// Final Color:
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCollector"
            Tags {
                "LightMode"="ShadowCollector"
            }
            
            Fog {Mode Off}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCOLLECTOR
            #define SHADOW_COLLECTOR_PASS
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcollector
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_COLLECTOR;
                float2 uv0 : TEXCOORD5;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_SHADOW_COLLECTOR(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                float2 node_53 = i.uv0;
                float node_134 = 10.0;
                float node_101 = 1.0;
                float node_77 = floor(node_53.g * node_134) / (node_134 - 1);
                float2 node_109 = i.uv0;
                float node_122 = (step(0.35,node_109.r)*step(node_109.r,0.65));
                float node_111 = step(0.7,node_109.r);
                clip(((saturate(lerp(0.7,step(0.55,frac((node_53.g*node_134))),node_101))*step(lerp(node_53.g,node_77,node_101),saturate(lerp(lerp(1.0,0.4,node_122),0.8,node_111))))*(step(node_109.r,0.3)+node_122+node_111)) - 0.5);
                SHADOW_COLLECTOR_FRAGMENT(i)
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Cull Off
            Offset 1, 1
            
            Fog {Mode Off}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                float2 node_53 = i.uv0;
                float node_134 = 10.0;
                float node_101 = 1.0;
                float node_77 = floor(node_53.g * node_134) / (node_134 - 1);
                float2 node_109 = i.uv0;
                float node_122 = (step(0.35,node_109.r)*step(node_109.r,0.65));
                float node_111 = step(0.7,node_109.r);
                clip(((saturate(lerp(0.7,step(0.55,frac((node_53.g*node_134))),node_101))*step(lerp(node_53.g,node_77,node_101),saturate(lerp(lerp(1.0,0.4,node_122),0.8,node_111))))*(step(node_109.r,0.3)+node_122+node_111)) - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent Cutout"
    CustomEditor "ShaderForgeMaterialInspector"
}
