// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge Beta 0.35 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.35;sub:START;pass:START;ps:flbk:,lico:0,lgpr:1,nrmq:1,limd:0,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:True,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.945098,fgcg:0.9137255,fgcb:0.8470588,fgca:1,fgde:0.02,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:33067,y:32861|emission-243-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:15,x:33110,y:32997,ptlb:Heightmap,ptin:_Heightmap,glob:False,tex:28c7aad1372ff114b90d330f8a2dd938;n:type:ShaderForge.SFN_Tex2d,id:134,x:33127,y:32965,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False|UVIN-154-OUT,MIP-455-OUT,TEX-15-TEX;n:type:ShaderForge.SFN_TexCoord,id:135,x:33139,y:32969,uv:0;n:type:ShaderForge.SFN_Add,id:136,x:33147,y:32971|A-135-UVOUT,B-266-OUT;n:type:ShaderForge.SFN_Vector2,id:143,x:33180,y:32978,v1:0,v2:-1;n:type:ShaderForge.SFN_Vector2,id:145,x:33180,y:32987,v1:-1,v2:0;n:type:ShaderForge.SFN_Vector2,id:146,x:33180,y:32975,v1:1,v2:0;n:type:ShaderForge.SFN_Vector2,id:147,x:33180,y:32995,v1:0,v2:1;n:type:ShaderForge.SFN_Add,id:150,x:33147,y:32956|A-135-UVOUT,B-269-OUT;n:type:ShaderForge.SFN_Add,id:152,x:33114,y:32957|A-135-UVOUT,B-271-OUT;n:type:ShaderForge.SFN_Add,id:154,x:33169,y:32983|A-135-UVOUT,B-273-OUT;n:type:ShaderForge.SFN_Relay,id:155,x:33139,y:32983|IN-135-UVOUT;n:type:ShaderForge.SFN_Tex2d,id:157,x:33139,y:32954,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False|UVIN-152-OUT,MIP-455-OUT,TEX-15-TEX;n:type:ShaderForge.SFN_Tex2d,id:159,x:33139,y:32969,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False|UVIN-136-OUT,MIP-455-OUT,TEX-15-TEX;n:type:ShaderForge.SFN_Tex2d,id:161,x:33139,y:32952,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False|UVIN-150-OUT,MIP-455-OUT,TEX-15-TEX;n:type:ShaderForge.SFN_Tex2d,id:163,x:33124,y:32987,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:0,isnm:False|UVIN-155-OUT,MIP-455-OUT,TEX-15-TEX;n:type:ShaderForge.SFN_Subtract,id:177,x:33110,y:32963|A-229-OUT,B-231-OUT;n:type:ShaderForge.SFN_Abs,id:178,x:33128,y:32963|IN-177-OUT;n:type:ShaderForge.SFN_Subtract,id:182,x:33126,y:32988|A-229-OUT,B-233-OUT;n:type:ShaderForge.SFN_Abs,id:184,x:33139,y:33035|IN-182-OUT;n:type:ShaderForge.SFN_Abs,id:190,x:33110,y:32963|IN-194-OUT;n:type:ShaderForge.SFN_Subtract,id:194,x:33139,y:32988|A-229-OUT,B-235-OUT;n:type:ShaderForge.SFN_Abs,id:196,x:33142,y:32984|IN-200-OUT;n:type:ShaderForge.SFN_Subtract,id:200,x:33128,y:32963|A-229-OUT,B-237-OUT;n:type:ShaderForge.SFN_Multiply,id:228,x:33102,y:32949|A-454-OUT,B-319-OUT;n:type:ShaderForge.SFN_Posterize,id:229,x:33139,y:32954|IN-163-RGB,STPS-454-OUT;n:type:ShaderForge.SFN_Posterize,id:231,x:33127,y:32954|IN-159-RGB,STPS-454-OUT;n:type:ShaderForge.SFN_Posterize,id:233,x:33139,y:32972|IN-161-RGB,STPS-454-OUT;n:type:ShaderForge.SFN_Posterize,id:235,x:33139,y:32988|IN-157-RGB,STPS-454-OUT;n:type:ShaderForge.SFN_Posterize,id:237,x:33127,y:32954|IN-134-RGB,STPS-454-OUT;n:type:ShaderForge.SFN_Desaturate,id:238,x:33102,y:32968|COL-228-OUT,DES-355-OUT;n:type:ShaderForge.SFN_Step,id:239,x:33102,y:32983|A-240-OUT,B-238-OUT;n:type:ShaderForge.SFN_Vector1,id:240,x:33128,y:33052,v1:0.1;n:type:ShaderForge.SFN_OneMinus,id:242,x:33102,y:32969|IN-302-OUT;n:type:ShaderForge.SFN_Multiply,id:243,x:33139,y:32988|A-447-OUT,B-444-OUT;n:type:ShaderForge.SFN_Multiply,id:260,x:33142,y:32983|A-239-OUT,B-261-OUT;n:type:ShaderForge.SFN_Vector1,id:261,x:33114,y:33017,v1:0.5;n:type:ShaderForge.SFN_Multiply,id:266,x:33139,y:32955|A-145-OUT,B-456-OUT;n:type:ShaderForge.SFN_Multiply,id:269,x:33126,y:32957|A-146-OUT,B-456-OUT;n:type:ShaderForge.SFN_Multiply,id:271,x:33139,y:32956|A-143-OUT,B-456-OUT;n:type:ShaderForge.SFN_Multiply,id:273,x:33139,y:32973|A-147-OUT,B-456-OUT;n:type:ShaderForge.SFN_Clamp01,id:302,x:33102,y:32983|IN-260-OUT;n:type:ShaderForge.SFN_Add,id:319,x:33114,y:32983|A-178-OUT,B-184-OUT,C-190-OUT,D-196-OUT;n:type:ShaderForge.SFN_Vector1,id:355,x:33087,y:32963,v1:1;n:type:ShaderForge.SFN_Relay,id:443,x:33155,y:32987|IN-229-OUT;n:type:ShaderForge.SFN_Multiply,id:444,x:33155,y:32985|A-445-OUT,B-453-OUT;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:445,x:33155,y:32957|IN-443-OUT,IMIN-448-OUT,IMAX-450-OUT,OMIN-452-OUT,OMAX-450-OUT;n:type:ShaderForge.SFN_Relay,id:447,x:33155,y:33026,cmnt:Mip Detection|IN-242-OUT;n:type:ShaderForge.SFN_Vector1,id:448,x:33155,y:32951,v1:0;n:type:ShaderForge.SFN_Vector1,id:450,x:33155,y:33069,v1:1;n:type:ShaderForge.SFN_Vector1,id:452,x:33155,y:33017,v1:0.6;n:type:ShaderForge.SFN_Vector3,id:453,x:33125,y:33001,v1:0.92,v2:0.88,v3:0.8;n:type:ShaderForge.SFN_Vector1,id:454,x:33139,y:33001,v1:6;n:type:ShaderForge.SFN_Vector1,id:455,x:33139,y:33065,v1:2.6;n:type:ShaderForge.SFN_Vector1,id:456,x:33180,y:32988,v1:0.002;proporder:15;pass:END;sub:END;*/

