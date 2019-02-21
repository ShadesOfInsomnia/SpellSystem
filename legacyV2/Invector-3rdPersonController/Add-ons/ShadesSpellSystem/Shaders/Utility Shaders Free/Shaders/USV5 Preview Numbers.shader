// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge Beta 0.35 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.35;sub:START;pass:START;ps:flbk:,lico:0,lgpr:1,nrmq:1,limd:0,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:0,bsrc:0,bdst:0,culm:0,dpts:2,wrdp:True,ufog:True,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.945098,fgcg:0.9137255,fgcb:0.8470588,fgca:1,fgde:0.02,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:34172,y:33373|emission-296-OUT,clip-297-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:2,x:34249,y:33507,ptlb:Numbers,ptin:_Numbers,glob:False;n:type:ShaderForge.SFN_Tex2d,id:3,x:34249,y:33507,ntxv:0,isnm:False|UVIN-16-OUT,TEX-2-TEX;n:type:ShaderForge.SFN_Multiply,id:5,x:34221,y:33507|A-22-V,B-47-OUT;n:type:ShaderForge.SFN_Multiply,id:11,x:34270,y:33507|A-18-OUT,B-42-OUT;n:type:ShaderForge.SFN_Append,id:16,x:34232,y:33507|A-36-OUT,B-51-OUT;n:type:ShaderForge.SFN_Multiply,id:18,x:34194,y:33536|A-24-OUT,B-22-U;n:type:ShaderForge.SFN_TexCoord,id:22,x:34249,y:33507,uv:0;n:type:ShaderForge.SFN_Vector1,id:24,x:34232,y:33608,v1:4;n:type:ShaderForge.SFN_Add,id:36,x:34249,y:33486|A-39-OUT,B-11-OUT;n:type:ShaderForge.SFN_Multiply,id:39,x:34249,y:33530|A-52-OUT,B-42-OUT;n:type:ShaderForge.SFN_Divide,id:42,x:34270,y:33507|A-43-OUT,B-291-OUT;n:type:ShaderForge.SFN_Vector1,id:43,x:34249,y:33541,v1:1;n:type:ShaderForge.SFN_Vector1,id:45,x:34270,y:33564,v1:1;n:type:ShaderForge.SFN_Divide,id:47,x:34249,y:33507|A-45-OUT,B-293-OUT;n:type:ShaderForge.SFN_Multiply,id:49,x:34249,y:33551|A-47-OUT,B-79-OUT;n:type:ShaderForge.SFN_Add,id:51,x:34207,y:33507|A-5-OUT,B-49-OUT;n:type:ShaderForge.SFN_Relay,id:52,x:34278,y:33551,cmnt:Columns across?|IN-80-OUT;n:type:ShaderForge.SFN_Relay,id:53,x:34249,y:33551,cmnt:Rows Down|IN-78-OUT;n:type:ShaderForge.SFN_Subtract,id:57,x:34249,y:33507|A-66-R,B-58-OUT;n:type:ShaderForge.SFN_Vector1,id:58,x:34304,y:33642,v1:1;n:type:ShaderForge.SFN_ComponentMask,id:66,x:34207,y:33554,cc1:0,cc2:1,cc3:2,cc4:3|IN-289-OUT;n:type:ShaderForge.SFN_Vector1,id:77,x:34139,y:33519,v1:4;n:type:ShaderForge.SFN_Step,id:78,x:34221,y:33486,cmnt:Rows Down|A-77-OUT,B-57-OUT;n:type:ShaderForge.SFN_OneMinus,id:79,x:34207,y:33507|IN-53-OUT;n:type:ShaderForge.SFN_Floor,id:80,x:34194,y:33486,cmnt:Columns Across|IN-66-R;n:type:ShaderForge.SFN_Lerp,id:82,x:34256,y:33507|A-218-OUT,B-220-OUT,T-106-OUT;n:type:ShaderForge.SFN_TexCoord,id:85,x:34260,y:33530,uv:0;n:type:ShaderForge.SFN_Step,id:106,x:34232,y:33530|A-111-OUT,B-85-U;n:type:ShaderForge.SFN_Step,id:108,x:34201,y:33503|A-113-OUT,B-85-U;n:type:ShaderForge.SFN_Step,id:110,x:34232,y:33561|A-115-OUT,B-85-U;n:type:ShaderForge.SFN_Vector1,id:111,x:34249,y:33507,v1:0.25;n:type:ShaderForge.SFN_Vector1,id:113,x:34249,y:33571,v1:0.5;n:type:ShaderForge.SFN_Vector1,id:115,x:34249,y:33637,v1:0.75;n:type:ShaderForge.SFN_Lerp,id:117,x:34207,y:33507|A-82-OUT,B-222-OUT,T-108-OUT;n:type:ShaderForge.SFN_Lerp,id:119,x:34224,y:33454|A-117-OUT,B-224-OUT,T-110-OUT;n:type:ShaderForge.SFN_Tex2d,id:121,x:34221,y:33507,ntxv:0,isnm:False|UVIN-191-OUT,TEX-2-TEX;n:type:ShaderForge.SFN_Subtract,id:123,x:34221,y:33507|A-66-G,B-125-OUT;n:type:ShaderForge.SFN_Vector1,id:125,x:34324,y:33551,v1:0;n:type:ShaderForge.SFN_Vector1,id:127,x:34304,y:33523,v1:4;n:type:ShaderForge.SFN_Step,id:129,x:34260,y:33507,cmnt:Rows Down|A-127-OUT,B-123-OUT;n:type:ShaderForge.SFN_Floor,id:131,x:34260,y:33507,cmnt:Columns Across|IN-66-G;n:type:ShaderForge.SFN_Subtract,id:133,x:34207,y:33479|A-66-B,B-135-OUT;n:type:ShaderForge.SFN_Vector1,id:135,x:34342,y:33629,v1:-1;n:type:ShaderForge.SFN_Vector1,id:137,x:34180,y:33470,v1:4;n:type:ShaderForge.SFN_Step,id:139,x:34232,y:33486,cmnt:Rows Down|A-137-OUT,B-133-OUT;n:type:ShaderForge.SFN_Floor,id:141,x:34214,y:33507,cmnt:Columns Across|IN-66-B;n:type:ShaderForge.SFN_Subtract,id:143,x:34232,y:33530|A-66-A,B-145-OUT;n:type:ShaderForge.SFN_Vector1,id:145,x:34394,y:33605,v1:-2;n:type:ShaderForge.SFN_Vector1,id:147,x:34232,y:33436,v1:4;n:type:ShaderForge.SFN_Step,id:149,x:34249,y:33507,cmnt:Rows Down|A-147-OUT,B-143-OUT;n:type:ShaderForge.SFN_Floor,id:151,x:34221,y:33474,cmnt:Columns Across|IN-66-A;n:type:ShaderForge.SFN_Multiply,id:187,x:34207,y:33507|A-22-V,B-207-OUT;n:type:ShaderForge.SFN_Multiply,id:189,x:34207,y:33507|A-18-OUT,B-201-OUT;n:type:ShaderForge.SFN_Append,id:191,x:34207,y:33536|A-193-OUT,B-211-OUT;n:type:ShaderForge.SFN_Add,id:193,x:34207,y:33530|A-195-OUT,B-189-OUT;n:type:ShaderForge.SFN_Multiply,id:195,x:34232,y:33507|A-213-OUT,B-201-OUT;n:type:ShaderForge.SFN_Divide,id:201,x:34207,y:33479|A-203-OUT,B-291-OUT;n:type:ShaderForge.SFN_Vector1,id:203,x:34249,y:33523,v1:1;n:type:ShaderForge.SFN_Vector1,id:205,x:34278,y:33523,v1:1;n:type:ShaderForge.SFN_Divide,id:207,x:34232,y:33507|A-205-OUT,B-293-OUT;n:type:ShaderForge.SFN_Multiply,id:209,x:34232,y:33541|A-207-OUT,B-217-OUT;n:type:ShaderForge.SFN_Add,id:211,x:34262,y:33530|A-187-OUT,B-209-OUT;n:type:ShaderForge.SFN_Relay,id:213,x:34262,y:33541,cmnt:Columns across?|IN-131-OUT;n:type:ShaderForge.SFN_Relay,id:215,x:34278,y:33598,cmnt:Rows Down|IN-129-OUT;n:type:ShaderForge.SFN_OneMinus,id:217,x:34278,y:33530|IN-215-OUT;n:type:ShaderForge.SFN_Relay,id:218,x:34210,y:33523,cmnt:1st Digit|IN-3-A;n:type:ShaderForge.SFN_Relay,id:220,x:34147,y:33521,cmnt:2nd Digit|IN-121-A;n:type:ShaderForge.SFN_Relay,id:222,x:34147,y:33529,cmnt:3rd Digit|IN-254-A;n:type:ShaderForge.SFN_Relay,id:224,x:34147,y:33541,cmnt:4th Digit|IN-256-A;n:type:ShaderForge.SFN_Multiply,id:226,x:34214,y:33489|A-22-V,B-242-OUT;n:type:ShaderForge.SFN_Multiply,id:228,x:34214,y:33507|A-18-OUT,B-236-OUT;n:type:ShaderForge.SFN_Append,id:230,x:34214,y:33507|A-232-OUT,B-246-OUT;n:type:ShaderForge.SFN_Add,id:232,x:34232,y:33496|A-234-OUT,B-228-OUT;n:type:ShaderForge.SFN_Multiply,id:234,x:34249,y:33486|A-248-OUT,B-236-OUT;n:type:ShaderForge.SFN_Divide,id:236,x:34249,y:33507|A-238-OUT,B-291-OUT;n:type:ShaderForge.SFN_Vector1,id:238,x:34207,y:33564,v1:1;n:type:ShaderForge.SFN_Vector1,id:240,x:34207,y:33564,v1:1;n:type:ShaderForge.SFN_Divide,id:242,x:34207,y:33457|A-240-OUT,B-293-OUT;n:type:ShaderForge.SFN_Multiply,id:244,x:34232,y:33497|A-242-OUT,B-252-OUT;n:type:ShaderForge.SFN_Add,id:246,x:34249,y:33507|A-226-OUT,B-244-OUT;n:type:ShaderForge.SFN_Relay,id:248,x:34278,y:33642,cmnt:Columns across?|IN-141-OUT;n:type:ShaderForge.SFN_Relay,id:250,x:34249,y:33642,cmnt:Rows Down|IN-139-OUT;n:type:ShaderForge.SFN_OneMinus,id:252,x:34249,y:33530|IN-250-OUT;n:type:ShaderForge.SFN_Tex2d,id:254,x:34180,y:33574,ntxv:0,isnm:False|UVIN-230-OUT,TEX-2-TEX;n:type:ShaderForge.SFN_Tex2d,id:256,x:34194,y:33517,ntxv:0,isnm:False|UVIN-262-OUT,TEX-2-TEX;n:type:ShaderForge.SFN_Multiply,id:258,x:34207,y:33474|A-22-V,B-274-OUT;n:type:ShaderForge.SFN_Multiply,id:260,x:34249,y:33517|A-18-OUT,B-268-OUT;n:type:ShaderForge.SFN_Append,id:262,x:34249,y:33530|A-264-OUT,B-278-OUT;n:type:ShaderForge.SFN_Add,id:264,x:34282,y:33452|A-266-OUT,B-260-OUT;n:type:ShaderForge.SFN_Multiply,id:266,x:34194,y:33530|A-280-OUT,B-268-OUT;n:type:ShaderForge.SFN_Divide,id:268,x:34232,y:33598|A-270-OUT,B-291-OUT;n:type:ShaderForge.SFN_Vector1,id:270,x:34207,y:33523,v1:1;n:type:ShaderForge.SFN_Vector1,id:272,x:34249,y:33551,v1:1;n:type:ShaderForge.SFN_Divide,id:274,x:34221,y:33486|A-272-OUT,B-293-OUT;n:type:ShaderForge.SFN_Multiply,id:276,x:34207,y:33530|A-274-OUT,B-284-OUT;n:type:ShaderForge.SFN_Add,id:278,x:34324,y:33608|A-258-OUT,B-276-OUT;n:type:ShaderForge.SFN_Relay,id:280,x:34249,y:33564,cmnt:Columns across?|IN-151-OUT;n:type:ShaderForge.SFN_Relay,id:282,x:34278,y:33598,cmnt:Rows Down|IN-149-OUT;n:type:ShaderForge.SFN_OneMinus,id:284,x:34180,y:33507|IN-282-OUT;n:type:ShaderForge.SFN_Vector4,id:288,x:34249,y:33598,v1:0,v2:1,v3:2,v4:3;n:type:ShaderForge.SFN_Subtract,id:289,x:34249,y:33507|A-300-OUT,B-288-OUT;n:type:ShaderForge.SFN_Vector1,id:291,x:34249,y:33486,v1:5;n:type:ShaderForge.SFN_Vector1,id:293,x:34207,y:33598,v1:2;n:type:ShaderForge.SFN_Lerp,id:296,x:34207,y:33454|A-301-OUT,B-303-OUT,T-119-OUT;n:type:ShaderForge.SFN_Lerp,id:297,x:34207,y:33463|A-298-OUT,B-119-OUT,T-305-OUT;n:type:ShaderForge.SFN_Vector1,id:298,x:34207,y:33507,v1:1;n:type:ShaderForge.SFN_Vector4,id:300,x:34232,y:33507,v1:1,v2:2,v3:3,v4:4;n:type:ShaderForge.SFN_Vector3,id:301,x:34224,y:33507,v1:0.08153111,v2:0.08395476,v3:0.09558821;n:type:ShaderForge.SFN_Vector3,id:303,x:34207,y:33507,v1:1,v2:0.2275862,v3:0;n:type:ShaderForge.SFN_Time,id:304,x:34256,y:33489;n:type:ShaderForge.SFN_Step,id:305,x:34196,y:33479|A-308-OUT,B-307-OUT;n:type:ShaderForge.SFN_Vector1,id:307,x:34207,y:33541,v1:0.5;n:type:ShaderForge.SFN_Frac,id:308,x:34239,y:33463|IN-309-OUT;n:type:ShaderForge.SFN_Divide,id:309,x:34256,y:33507|A-304-T,B-311-OUT;n:type:ShaderForge.SFN_Vector1,id:311,x:34239,y:33541,v1:4;proporder:2;pass:END;sub:END;*/

