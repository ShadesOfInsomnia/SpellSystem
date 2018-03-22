// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge Beta 0.35 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.35;sub:START;pass:START;ps:flbk:Unlit/Texture,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:True,ufog:True,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.945098,fgcg:0.9137255,fgcb:0.8470588,fgca:1,fgde:0.02,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:31505,y:32966|emission-194-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:31989,y:32963,ntxv:0,isnm:False|UVIN-100-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:13,x:32186,y:32946,ptlb:Texture,ptin:_Texture,glob:False;n:type:ShaderForge.SFN_Tex2d,id:57,x:32100,y:32800,ntxv:0,isnm:False|UVIN-98-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:59,x:32289,y:32813,ntxv:0,isnm:False|UVIN-94-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:61,x:32305,y:32897,ntxv:0,isnm:False|UVIN-96-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:63,x:32047,y:32876,ntxv:0,isnm:False|UVIN-93-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_TexCoord,id:71,x:32249,y:32994,uv:0;n:type:ShaderForge.SFN_Add,id:72,x:32186,y:33004|A-71-V,B-86-OUT;n:type:ShaderForge.SFN_Vector1,id:73,x:31866,y:32978,v1:2;n:type:ShaderForge.SFN_Power,id:74,x:32104,y:32934,cmnt:Pixel Defibrilator|VAL-73-OUT,EXP-85-OUT;n:type:ShaderForge.SFN_Relay,id:84,x:31989,y:32910|IN-185-OUT;n:type:ShaderForge.SFN_Relay,id:85,x:32757,y:32807|IN-84-OUT;n:type:ShaderForge.SFN_Divide,id:86,x:31889,y:33116,cmnt:Blur Amount|A-74-OUT,B-211-OUT;n:type:ShaderForge.SFN_Subtract,id:87,x:32189,y:33057|A-71-U,B-86-OUT;n:type:ShaderForge.SFN_Add,id:90,x:32343,y:33114|A-71-U,B-86-OUT;n:type:ShaderForge.SFN_Subtract,id:92,x:32222,y:32994|A-71-V,B-86-OUT;n:type:ShaderForge.SFN_Append,id:93,x:31971,y:33145|A-105-OUT,B-72-OUT;n:type:ShaderForge.SFN_Relay,id:94,x:32357,y:33114|IN-71-UVOUT;n:type:ShaderForge.SFN_Append,id:96,x:32371,y:32844|A-105-OUT,B-92-OUT;n:type:ShaderForge.SFN_Append,id:98,x:32162,y:32771|A-90-OUT,B-106-OUT;n:type:ShaderForge.SFN_Append,id:100,x:32393,y:32994|A-87-OUT,B-106-OUT;n:type:ShaderForge.SFN_Relay,id:105,x:32018,y:33048|IN-71-U;n:type:ShaderForge.SFN_Relay,id:106,x:32440,y:33091,cmnt:V for vendetta|IN-71-V;n:type:ShaderForge.SFN_Lerp,id:107,x:31819,y:33061|A-59-RGB,B-176-OUT,T-108-OUT;n:type:ShaderForge.SFN_Clamp01,id:108,x:32211,y:33073|IN-185-OUT;n:type:ShaderForge.SFN_ConstantClamp,id:109,x:32410,y:33073,min:0,max:5|IN-84-OUT;n:type:ShaderForge.SFN_Relay,id:110,x:32855,y:32978,cmnt:texture Offset|IN-207-OUT;n:type:ShaderForge.SFN_Tex2d,id:112,x:32354,y:32946,ntxv:0,isnm:False|UVIN-124-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:114,x:32329,y:33103,ntxv:0,isnm:False|UVIN-126-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:116,x:32249,y:33116,ntxv:0,isnm:False|UVIN-120-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:118,x:32103,y:33116,ntxv:0,isnm:False|UVIN-122-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Append,id:120,x:32354,y:33137|A-87-OUT,B-72-OUT;n:type:ShaderForge.SFN_Append,id:122,x:32158,y:33161|A-90-OUT,B-72-OUT;n:type:ShaderForge.SFN_Append,id:124,x:31989,y:32840|A-90-OUT,B-92-OUT;n:type:ShaderForge.SFN_Append,id:126,x:31933,y:33015|A-87-OUT,B-92-OUT;n:type:ShaderForge.SFN_Add,id:128,x:32365,y:33047,cmnt:Medium|A-63-RGB,B-61-RGB,C-57-RGB,D-2-RGB;n:type:ShaderForge.SFN_Add,id:130,x:32189,y:33114,cmnt:fancy|A-118-RGB,B-116-RGB,C-112-RGB,D-114-RGB;n:type:ShaderForge.SFN_Relay,id:131,x:32071,y:33073,cmnt:Outer pixels|IN-59-RGB;n:type:ShaderForge.SFN_Multiply,id:132,x:32086,y:33114|A-140-OUT,B-130-OUT;n:type:ShaderForge.SFN_Multiply,id:133,x:32167,y:32965|A-138-OUT,B-128-OUT;n:type:ShaderForge.SFN_Multiply,id:134,x:32167,y:33073|A-136-OUT,B-131-OUT;n:type:ShaderForge.SFN_Add,id:135,x:32431,y:32924|A-174-OUT,B-132-OUT,C-133-OUT,D-134-OUT;n:type:ShaderForge.SFN_Vector1,id:136,x:32410,y:33009,v1:0.4;n:type:ShaderForge.SFN_Vector1,id:138,x:32252,y:33128,v1:0.075;n:type:ShaderForge.SFN_Vector1,id:140,x:32042,y:33019,v1:0.05;n:type:ShaderForge.SFN_Add,id:142,x:32104,y:32955,cmnt:Low Quality|A-150-RGB,B-148-RGB,C-152-RGB,D-154-RGB;n:type:ShaderForge.SFN_Multiply,id:143,x:31866,y:33014,cmnt:Sharp Amount|A-86-OUT,B-184-OUT;n:type:ShaderForge.SFN_Tex2d,id:148,x:32066,y:33015,ntxv:0,isnm:False|UVIN-164-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:150,x:32305,y:33015,ntxv:0,isnm:False|UVIN-166-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:152,x:32186,y:33219,ntxv:0,isnm:False|UVIN-168-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Tex2d,id:154,x:32012,y:33116,ntxv:0,isnm:False|UVIN-170-OUT,MIP-110-OUT,TEX-13-TEX;n:type:ShaderForge.SFN_Subtract,id:156,x:32167,y:32975|A-71-U,B-143-OUT;n:type:ShaderForge.SFN_Add,id:158,x:32071,y:33058|A-71-U,B-143-OUT;n:type:ShaderForge.SFN_Add,id:160,x:32305,y:33015|A-71-V,B-143-OUT;n:type:ShaderForge.SFN_Subtract,id:162,x:32305,y:33161|A-71-V,B-143-OUT;n:type:ShaderForge.SFN_Append,id:164,x:32139,y:33041|A-156-OUT,B-106-OUT;n:type:ShaderForge.SFN_Append,id:166,x:32162,y:32863|A-158-OUT,B-106-OUT;n:type:ShaderForge.SFN_Append,id:168,x:32249,y:33196|A-105-OUT,B-160-OUT;n:type:ShaderForge.SFN_Append,id:170,x:32042,y:33196|A-105-OUT,B-162-OUT;n:type:ShaderForge.SFN_Vector1,id:172,x:32223,y:33019,v1:0.025;n:type:ShaderForge.SFN_Multiply,id:174,x:32270,y:33114|A-172-OUT,B-142-OUT;n:type:ShaderForge.SFN_Lerp,id:176,x:32384,y:33047|A-182-OUT,B-135-OUT,T-188-OUT;n:type:ShaderForge.SFN_Add,id:177,x:32328,y:32934|A-142-OUT,B-130-OUT,C-128-OUT,D-131-OUT;n:type:ShaderForge.SFN_Divide,id:182,x:32189,y:32965|A-177-OUT,B-183-OUT;n:type:ShaderForge.SFN_Vector1,id:183,x:32328,y:33088,v1:13;n:type:ShaderForge.SFN_Vector1,id:184,x:32410,y:32924,v1:1.5;n:type:ShaderForge.SFN_RemapRange,id:185,x:31922,y:33161,frmn:0,frmx:1,tomn:0,tomx:6|IN-187-OUT;n:type:ShaderForge.SFN_Vector1,id:187,x:32410,y:33196,v1:0.3;n:type:ShaderForge.SFN_Vector1,id:188,x:32328,y:33091,v1:0;n:type:ShaderForge.SFN_TexCoord,id:189,x:32452,y:33081,uv:0;n:type:ShaderForge.SFN_Step,id:190,x:31903,y:32910|A-189-V,B-191-OUT;n:type:ShaderForge.SFN_Vector1,id:191,x:32305,y:33253,v1:-1;n:type:ShaderForge.SFN_Sin,id:192,x:31866,y:33145|IN-190-OUT;n:type:ShaderForge.SFN_Relay,id:193,x:31852,y:33058|IN-206-OUT;n:type:ShaderForge.SFN_Add,id:194,x:31819,y:33065|A-107-OUT,B-196-OUT;n:type:ShaderForge.SFN_ConstantClamp,id:196,x:31819,y:33058,min:0,max:0|IN-193-OUT;n:type:ShaderForge.SFN_ArcTan2,id:197,x:32071,y:32922|A-192-OUT,B-192-OUT;n:type:ShaderForge.SFN_Tau,id:198,x:32884,y:32978;n:type:ShaderForge.SFN_Subtract,id:199,x:32104,y:33019,cmnt:Smoothness Optimization|A-197-OUT,B-198-OUT;n:type:ShaderForge.SFN_Floor,id:200,x:32343,y:32965|IN-199-OUT;n:type:ShaderForge.SFN_Sqrt,id:201,x:32287,y:33057|IN-200-OUT;n:type:ShaderForge.SFN_Reflect,id:202,x:32253,y:33091|A-201-OUT,B-204-OUT;n:type:ShaderForge.SFN_Transform,id:203,x:32162,y:33019,cmnt:Offset Direction,tffrom:1,tfto:2|IN-205-OUT;n:type:ShaderForge.SFN_VectorProjection,id:204,x:32104,y:33091|A-199-OUT,B-203-XYZ;n:type:ShaderForge.SFN_Append,id:205,x:32115,y:33206|A-189-UVOUT,B-189-U;n:type:ShaderForge.SFN_Relay,id:206,x:32253,y:33196|IN-202-OUT;n:type:ShaderForge.SFN_Add,id:207,x:32328,y:32934|A-209-OUT,B-109-OUT;n:type:ShaderForge.SFN_ComponentMask,id:208,x:32343,y:32994,cc1:0,cc2:-1,cc3:-1,cc4:-1|IN-202-OUT;n:type:ShaderForge.SFN_ConstantClamp,id:209,x:32354,y:32924,min:0,max:0|IN-208-OUT;n:type:ShaderForge.SFN_Vector1,id:210,x:32086,y:33041,v1:2;n:type:ShaderForge.SFN_Power,id:211,x:32012,y:33196|VAL-210-OUT,EXP-213-OUT;n:type:ShaderForge.SFN_Vector1,id:213,x:32287,y:33196,v1:8;proporder:13;pass:END;sub:END;*/