Shader "Utility Shaders/Preview/Cartography 1" {
    Properties {
        _Heightmap ("Heightmap", 2D) = "white" {}
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
            uniform sampler2D _Heightmap; uniform float4 _Heightmap_ST;
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
                float node_454 = 6.0;
                float2 node_135 = i.uv0;
                float2 node_155 = node_135.rg;
                float node_455 = 2.6;
                float3 node_229 = floor(tex2Dlod(_Heightmap,float4(TRANSFORM_TEX(node_155, _Heightmap),0.0,node_455)).rgb * node_454) / (node_454 - 1);
                float node_456 = 0.002;
                float2 node_136 = (node_135.rg+(float2(-1,0)*node_456));
                float2 node_150 = (node_135.rg+(float2(1,0)*node_456));
                float2 node_152 = (node_135.rg+(float2(0,-1)*node_456));
                float2 node_154 = (node_135.rg+(float2(0,1)*node_456));
                float node_448 = 0.0;
                float node_450 = 1.0;
                float node_452 = 0.6;
                float3 emissive = ((1.0 - saturate((step(0.1,lerp((node_454*(abs((node_229-floor(tex2Dlod(_Heightmap,float4(TRANSFORM_TEX(node_136, _Heightmap),0.0,node_455)).rgb * node_454) / (node_454 - 1)))+abs((node_229-floor(tex2Dlod(_Heightmap,float4(TRANSFORM_TEX(node_150, _Heightmap),0.0,node_455)).rgb * node_454) / (node_454 - 1)))+abs((node_229-floor(tex2Dlod(_Heightmap,float4(TRANSFORM_TEX(node_152, _Heightmap),0.0,node_455)).rgb * node_454) / (node_454 - 1)))+abs((node_229-floor(tex2Dlod(_Heightmap,float4(TRANSFORM_TEX(node_154, _Heightmap),0.0,node_455)).rgb * node_454) / (node_454 - 1))))),dot((node_454*(abs((node_229-floor(tex2Dlod(_Heightmap,float4(TRANSFORM_TEX(node_136, _Heightmap),0.0,node_455)).rgb * node_454) / (node_454 - 1)))+abs((node_229-floor(tex2Dlod(_Heightmap,float4(TRANSFORM_TEX(node_150, _Heightmap),0.0,node_455)).rgb * node_454) / (node_454 - 1)))+abs((node_229-floor(tex2Dlod(_Heightmap,float4(TRANSFORM_TEX(node_152, _Heightmap),0.0,node_455)).rgb * node_454) / (node_454 - 1)))+abs((node_229-floor(tex2Dlod(_Heightmap,float4(TRANSFORM_TEX(node_154, _Heightmap),0.0,node_455)).rgb * node_454) / (node_454 - 1))))),float3(0.3,0.59,0.11)),1.0))*0.5)))*((node_452 + ( (node_229 - node_448) * (node_450 - node_452) ) / (node_450 - node_448))*float3(0.92,0.88,0.8)));
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
