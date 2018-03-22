// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge Beta 0.35 
// Shader Forge (c) Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:0.35;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:1,uamb:True,mssp:True,lmpd:False,lprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:1,bsrc:3,bdst:7,culm:0,dpts:2,wrdp:False,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.945098,fgcg:0.9137255,fgcb:0.8470588,fgca:1,fgde:0.02,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1,x:33123,y:32222|diff-48-OUT,emission-55-OUT;n:type:ShaderForge.SFN_Tex2d,id:2,x:33189,y:32318,tex:18ab3ff8accb2a543a7ad02f93204165,ntxv:0,isnm:False|UVIN-70-OUT,TEX-3-TEX;n:type:ShaderForge.SFN_Tex2dAsset,id:3,x:33157,y:32318,ptlb:Main Texture,ptin:_MainTexture,glob:False,tex:18ab3ff8accb2a543a7ad02f93204165;n:type:ShaderForge.SFN_TexCoord,id:5,x:33130,y:32310,uv:0;n:type:ShaderForge.SFN_Distance,id:6,x:33177,y:32306|A-5-UVOUT,B-7-OUT;n:type:ShaderForge.SFN_Vector2,id:7,x:33156,y:32338,v1:0.5,v2:0.5;n:type:ShaderForge.SFN_RemapRange,id:8,x:33156,y:32288,frmn:0.5,frmx:0,tomn:0,tomx:1|IN-6-OUT;n:type:ShaderForge.SFN_Power,id:19,x:33156,y:32293|VAL-8-OUT,EXP-20-OUT;n:type:ShaderForge.SFN_Vector1,id:20,x:33169,y:32442,v1:0.5;n:type:ShaderForge.SFN_Normalize,id:21,x:33141,y:32306|IN-22-OUT;n:type:ShaderForge.SFN_Subtract,id:22,x:33138,y:32311|A-25-OUT,B-26-OUT;n:type:ShaderForge.SFN_TexCoord,id:23,x:33177,y:32328,uv:0;n:type:ShaderForge.SFN_RemapRange,id:25,x:33141,y:32293,frmn:0,frmx:1,tomn:-1,tomx:1|IN-23-UVOUT;n:type:ShaderForge.SFN_Vector2,id:26,x:33141,y:32338,v1:0,v2:0;n:type:ShaderForge.SFN_OneMinus,id:27,x:33196,y:32338|IN-38-OUT;n:type:ShaderForge.SFN_Multiply,id:28,x:33157,y:32318|A-21-OUT,B-27-OUT;n:type:ShaderForge.SFN_RemapRange,id:31,x:33189,y:32300,frmn:-1,frmx:1,tomn:0,tomx:1|IN-28-OUT;n:type:ShaderForge.SFN_Append,id:32,x:33189,y:32311|A-31-OUT,B-78-OUT;n:type:ShaderForge.SFN_Clamp01,id:34,x:33189,y:32318|IN-31-OUT;n:type:ShaderForge.SFN_Step,id:36,x:33189,y:32318|A-37-OUT,B-58-OUT;n:type:ShaderForge.SFN_Vector1,id:37,x:33169,y:32362,v1:0;n:type:ShaderForge.SFN_Clamp01,id:38,x:33163,y:32334|IN-19-OUT;n:type:ShaderForge.SFN_Lerp,id:43,x:33157,y:32318|A-45-OUT,B-36-OUT,T-90-OUT;n:type:ShaderForge.SFN_Vector1,id:45,x:33144,y:32362,v1:1;n:type:ShaderForge.SFN_NormalBlend,id:46,x:33189,y:32296|BSE-66-OUT,DTL-32-OUT;n:type:ShaderForge.SFN_Lerp,id:48,x:33157,y:32318|A-50-OUT,B-2-RGB,T-87-OUT;n:type:ShaderForge.SFN_Vector1,id:50,x:33169,y:32362,v1:0;n:type:ShaderForge.SFN_Lerp,id:52,x:33189,y:32318|A-53-OUT,B-46-OUT,T-87-OUT;n:type:ShaderForge.SFN_Vector3,id:53,x:33157,y:32303,v1:0,v2:0,v3:1;n:type:ShaderForge.SFN_Lerp,id:55,x:33144,y:32318|A-57-OUT,B-2-RGB,T-88-OUT;n:type:ShaderForge.SFN_Vector1,id:57,x:33144,y:32383,v1:0;n:type:ShaderForge.SFN_Relay,id:58,x:33163,y:32338|IN-8-OUT;n:type:ShaderForge.SFN_ComponentMask,id:59,x:33189,y:32316,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-28-OUT;n:type:ShaderForge.SFN_Dot,id:60,x:33200,y:32318,dt:3|A-59-OUT,B-59-OUT;n:type:ShaderForge.SFN_OneMinus,id:61,x:33200,y:32318|IN-60-OUT;n:type:ShaderForge.SFN_Vector3,id:64,x:33208,y:32352,v1:0,v2:0,v3:1;n:type:ShaderForge.SFN_Lerp,id:66,x:33189,y:32318|A-64-OUT,B-92-OUT,T-91-OUT;n:type:ShaderForge.SFN_Lerp,id:70,x:33189,y:32318|A-72-UVOUT,B-73-OUT,T-86-OUT;n:type:ShaderForge.SFN_TexCoord,id:72,x:33200,y:32318,uv:0;n:type:ShaderForge.SFN_Lerp,id:73,x:33169,y:32318|A-72-UVOUT,B-34-OUT,T-36-OUT;n:type:ShaderForge.SFN_Relay,id:74,x:33218,y:32383|IN-43-OUT;n:type:ShaderForge.SFN_Step,id:76,x:33189,y:32318|A-60-OUT,B-77-OUT;n:type:ShaderForge.SFN_Vector1,id:77,x:33200,y:32352,v1:1;n:type:ShaderForge.SFN_Lerp,id:78,x:33189,y:32304|A-77-OUT,B-61-OUT,T-76-OUT;n:type:ShaderForge.SFN_Time,id:79,x:33189,y:32303;n:type:ShaderForge.SFN_Frac,id:80,x:33208,y:32318|IN-81-OUT;n:type:ShaderForge.SFN_Multiply,id:81,x:33189,y:32290|A-79-T,B-82-OUT;n:type:ShaderForge.SFN_Vector1,id:82,x:33208,y:32362,v1:0.2;n:type:ShaderForge.SFN_Pi,id:83,x:33205,y:32336;n:type:ShaderForge.SFN_Multiply,id:85,x:33200,y:32318|A-80-OUT,B-83-OUT;n:type:ShaderForge.SFN_Sin,id:86,x:33189,y:32318|IN-85-OUT;n:type:ShaderForge.SFN_Vector1,id:87,x:33218,y:32303,v1:0;n:type:ShaderForge.SFN_Vector1,id:88,x:33169,y:32415,v1:1;n:type:ShaderForge.SFN_Vector1,id:90,x:33157,y:32362,v1:0;n:type:ShaderForge.SFN_Vector1,id:91,x:33189,y:32352,v1:0.5;n:type:ShaderForge.SFN_Vector3,id:92,x:33189,y:32318,v1:0,v2:0,v3:0;proporder:3;pass:END;sub:END;*/