Shader "Utility Shaders/Preview/Numbers" {
    Properties {
        _Numbers ("Numbers", 2D) = "white" {}
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
            uniform float4 _TimeEditor;
            uniform sampler2D _Numbers; uniform float4 _Numbers_ST;
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
                float4 node_66 = (float4(1,2,3,4)-float4(0,1,2,3)).rgba;
                float node_291 = 5.0;
                float node_42 = (1.0/node_291);
                float2 node_22 = i.uv0;
                float node_18 = (4.0*node_22.r);
                float node_293 = 2.0;
                float node_47 = (1.0/node_293);
                float2 node_16 = float2(((floor(node_66.r)*node_42)+(node_18*node_42)),((node_22.g*node_47)+(node_47*(1.0 - step(4.0,(node_66.r-1.0))))));
                float node_201 = (1.0/node_291);
                float node_207 = (1.0/node_293);
                float2 node_191 = float2(((floor(node_66.g)*node_201)+(node_18*node_201)),((node_22.g*node_207)+(node_207*(1.0 - step(4.0,(node_66.g-0.0))))));
                float2 node_85 = i.uv0;
                float node_236 = (1.0/node_291);
                float node_242 = (1.0/node_293);
                float2 node_230 = float2(((floor(node_66.b)*node_236)+(node_18*node_236)),((node_22.g*node_242)+(node_242*(1.0 - step(4.0,(node_66.b-(-1.0)))))));
                float node_268 = (1.0/node_291);
                float node_274 = (1.0/node_293);
                float2 node_262 = float2(((floor(node_66.a)*node_268)+(node_18*node_268)),((node_22.g*node_274)+(node_274*(1.0 - step(4.0,(node_66.a-(-2.0)))))));
                float node_119 = lerp(lerp(lerp(tex2D(_Numbers,TRANSFORM_TEX(node_16, _Numbers)).a,tex2D(_Numbers,TRANSFORM_TEX(node_191, _Numbers)).a,step(0.25,node_85.r)),tex2D(_Numbers,TRANSFORM_TEX(node_230, _Numbers)).a,step(0.5,node_85.r)),tex2D(_Numbers,TRANSFORM_TEX(node_262, _Numbers)).a,step(0.75,node_85.r));
                float4 node_304 = _Time + _TimeEditor;
                clip(lerp(1.0,node_119,step(frac((node_304.g/4.0)),0.5)) - 0.5);
////// Lighting:
////// Emissive:
                float3 emissive = lerp(float3(0.08153111,0.08395476,0.09558821),float3(1,0.2275862,0),node_119);
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
            uniform float4 _TimeEditor;
            uniform sampler2D _Numbers; uniform float4 _Numbers_ST;
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
                float4 node_66 = (float4(1,2,3,4)-float4(0,1,2,3)).rgba;
                float node_291 = 5.0;
                float node_42 = (1.0/node_291);
                float2 node_22 = i.uv0;
                float node_18 = (4.0*node_22.r);
                float node_293 = 2.0;
                float node_47 = (1.0/node_293);
                float2 node_16 = float2(((floor(node_66.r)*node_42)+(node_18*node_42)),((node_22.g*node_47)+(node_47*(1.0 - step(4.0,(node_66.r-1.0))))));
                float node_201 = (1.0/node_291);
                float node_207 = (1.0/node_293);
                float2 node_191 = float2(((floor(node_66.g)*node_201)+(node_18*node_201)),((node_22.g*node_207)+(node_207*(1.0 - step(4.0,(node_66.g-0.0))))));
                float2 node_85 = i.uv0;
                float node_236 = (1.0/node_291);
                float node_242 = (1.0/node_293);
                float2 node_230 = float2(((floor(node_66.b)*node_236)+(node_18*node_236)),((node_22.g*node_242)+(node_242*(1.0 - step(4.0,(node_66.b-(-1.0)))))));
                float node_268 = (1.0/node_291);
                float node_274 = (1.0/node_293);
                float2 node_262 = float2(((floor(node_66.a)*node_268)+(node_18*node_268)),((node_22.g*node_274)+(node_274*(1.0 - step(4.0,(node_66.a-(-2.0)))))));
                float node_119 = lerp(lerp(lerp(tex2D(_Numbers,TRANSFORM_TEX(node_16, _Numbers)).a,tex2D(_Numbers,TRANSFORM_TEX(node_191, _Numbers)).a,step(0.25,node_85.r)),tex2D(_Numbers,TRANSFORM_TEX(node_230, _Numbers)).a,step(0.5,node_85.r)),tex2D(_Numbers,TRANSFORM_TEX(node_262, _Numbers)).a,step(0.75,node_85.r));
                float4 node_304 = _Time + _TimeEditor;
                clip(lerp(1.0,node_119,step(frac((node_304.g/4.0)),0.5)) - 0.5);
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
            uniform float4 _TimeEditor;
            uniform sampler2D _Numbers; uniform float4 _Numbers_ST;
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
                float4 node_66 = (float4(1,2,3,4)-float4(0,1,2,3)).rgba;
                float node_291 = 5.0;
                float node_42 = (1.0/node_291);
                float2 node_22 = i.uv0;
                float node_18 = (4.0*node_22.r);
                float node_293 = 2.0;
                float node_47 = (1.0/node_293);
                float2 node_16 = float2(((floor(node_66.r)*node_42)+(node_18*node_42)),((node_22.g*node_47)+(node_47*(1.0 - step(4.0,(node_66.r-1.0))))));
                float node_201 = (1.0/node_291);
                float node_207 = (1.0/node_293);
                float2 node_191 = float2(((floor(node_66.g)*node_201)+(node_18*node_201)),((node_22.g*node_207)+(node_207*(1.0 - step(4.0,(node_66.g-0.0))))));
                float2 node_85 = i.uv0;
                float node_236 = (1.0/node_291);
                float node_242 = (1.0/node_293);
                float2 node_230 = float2(((floor(node_66.b)*node_236)+(node_18*node_236)),((node_22.g*node_242)+(node_242*(1.0 - step(4.0,(node_66.b-(-1.0)))))));
                float node_268 = (1.0/node_291);
                float node_274 = (1.0/node_293);
                float2 node_262 = float2(((floor(node_66.a)*node_268)+(node_18*node_268)),((node_22.g*node_274)+(node_274*(1.0 - step(4.0,(node_66.a-(-2.0)))))));
                float node_119 = lerp(lerp(lerp(tex2D(_Numbers,TRANSFORM_TEX(node_16, _Numbers)).a,tex2D(_Numbers,TRANSFORM_TEX(node_191, _Numbers)).a,step(0.25,node_85.r)),tex2D(_Numbers,TRANSFORM_TEX(node_230, _Numbers)).a,step(0.5,node_85.r)),tex2D(_Numbers,TRANSFORM_TEX(node_262, _Numbers)).a,step(0.75,node_85.r));
                float4 node_304 = _Time + _TimeEditor;
                clip(lerp(1.0,node_119,step(frac((node_304.g/4.0)),0.5)) - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
