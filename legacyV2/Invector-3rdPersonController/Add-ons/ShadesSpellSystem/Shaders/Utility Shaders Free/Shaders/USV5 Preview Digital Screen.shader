// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge Beta 0.35 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.35;sub:START;pass:START;ps:flbk:,lico:0,lgpr:1,nrmq:1,limd:0,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:1,culm:0,dpts:2,wrdp:True,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.945098,fgcg:0.9137255,fgcb:0.8470588,fgca:1,fgde:0.02,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:32584,y:32378|emission-34-OUT;n:type:ShaderForge.SFN_Noise,id:2,x:32619,y:32478|XY-31-OUT;n:type:ShaderForge.SFN_TexCoord,id:3,x:32629,y:32468,uv:0;n:type:ShaderForge.SFN_RemapRange,id:5,x:32608,y:32478,frmn:0,frmx:1,tomn:0,tomx:1.5|IN-2-OUT;n:type:ShaderForge.SFN_Multiply,id:6,x:32629,y:32489|A-3-V,B-21-OUT;n:type:ShaderForge.SFN_Vector1,id:8,x:32619,y:32532,v1:1;n:type:ShaderForge.SFN_Lerp,id:9,x:32608,y:32464|A-8-OUT,B-5-OUT,T-38-OUT;n:type:ShaderForge.SFN_Divide,id:13,x:32608,y:32476|A-3-UVOUT,B-75-OUT;n:type:ShaderForge.SFN_Sin,id:20,x:32642,y:32464|IN-6-OUT;n:type:ShaderForge.SFN_Multiply,id:21,x:32629,y:32454|A-72-OUT,B-22-OUT;n:type:ShaderForge.SFN_Tau,id:22,x:32645,y:32512;n:type:ShaderForge.SFN_Abs,id:23,x:32629,y:32464|IN-20-OUT;n:type:ShaderForge.SFN_Blend,id:24,x:32629,y:32476,blmd:10,clmp:True|SRC-23-OUT,DST-23-OUT;n:type:ShaderForge.SFN_Lerp,id:25,x:32608,y:32498|A-8-OUT,B-24-OUT,T-48-OUT;n:type:ShaderForge.SFN_Multiply,id:27,x:32629,y:32512|A-9-OUT,B-25-OUT;n:type:ShaderForge.SFN_Posterize,id:29,x:32595,y:32478|IN-3-UVOUT,STPS-77-OUT;n:type:ShaderForge.SFN_Lerp,id:31,x:32608,y:32476|A-13-OUT,B-29-OUT,T-58-OUT;n:type:ShaderForge.SFN_Tex2d,id:33,x:32608,y:32481,ptlb:Texture,ptin:_Texture,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:34,x:32608,y:32464|A-27-OUT,B-33-RGB;n:type:ShaderForge.SFN_Vector1,id:38,x:32608,y:32512,v1:0.1;n:type:ShaderForge.SFN_Vector1,id:48,x:32595,y:32498,v1:0.2;n:type:ShaderForge.SFN_Vector1,id:58,x:32595,y:32532,v1:1;n:type:ShaderForge.SFN_Vector1,id:72,x:32629,y:32465,v1:32;n:type:ShaderForge.SFN_Vector1,id:75,x:32619,y:32532,v1:1;n:type:ShaderForge.SFN_Vector1,id:77,x:32595,y:32498,v1:256;proporder:33;pass:END;sub:END;*/

Shader "Utility Shaders/Preview/Digital Screen" {
    Properties {
        _Texture ("Texture", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
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
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
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
////// Lighting:
////// Emissive:
                float node_8 = 1.0;
                float2 node_3 = i.uv0;
                float node_77 = 256.0;
                float2 node_31 = lerp((node_3.rg/1.0),floor(node_3.rg * node_77) / (node_77 - 1),1.0);
                float2 node_2_skew = node_31 + 0.2127+node_31.x*0.3713*node_31.y;
                float2 node_2_rnd = 4.789*sin(489.123*(node_2_skew));
                float node_2 = frac(node_2_rnd.x*node_2_rnd.y*(1+node_2_skew.x));
                float node_23 = abs(sin((node_3.g*(32.0*6.28318530718))));
                float2 node_90 = i.uv0;
                float3 emissive = ((lerp(node_8,(node_2*1.5+0.0),0.1)*lerp(node_8,saturate(( node_23 > 0.5 ? (1.0-(1.0-2.0*(node_23-0.5))*(1.0-node_23)) : (2.0*node_23*node_23) )),0.2))*tex2D(_Texture,TRANSFORM_TEX(node_90.rg, _Texture)).rgb);
                float3 finalColor = emissive;
/// Final Color:
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
