// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Sinuous Shrooms/Standard Shroom"
{
	Properties
	{
			[Space]
		[Header(Base Material)]
		_BaseColor1("Base Color 1", Color) = (1,1,1,1)
		_BaseColor2("Base Color 2", Color) = (1,1,1,1)
		_BaseTex("Base Pattern", 2D) = "white" {}
		_BaseTexScale("Base Pattern Scale" , Range(1 , 10)) = 2.0
		
			[Space]
		[Header(Cap)]
		_CapColor("Cap Color", Color) = (1,1,1,1)
		_Capglow("Cap Glow",Range(0.0,0.7)) = 0.0
		_CapTex("Cap Mask", 2D) = "white" {}
		_CapTexScale("Cap Mask Scale" , Range(0.5 , 3.0)) = 2.0

			[Space]
		[Header(Gills)]
		_BottomColor ("Gills Color", Color) = (1,1,1,1)
		_Underglow ("Gills Glow",Range(0.0,0.7)) = 0.2


			[Space]
		[Header(PBR)]
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }
		LOD 400

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0
		#include "UnityCG.cginc"

		sampler2D _CapTex,_BaseTex;

		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _CapColor, _BaseColor1, _BaseColor2, _BottomColor;
		half _CapTexScale, _BaseTexScale;
		fixed _Underglow, _Capglow;

		fixed calcangle(float2 uv, float2 center = 0.5, float offset = 0.0)
		{
			float2 o = uv - center;
			fixed res = atan2(o.x, o.y) * 0.15915494309 + 0.5;
			return frac(res + offset);
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			float3 objwPos = mul(unity_ObjectToWorld, float4(0.0,0.0,0.0, 1.0)).xyz;
			float camDist = distance(objwPos, _WorldSpaceCameraPos);

			if (camDist > 50.0)
			{
				o.Albedo = lerp(_BaseColor1, _CapColor,0.25) * 0.5;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Emission = 0.0;
				o.Alpha = 1.0;
			}
			else
			{
				float3 objPos = mul(unity_WorldToObject, float4(IN.worldPos, 1.0)).xyz;
				float3 wpos = IN.worldPos;
				float3 normal = mul(unity_WorldToObject, float4(IN.worldNormal, 0.0)).xyz;

				fixed down = smoothstep(0.4, 1.0, dot(normal, float3(0.0, -1.0, 0.0)));
				fixed up = smoothstep(0.0, 1.0, dot(normal, float3(0.0, 1.0, 0.0)));
				fixed sides = smoothstep(0.0, 0.5, 1.0 - abs(dot(normal, float3(0.0, 1.0, 0.0))));

				fixed d = saturate(length(objPos.xz) * 0.5);
				fixed dc = smoothstep(0.1, 0.3, d);
				fixed a = calcangle(objPos.xz, 0.0);

				fixed4 g = lerp(0.0, Luminance(tex2D(_BaseTex, float2(sin(a*6.28318 * floor(_BaseTexScale)), objPos.y)).rgb), _BaseColor2.a*sides*dc);
				fixed4 base = lerp(_BaseColor1, _BaseColor2, g);

				fixed4 capTex = tex2D(_CapTex, (0.5 + 0.5*(objPos.xz / _CapTexScale)));
				fixed capMask = lerp(1.0, capTex.a, _CapColor.a);
				half3 cap = lerp(base, _CapColor.rgb * capTex.rgb, capMask);

				half3 gills = _BottomColor.rgb * smoothstep(0.1, 0.5, d * sin(a * 540.0) * 0.5 + 0.5);

				fixed3 c = lerp(base, gills, down);
				c = lerp(c, cap, up);

				fixed glow = (0.8 + 0.2 * sin(_Time.y * 0.5));

				o.Albedo = c.rgb * (0.5 + 0.5*g);
				o.Metallic = _Metallic * (0.25 + 0.75*g);
				o.Smoothness = _Glossiness * (1.0 - down);
				o.Emission = (cap * up  * _Capglow + gills * down * _Underglow) * glow;
				o.Alpha = 1.0;
			}
		}
		ENDCG
	}
	FallBack "Diffuse"
}