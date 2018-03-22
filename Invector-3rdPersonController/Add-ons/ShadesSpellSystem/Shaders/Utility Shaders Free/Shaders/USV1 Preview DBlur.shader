// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge Beta 0.35 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.35;sub:START;pass:START;ps:flbk:Unlit/Texture,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:True,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.945098,fgcg:0.9137255,fgcb:0.8470588,fgca:1,fgde:0.02,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:31724,y:32625|emission-107-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:32189,y:32763,tex:9b5c35293e835e84eab33a6887bc527b,ntxv:0,isnm:False|UVIN-92-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:13,x:32201,y:32742,ptlb:Texture,ptin:_Texture,glob:False,tex:9b5c35293e835e84eab33a6887bc527b;n:type:ShaderForge.SFN_Tex2d,id:57,x:32149,y:32708,tex:9b5c35293e835e84eab33a6887bc527b,ntxv:0,isnm:False|UVIN-87-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:59,x:32174,y:32708,tex:9b5c35293e835e84eab33a6887bc527b,ntxv:0,isnm:False|UVIN-94-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:61,x:32237,y:32708,tex:9b5c35293e835e84eab33a6887bc527b,ntxv:0,isnm:False|UVIN-72-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:63,x:32201,y:32742,tex:9b5c35293e835e84eab33a6887bc527b,ntxv:0,isnm:False|UVIN-90-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Add,id:64,x:32174,y:32763|A-207-OUT,B-206-RGB,C-232-RGB,D-256-RGB,E-262-RGB;n:type:ShaderForge.SFN_Divide,id:65,x:32222,y:32742|A-64-OUT,B-66-OUT;n:type:ShaderForge.SFN_Vector1,id:66,x:32149,y:32742,v1:13;n:type:ShaderForge.SFN_TexCoord,id:71,x:32161,y:32742,uv:0;n:type:ShaderForge.SFN_Add,id:72,x:32222,y:32723,cmnt:plus plus|A-120-OUT,B-124-OUT;n:type:ShaderForge.SFN_Relay,id:84,x:32252,y:32742|IN-302-OUT;n:type:ShaderForge.SFN_Relay,id:85,x:32230,y:32776|IN-84-OUT;n:type:ShaderForge.SFN_Divide,id:86,x:32174,y:32742,cmnt:Calculate Offset by Mip|A-194-OUT,B-314-OUT;n:type:ShaderForge.SFN_Subtract,id:87,x:32174,y:32742,cmnt:minus|A-120-OUT,B-86-OUT;n:type:ShaderForge.SFN_Add,id:90,x:32189,y:32742,cmnt:plus|A-120-OUT,B-86-OUT;n:type:ShaderForge.SFN_Subtract,id:92,x:32189,y:32783|A-120-OUT,B-124-OUT;n:type:ShaderForge.SFN_Relay,id:94,x:32161,y:32742|IN-120-OUT;n:type:ShaderForge.SFN_Lerp,id:107,x:32201,y:32749|A-59-RGB,B-209-OUT,T-108-OUT;n:type:ShaderForge.SFN_Clamp01,id:108,x:32201,y:32742|IN-84-OUT;n:type:ShaderForge.SFN_ConstantClamp,id:109,x:32201,y:32698,min:0,max:1|IN-84-OUT;n:type:ShaderForge.SFN_Relay,id:110,x:32252,y:32797,cmnt:Mip Level|IN-126-OUT;n:type:ShaderForge.SFN_Relay,id:118,x:32222,y:32742,cmnt:V|IN-71-U;n:type:ShaderForge.SFN_Relay,id:119,x:32230,y:32767,cmnt:U|IN-71-V;n:type:ShaderForge.SFN_Append,id:120,x:32189,y:32763|A-118-OUT,B-119-OUT;n:type:ShaderForge.SFN_Vector1,id:123,x:32180,y:32812,v1:2;n:type:ShaderForge.SFN_Multiply,id:124,x:32212,y:32698,cmnt:Double Offset|A-86-OUT,B-123-OUT;n:type:ShaderForge.SFN_Lerp,id:126,x:32174,y:32698|A-127-OUT,B-109-OUT,T-311-OUT;n:type:ShaderForge.SFN_Vector1,id:127,x:32189,y:32742,v1:0;n:type:ShaderForge.SFN_Vector1,id:154,x:32487,y:32536,v1:0;n:type:ShaderForge.SFN_Vector1,id:156,x:32174,y:32742,v1:360;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:173,x:32174,y:32742|IN-316-OUT,IMIN-154-OUT,IMAX-156-OUT,OMIN-175-OUT,OMAX-174-OUT;n:type:ShaderForge.SFN_Pi,id:174,x:32217,y:32852;n:type:ShaderForge.SFN_Negate,id:175,x:32201,y:32698|IN-174-OUT;n:type:ShaderForge.SFN_Sin,id:176,x:32277,y:32698|IN-173-OUT;n:type:ShaderForge.SFN_Cos,id:177,x:32174,y:32742|IN-173-OUT;n:type:ShaderForge.SFN_Append,id:178,x:32201,y:32698|A-176-OUT,B-177-OUT;n:type:ShaderForge.SFN_Relay,id:179,x:32230,y:32797,cmnt:negative Offset|IN-178-OUT;n:type:ShaderForge.SFN_Multiply,id:194,x:32149,y:32742|A-179-OUT,B-85-OUT;n:type:ShaderForge.SFN_Subtract,id:196,x:32189,y:32763,cmnt:plus plus|A-120-OUT,B-200-OUT;n:type:ShaderForge.SFN_Add,id:198,x:32222,y:32679,cmnt:minus minus|A-120-OUT,B-200-OUT;n:type:ShaderForge.SFN_Multiply,id:200,x:32174,y:32698,cmnt:Tripple Ofsset|A-86-OUT,B-202-OUT;n:type:ShaderForge.SFN_Vector1,id:202,x:32201,y:32894,v1:3;n:type:ShaderForge.SFN_Tex2d,id:204,x:32174,y:32742,tex:9b5c35293e835e84eab33a6887bc527b,ntxv:0,isnm:False|UVIN-198-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:206,x:32174,y:32698,tex:9b5c35293e835e84eab33a6887bc527b,ntxv:0,isnm:False|UVIN-196-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Add,id:207,x:32174,y:32683,cmnt:Blur Putput|A-284-OUT,B-63-RGB,C-59-RGB,D-57-RGB,E-2-RGB;n:type:ShaderForge.SFN_Lerp,id:209,x:32149,y:32742|A-65-OUT,B-223-OUT,T-312-OUT;n:type:ShaderForge.SFN_Relay,id:210,x:32201,y:32767|IN-59-RGB;n:type:ShaderForge.SFN_Add,id:211,x:32174,y:32742|A-63-RGB,B-57-RGB;n:type:ShaderForge.SFN_Add,id:212,x:32222,y:32708,cmnt:ppp|A-204-RGB,B-204-RGB;n:type:ShaderForge.SFN_Add,id:213,x:32201,y:32742,cmnt:pppp|A-230-RGB,B-232-RGB;n:type:ShaderForge.SFN_Add,id:218,x:32149,y:32733|A-213-OUT,B-248-OUT,C-241-OUT,D-244-OUT,E-220-OUT;n:type:ShaderForge.SFN_Multiply,id:220,x:32201,y:32733|A-210-OUT,B-222-OUT;n:type:ShaderForge.SFN_Vector1,id:222,x:32161,y:32742,v1:32;n:type:ShaderForge.SFN_Divide,id:223,x:32201,y:32708|A-218-OUT,B-225-OUT;n:type:ShaderForge.SFN_Vector1,id:225,x:32237,y:32797,v1:64;n:type:ShaderForge.SFN_Add,id:228,x:32174,y:32742|A-61-RGB,B-2-RGB;n:type:ShaderForge.SFN_Tex2d,id:230,x:32189,y:32708,tex:9b5c35293e835e84eab33a6887bc527b,ntxv:0,isnm:False|UVIN-236-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:232,x:32174,y:32708,tex:9b5c35293e835e84eab33a6887bc527b,ntxv:0,isnm:False|UVIN-234-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Subtract,id:234,x:32201,y:32742,cmnt:minus minus minus minus|A-120-OUT,B-238-OUT;n:type:ShaderForge.SFN_Add,id:236,x:32222,y:32679,cmnt:plus plus plus plus|A-120-OUT,B-238-OUT;n:type:ShaderForge.SFN_Multiply,id:238,x:32174,y:32698,cmnt:Quad Offset|A-86-OUT,B-240-OUT;n:type:ShaderForge.SFN_Vector1,id:240,x:32174,y:32742,v1:4;n:type:ShaderForge.SFN_Multiply,id:241,x:32174,y:32698|A-228-OUT,B-327-OUT;n:type:ShaderForge.SFN_Multiply,id:244,x:32174,y:32742,cmnt:clamp this|A-211-OUT,B-318-OUT;n:type:ShaderForge.SFN_Vector1,id:246,x:32161,y:32783,v1:2;n:type:ShaderForge.SFN_Multiply,id:248,x:32149,y:32742|A-212-OUT,B-323-OUT;n:type:ShaderForge.SFN_Vector1,id:250,x:32174,y:32742,v1:1;n:type:ShaderForge.SFN_Tex2d,id:256,x:32201,y:32723,tex:9b5c35293e835e84eab33a6887bc527b,ntxv:0,isnm:False|UVIN-264-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:258,x:32201,y:32763,tex:9b5c35293e835e84eab33a6887bc527b,ntxv:0,isnm:False|UVIN-268-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:260,x:32149,y:32797,tex:9b5c35293e835e84eab33a6887bc527b,ntxv:0,isnm:False|UVIN-270-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:262,x:32189,y:32742,tex:9b5c35293e835e84eab33a6887bc527b,ntxv:0,isnm:False|UVIN-266-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Subtract,id:264,x:32201,y:32742,cmnt:m m m m m|A-120-OUT,B-272-OUT;n:type:ShaderForge.SFN_Subtract,id:266,x:32201,y:32763,cmnt:pp|A-120-OUT,B-274-OUT;n:type:ShaderForge.SFN_Add,id:268,x:32149,y:32723,cmnt:unused|A-120-OUT,B-272-OUT;n:type:ShaderForge.SFN_Add,id:270,x:32149,y:32742,cmnt:p p p p p p|A-120-OUT,B-274-OUT;n:type:ShaderForge.SFN_Multiply,id:272,x:32149,y:32698,cmnt:Penta Offset|A-86-OUT,B-276-OUT;n:type:ShaderForge.SFN_Multiply,id:274,x:32201,y:32698,cmnt:Hecta Offset|A-86-OUT,B-278-OUT;n:type:ShaderForge.SFN_Vector1,id:276,x:32222,y:32767,v1:5;n:type:ShaderForge.SFN_Vector1,id:278,x:32136,y:32742,v1:6;n:type:ShaderForge.SFN_Add,id:280,x:32174,y:32698,cmnt:ppppp|A-258-RGB,B-256-RGB;n:type:ShaderForge.SFN_Add,id:282,x:32161,y:32698,cmnt:pppppp|A-260-RGB,B-262-RGB;n:type:ShaderForge.SFN_Add,id:284,x:32189,y:32708|A-260-RGB,B-258-RGB,C-230-RGB,D-204-RGB,E-61-RGB;n:type:ShaderForge.SFN_RemapRange,id:302,x:32174,y:32742,frmn:0,frmx:1,tomn:0,tomx:8|IN-313-OUT;n:type:ShaderForge.SFN_Vector1,id:311,x:32161,y:32742,v1:1;n:type:ShaderForge.SFN_Vector1,id:312,x:32136,y:32797,v1:0;n:type:ShaderForge.SFN_Vector1,id:313,x:32201,y:32742,v1:0.4;n:type:ShaderForge.SFN_Vector1,id:314,x:32174,y:32742,v1:201;n:type:ShaderForge.SFN_Vector1,id:316,x:32201,y:32767,v1:52;n:type:ShaderForge.SFN_Vector1,id:317,x:32161,y:32742,v1:2;n:type:ShaderForge.SFN_Power,id:318,x:32189,y:32723|VAL-317-OUT,EXP-319-OUT;n:type:ShaderForge.SFN_Vector1,id:319,x:32161,y:32742,v1:3;n:type:ShaderForge.SFN_Vector1,id:321,x:32174,y:32679,v1:2;n:type:ShaderForge.SFN_Power,id:323,x:32201,y:32698|VAL-321-OUT,EXP-250-OUT;n:type:ShaderForge.SFN_Power,id:327,x:32222,y:32698|VAL-329-OUT,EXP-246-OUT;n:type:ShaderForge.SFN_Vector1,id:329,x:32201,y:32742,v1:2;proporder:13;pass:END;sub:END;*/