Shader "Utility Shaders/Preview/Blur H Preview" {
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
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                float3 tangentDir : TEXCOORD2;
                float3 binormalDir : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(float4(v.normal,0), unity_WorldToObject).xyz;
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
/////// Normals:
                float3 normalDirection =  i.normalDir;
////// Lighting:
////// Emissive:
                float2 node_71 = i.uv0;
                float2 node_94 = node_71.rg;
                float2 node_189 = i.uv0;
                float node_192 = sin(step(node_189.g,(-1.0)));
                float node_199 = (atan2(node_192,node_192)-6.28318530718); // Smoothness Optimization
                float3 node_203 = mul( tangentTransform, mul( unity_ObjectToWorld, float4(float3(node_189.rg,node_189.r),0) ).xyz ).xyz; // Offset Direction
                float3 node_202 = reflect(sqrt(floor(node_199)),(node_203.rgb * dot(node_199,node_203.rgb)/dot(node_203.rgb,node_203.rgb)));
                float node_185 = (0.3*6.0+0.0);
                float node_84 = node_185;
                float node_110 = (clamp(node_202.r,0,0)+clamp(node_84,0,5)); // texture Offset
                float4 node_59 = tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_94, _Texture),0.0,node_110));
                float node_86 = (pow(2.0,node_84)/pow(2.0,8.0)); // Blur Amount
                float node_143 = (node_86*1.5); // Sharp Amount
                float node_106 = node_71.g; // V for vendetta
                float2 node_166 = float2((node_71.r+node_143),node_106);
                float2 node_164 = float2((node_71.r-node_143),node_106);
                float node_105 = node_71.r;
                float2 node_168 = float2(node_105,(node_71.g+node_143));
                float2 node_170 = float2(node_105,(node_71.g-node_143));
                float3 node_142 = (tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_166, _Texture),0.0,node_110)).rgb+tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_164, _Texture),0.0,node_110)).rgb+tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_168, _Texture),0.0,node_110)).rgb+tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_170, _Texture),0.0,node_110)).rgb); // Low Quality
                float node_90 = (node_71.r+node_86);
                float node_72 = (node_71.g+node_86);
                float2 node_122 = float2(node_90,node_72);
                float node_87 = (node_71.r-node_86);
                float2 node_120 = float2(node_87,node_72);
                float node_92 = (node_71.g-node_86);
                float2 node_124 = float2(node_90,node_92);
                float2 node_126 = float2(node_87,node_92);
                float3 node_130 = (tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_122, _Texture),0.0,node_110)).rgb+tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_120, _Texture),0.0,node_110)).rgb+tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_124, _Texture),0.0,node_110)).rgb+tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_126, _Texture),0.0,node_110)).rgb); // fancy
                float2 node_93 = float2(node_105,node_72);
                float2 node_96 = float2(node_105,node_92);
                float2 node_98 = float2(node_90,node_106);
                float2 node_100 = float2(node_87,node_106);
                float3 node_128 = (tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_93, _Texture),0.0,node_110)).rgb+tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_96, _Texture),0.0,node_110)).rgb+tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_98, _Texture),0.0,node_110)).rgb+tex2Dlod(_Texture,float4(TRANSFORM_TEX(node_100, _Texture),0.0,node_110)).rgb); // Medium
                float3 node_131 = node_59.rgb; // Outer pixels
                float3 emissive = (lerp(node_59.rgb,lerp(((node_142+node_130+node_128+node_131)/13.0),((0.025*node_142)+(0.05*node_130)+(0.075*node_128)+(0.4*node_131)),0.0),saturate(node_185))+clamp(node_202,0,0));
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
