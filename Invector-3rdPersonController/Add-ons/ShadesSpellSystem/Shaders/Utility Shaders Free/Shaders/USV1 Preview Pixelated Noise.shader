// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge Beta 0.35 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.35;sub:START;pass:START;ps:flbk:Unlit/Texture,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:True,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.945098,fgcg:0.9137255,fgcb:0.8470588,fgca:1,fgde:0.02,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:31778,y:32737|emission-31-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:32346,y:32893,ntxv:0,isnm:False|UVIN-94-OUT,MIP-132-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:13,x:32327,y:32893,ptlb:Texture,ptin:_Texture,glob:False;n:type:ShaderForge.SFN_Lerp,id:28,x:32346,y:32900|A-110-OUT,B-182-OUT,T-177-OUT;n:type:ShaderForge.SFN_Lerp,id:31,x:32346,y:32891|A-110-OUT,B-28-OUT,T-181-OUT;n:type:ShaderForge.SFN_Relay,id:84,x:32297,y:32870|IN-175-OUT;n:type:ShaderForge.SFN_Relay,id:94,x:32345,y:32881|IN-139-UVOUT;n:type:ShaderForge.SFN_Clamp01,id:108,x:32346,y:32917|IN-175-OUT;n:type:ShaderForge.SFN_Relay,id:110,x:32327,y:32944|IN-126-OUT;n:type:ShaderForge.SFN_Relay,id:112,x:32297,y:32988|IN-139-UVOUT;n:type:ShaderForge.SFN_Posterize,id:113,x:32297,y:32944|IN-112-OUT,STPS-129-OUT;n:type:ShaderForge.SFN_Tex2d,id:115,x:32327,y:32944,ntxv:0,isnm:False|UVIN-141-OUT,MIP-132-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Power,id:116,x:32327,y:32900|VAL-117-OUT,EXP-84-OUT;n:type:ShaderForge.SFN_Vector1,id:117,x:32356,y:32881,v1:2;n:type:ShaderForge.SFN_Divide,id:123,x:32368,y:32917|A-176-OUT,B-116-OUT;n:type:ShaderForge.SFN_Lerp,id:126,x:32327,y:32944|A-2-RGB,B-115-RGB,T-108-OUT;n:type:ShaderForge.SFN_Floor,id:129,x:32346,y:32900|IN-123-OUT;n:type:ShaderForge.SFN_Vector1,id:130,x:32297,y:33008,v1:0;n:type:ShaderForge.SFN_Lerp,id:132,x:32327,y:32917|A-130-OUT,B-133-OUT,T-180-OUT;n:type:ShaderForge.SFN_Relay,id:133,x:32327,y:32944|IN-175-OUT;n:type:ShaderForge.SFN_TexCoord,id:139,x:32316,y:32917,uv:0;n:type:ShaderForge.SFN_Noise,id:140,x:32327,y:32944|XY-161-OUT;n:type:ShaderForge.SFN_Add,id:141,x:32346,y:32904|A-143-OUT,B-113-OUT;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:143,x:32327,y:32910|IN-140-OUT,IMIN-144-OUT,IMAX-146-OUT,OMIN-158-OUT,OMAX-160-OUT;n:type:ShaderForge.SFN_Vector1,id:144,x:32316,y:32944,v1:0;n:type:ShaderForge.SFN_Vector1,id:146,x:32327,y:32961,v1:1;n:type:ShaderForge.SFN_Multiply,id:158,x:32327,y:32900|A-178-OUT,B-177-OUT;n:type:ShaderForge.SFN_Negate,id:160,x:32316,y:32900|IN-158-OUT;n:type:ShaderForge.SFN_Add,id:161,x:32327,y:32944|A-179-OUT,B-113-OUT;n:type:ShaderForge.SFN_RemapRange,id:175,x:32327,y:32892,frmn:0,frmx:1,tomn:0,tomx:6|IN-177-OUT;n:type:ShaderForge.SFN_Vector1,id:176,x:32297,y:32910,v1:200;n:type:ShaderForge.SFN_Vector1,id:177,x:32356,y:32961,v1:0.5;n:type:ShaderForge.SFN_Vector1,id:178,x:32346,y:33008,v1:0.1;n:type:ShaderForge.SFN_Vector1,id:179,x:32297,y:33008,v1:1;n:type:ShaderForge.SFN_Vector1,id:180,x:32346,y:32944,v1:1;n:type:ShaderForge.SFN_Vector1,id:181,x:32346,y:32944,v1:0;n:type:ShaderForge.SFN_Vector4,id:182,x:32316,y:32928,v1:0,v2:0,v3:0,v4:0;proporder:13;pass:END;sub:END;*/

Shader "Utility Shaders/Preview/Noise" {
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
            #pragma glsl
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
                float2 node_139 = i.uv0;
                float2 node_94 = node_139.rg;
                float node_177 = 0.5;
                float node_175 = (node_177*6.0+0.0);
                float node_132 = lerp(0.0,node_175,1.0);
                float node_129 = floor((200.0/pow(2.0,node_175)));
                float2 node_113 = floor(node_139.rg * node_129) / (node_129 - 1);
                float2 node_161 = (1.0+node_113);
                float2 node_140_skew = node_161 + 0.2127+node_161.x*0.3713*node_161.y;
                float2 node_140_rnd = 4.789*sin(489.123*(node_140_skew));
                float node_140 = frac(node_140_rnd.x*node_140_rnd.y*(1+node_140_skew.x));
                float node_144 = 0.0;
                float node_158 = (0.1*node_177);
                float2 node_141 = ((node_158 + ( (node_140 - node_144) * ((-1*node_158) - node_158) ) / (1.0 - node_144))+node_113);
                float3 node_110 = lerp(tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_94, _Texture),0.0,node_132)).rgb,tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_141, _Texture),0.0,node_132)).rgb,saturate(node_175));
                float3 emissive = lerp(float4(node_110,0.0),lerp(float4(node_110,0.0),float4(0,0,0,0),node_177),0.0).rgb;
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
