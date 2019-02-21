// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge Beta 0.35 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.35;sub:START;pass:START;ps:flbk:Unlit/Texture,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:True,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.945098,fgcg:0.9137255,fgcb:0.8470588,fgca:1,fgde:0.02,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:31778,y:32737|emission-31-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:32310,y:32936,ntxv:0,isnm:False|UVIN-94-OUT,MIP-132-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:13,x:32371,y:32935,ptlb:Texture,ptin:_Texture,glob:False;n:type:ShaderForge.SFN_Lerp,id:28,x:32346,y:32894|A-110-OUT,B-277-OUT,T-245-OUT;n:type:ShaderForge.SFN_Lerp,id:31,x:32351,y:32915|A-110-OUT,B-28-OUT,T-267-OUT;n:type:ShaderForge.SFN_Relay,id:94,x:32380,y:32986|IN-202-UVOUT;n:type:ShaderForge.SFN_Clamp01,id:108,x:32288,y:32970|IN-219-OUT;n:type:ShaderForge.SFN_Relay,id:110,x:32380,y:32988|IN-126-OUT;n:type:ShaderForge.SFN_Relay,id:112,x:32321,y:33028|IN-202-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:115,x:32310,y:32970,ntxv:0,isnm:False|UVIN-141-OUT,MIP-132-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Lerp,id:126,x:32351,y:32894|A-2-RGB,B-115-RGB,T-108-OUT;n:type:ShaderForge.SFN_Vector1,id:130,x:32351,y:32986,v1:0;n:type:ShaderForge.SFN_Lerp,id:132,x:32300,y:33028|A-130-OUT,B-133-OUT,T-262-OUT;n:type:ShaderForge.SFN_Relay,id:133,x:32350,y:32936|IN-219-OUT;n:type:ShaderForge.SFN_Add,id:141,x:32462,y:33004|A-143-OUT,B-112-OUT;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:143,x:32321,y:32944|IN-168-OUT,IMIN-144-OUT,IMAX-146-OUT,OMIN-158-OUT,OMAX-160-OUT;n:type:ShaderForge.SFN_Vector1,id:144,x:32267,y:32970,v1:-1;n:type:ShaderForge.SFN_Vector1,id:146,x:32267,y:33028,v1:1;n:type:ShaderForge.SFN_Multiply,id:158,x:32321,y:32915|A-257-OUT,B-245-OUT;n:type:ShaderForge.SFN_Negate,id:160,x:32351,y:32959|IN-158-OUT;n:type:ShaderForge.SFN_Tex2d,id:167,x:32267,y:32935,ptlb:Distortion Map (RG),ptin:_DistortionMapRG,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Append,id:168,x:32394,y:32959|A-167-R,B-167-G;n:type:ShaderForge.SFN_TexCoord,id:202,x:32371,y:32970,uv:0;n:type:ShaderForge.SFN_RemapRange,id:219,x:32321,y:32936,frmn:0,frmx:1,tomn:0,tomx:6|IN-245-OUT;n:type:ShaderForge.SFN_Vector1,id:245,x:32371,y:33072,v1:0.62;n:type:ShaderForge.SFN_Vector1,id:257,x:32351,y:32936,v1:0.1;n:type:ShaderForge.SFN_Vector1,id:262,x:32351,y:32936,v1:0.8;n:type:ShaderForge.SFN_Vector1,id:267,x:32300,y:32959,v1:0.1;n:type:ShaderForge.SFN_Vector3,id:277,x:32321,y:32970,v1:0.5,v2:0.5,v3:0.5;proporder:13-167;pass:END;sub:END;*/

Shader "Utility Shaders/Preview/Distortion By Texture" {
    Properties {
        _Texture ("Texture", 2D) = "white" {}
        _DistortionMapRG ("Distortion Map (RG)", 2D) = "bump" {}
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
            #pragma glsl
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform sampler2D _DistortionMapRG; uniform float4 _DistortionMapRG_ST;
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
                float2 node_202 = i.uv0;
                float2 node_94 = node_202.rg;
                float node_245 = 0.62;
                float node_219 = (node_245*6.0+0.0);
                float node_132 = lerp(0.0,node_219,0.8);
                float2 node_308 = i.uv0;
                float3 node_167 = UnpackNormal(tex2D(_DistortionMapRG,TRANSFORM_TEX(node_308.rg, _DistortionMapRG)));
                float node_144 = (-1.0);
                float node_158 = (0.1*node_245);
                float2 node_141 = ((node_158 + ( (float2(node_167.r,node_167.g) - node_144) * ((-1*node_158) - node_158) ) / (1.0 - node_144))+node_202.rg);
                float3 node_110 = lerp(tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_94, _Texture),0.0,node_132)).rgb,tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_141, _Texture),0.0,node_132)).rgb,saturate(node_219));
                float3 emissive = lerp(node_110,lerp(node_110,float3(0.5,0.5,0.5),node_245),0.1);
                float3 finalColor = emissive;
/// Final Color:
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Unlit/Texture"
    CustomEditor "ShaderForgeMaterialInspector"
}