Shader "Utility Shaders/Preview/Spherize" {
    Properties {
        _MainTexture ("Main Texture", 2D) = "white" {}
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTexture; uniform float4 _MainTexture_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(float4(v.normal,0), unity_WorldToObject).xyz;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
/////// Normals:
                float3 normalDirection =  i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = dot( normalDirection, lightDirection );
                float3 diffuse = max( 0.0, NdotL) * attenColor + UNITY_LIGHTMODEL_AMBIENT.rgb;
////// Emissive:
                float node_57 = 0.0;
                float2 node_72 = i.uv0;
                float node_8 = (distance(i.uv0.rg,float2(0.5,0.5))*-2.0+1.0);
                float2 node_28 = (normalize(((i.uv0.rg*2.0+-1.0)-float2(0,0)))*(1.0 - saturate(pow(node_8,0.5))));
                float2 node_31 = (node_28*0.5+0.5);
                float node_36 = step(0.0,node_8);
                float2 node_73 = lerp(node_72.rg,saturate(node_31),node_36);
                float4 node_79 = _Time + _TimeEditor;
                float2 node_70 = lerp(node_72.rg,node_73,sin((frac((node_79.g*0.2))*3.141592654)));
                float4 node_2 = tex2D(_MainTexture,TRANSFORM_TEX(node_70, _MainTexture));
                float3 emissive = lerp(float3(node_57,node_57,node_57),node_2.rgb,1.0);
                float3 finalColor = 0;
                float3 diffuseLight = diffuse;
                float node_50 = 0.0;
                float node_87 = 0.0;
                finalColor += diffuseLight * lerp(float3(node_50,node_50,node_50),node_2.rgb,node_87);
                finalColor += emissive;
/// Final Color:
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        Pass {
            Name "ForwardAdd"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            ZWrite Off
            
            Fog { Color (0,0,0,0) }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTexture; uniform float4 _MainTexture_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(float4(v.normal,0), unity_WorldToObject).xyz;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
/////// Normals:
                float3 normalDirection =  i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
/////// Diffuse:
                float NdotL = dot( normalDirection, lightDirection );
                float3 diffuse = max( 0.0, NdotL) * attenColor;
                float3 finalColor = 0;
                float3 diffuseLight = diffuse;
                float node_50 = 0.0;
                float2 node_72 = i.uv0;
                float node_8 = (distance(i.uv0.rg,float2(0.5,0.5))*-2.0+1.0);
                float2 node_28 = (normalize(((i.uv0.rg*2.0+-1.0)-float2(0,0)))*(1.0 - saturate(pow(node_8,0.5))));
                float2 node_31 = (node_28*0.5+0.5);
                float node_36 = step(0.0,node_8);
                float2 node_73 = lerp(node_72.rg,saturate(node_31),node_36);
                float4 node_79 = _Time + _TimeEditor;
                float2 node_70 = lerp(node_72.rg,node_73,sin((frac((node_79.g*0.2))*3.141592654)));
                float4 node_2 = tex2D(_MainTexture,TRANSFORM_TEX(node_70, _MainTexture));
                float node_87 = 0.0;
                finalColor += diffuseLight * lerp(float3(node_50,node_50,node_50),node_2.rgb,node_87);
/// Final Color:
                return fixed4(finalColor * 1,0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