Shader "Utility Shaders/Preview/Directional Blur H" {
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
                float2 node_71 = i.uv0;
                float2 node_120 = float2(node_71.r,node_71.g);
                float2 node_94 = node_120;
                float node_84 = (0.4*8.0+0.0);
                float node_110 = lerp(0.0,clamp(node_84,0,1),1.0); // Mip Level
                float4 node_59 = tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_94, _Texture),0.0,node_110));
                float node_154 = 0.0;
                float node_174 = 3.141592654;
                float node_175 = (-1*node_174);
                float node_173 = (node_175 + ( (52.0 - node_154) * (node_174 - node_175) ) / (360.0 - node_154));
                float2 node_86 = ((float2(sin(node_173),cos(node_173))*node_84)/201.0); // Calculate Offset by Mip
                float2 node_274 = (node_86*6.0); // Hecta Offset
                float2 node_270 = (node_120+node_274); // p p p p p p
                float4 node_260 = tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_270, _Texture),0.0,node_110));
                float2 node_272 = (node_86*5.0); // Penta Offset
                float2 node_268 = (node_120+node_272); // unused
                float4 node_258 = tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_268, _Texture),0.0,node_110));
                float2 node_238 = (node_86*4.0); // Quad Offset
                float2 node_236 = (node_120+node_238); // plus plus plus plus
                float4 node_230 = tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_236, _Texture),0.0,node_110));
                float2 node_200 = (node_86*3.0); // Tripple Ofsset
                float2 node_198 = (node_120+node_200); // minus minus
                float4 node_204 = tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_198, _Texture),0.0,node_110));
                float2 node_124 = (node_86*2.0); // Double Offset
                float2 node_72 = (node_120+node_124); // plus plus
                float4 node_61 = tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_72, _Texture),0.0,node_110));
                float2 node_90 = (node_120+node_86); // plus
                float4 node_63 = tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_90, _Texture),0.0,node_110));
                float2 node_87 = (node_120-node_86); // minus
                float4 node_57 = tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_87, _Texture),0.0,node_110));
                float2 node_92 = (node_120-node_124);
                float4 node_2 = tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_92, _Texture),0.0,node_110));
                float2 node_196 = (node_120-node_200); // plus plus
                float2 node_234 = (node_120-node_238); // minus minus minus minus
                float4 node_232 = tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_234, _Texture),0.0,node_110));
                float2 node_264 = (node_120-node_272); // m m m m m
                float4 node_256 = tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_264, _Texture),0.0,node_110));
                float2 node_266 = (node_120-node_274); // pp
                float4 node_262 = tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_266, _Texture),0.0,node_110));
                float3 emissive = lerp(node_59.rgb,lerp(((((node_260.rgb+node_258.rgb+node_230.rgb+node_204.rgb+node_61.rgb)+node_63.rgb+node_59.rgb+node_57.rgb+node_2.rgb)+tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_196, _Texture),0.0,node_110)).rgb+node_232.rgb+node_256.rgb+node_262.rgb)/13.0),(((node_230.rgb+node_232.rgb)+((node_204.rgb+node_204.rgb)*pow(2.0,1.0))+((node_61.rgb+node_2.rgb)*pow(2.0,2.0))+((node_63.rgb+node_57.rgb)*pow(2.0,3.0))+(node_59.rgb*32.0))/64.0),0.0),saturate(node_84));
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
