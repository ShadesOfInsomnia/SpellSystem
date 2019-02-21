

Shader "Hidden/SpellSystemAmplifiedBloom" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Bloom0 ("Bloom0 (RGB)", 2D) = "black" {}
		_Bloom1 ("Bloom1 (RGB)", 2D) = "black" {}
		_Bloom2 ("Bloom2 (RGB)", 2D) = "black" {}
		_Bloom3 ("Bloom3 (RGB)", 2D) = "black" {}
		_Bloom4 ("Bloom4 (RGB)", 2D) = "black" {}
		_Bloom5 ("Bloom5 (RGB)", 2D) = "black" {}
		_LensDirt ("Lens Dirt", 2D) = "black" {}
	}
	
	
	CGINCLUDE
		#include "UnityCG.cginc"
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Bloom0;
		sampler2D _Bloom1;
		sampler2D _Bloom2;
		sampler2D _Bloom3;
		sampler2D _Bloom4;
		sampler2D _Bloom5;
		sampler2D _LensDirt;
		
		uniform half4 _MainTex_TexelSize;
		
		uniform float _BlurSize;
		uniform float _BloomIntensity;
		uniform float _LensDirtIntensity;
		
		struct v2f_simple 
		{
			float4 pos : SV_POSITION; 
			half4 uv : TEXCOORD0;

        #if UNITY_UV_STARTS_AT_TOP
			half4 uv2 : TEXCOORD1;
		#endif
		};	
		 
		v2f_simple vertBloom ( appdata_img v )
		{
			v2f_simple o;
			
			o.pos = UnityObjectToClipPos (v.vertex);
        	o.uv = float4(v.texcoord.xy, 1, 1);		
        	
        	#if UNITY_UV_STARTS_AT_TOP
        		o.uv2 = float4(v.texcoord.xy, 1, 1);				
        		if (_MainTex_TexelSize.y < 0.0)
        			o.uv.y = 1.0 - o.uv.y;
        	#endif
        	        	
			return o; 
		}
		
		fixed3 Fixed3(float x)
		{
			return fixed3(x, x, x);
		}
		
		fixed4 fragBloom ( v2f_simple i ) : SV_Target
		{	
        	#if UNITY_UV_STARTS_AT_TOP
				float2 coord = i.uv2.xy;
			#else
				float2 coord = i.uv.xy;
			#endif
			fixed4 color = tex2D(_MainTex, coord);
				
			fixed3 lens = tex2D(_LensDirt, i.uv.xy).rgb;
			
			fixed3 b0 = tex2D(_Bloom0, coord).rgb;
			fixed3 b1 = tex2D(_Bloom1, coord).rgb;
			fixed3 b2 = tex2D(_Bloom2, coord).rgb;
			fixed3 b3 = tex2D(_Bloom3, coord).rgb;
			fixed3 b4 = tex2D(_Bloom4, coord).rgb;
			fixed3 b5 = tex2D(_Bloom5, coord).rgb;
			
			
			fixed3 bloom = b0 * 0.5f
						 + b1 * 0.8f * 0.75f
						 + b2 * 0.6f
						 + b3 * 0.45f 
						 + b4 * 0.35f
						 + b5 * 0.23f;
						 ;
			
			bloom /= 2.2;
			
			fixed3 lensBloom = b0 * 1.0f + b1 * 0.8f + b2 * 0.6f + b3 * 0.45f + b4 * 0.35f + b5 * 0.23f;
			lensBloom /= 3.2;
			
			color.rgb = lerp(color.rgb, bloom.rgb, Fixed3(_BloomIntensity));
			color.r = lerp(color.r, lensBloom.r, (saturate(lens.r * _LensDirtIntensity)));
			color.g = lerp(color.g, lensBloom.g, (saturate(lens.g * _LensDirtIntensity)));
			color.b = lerp(color.b, lensBloom.b, (saturate(lens.b * _LensDirtIntensity)));

			//color.rgb += bloom;
			return color;
		} 
		
		
		struct v2f_tap
		{
			float4 pos : SV_POSITION;
			half4 uv20 : TEXCOORD0;
			half4 uv21 : TEXCOORD1;
			half4 uv22 : TEXCOORD2;
			half4 uv23 : TEXCOORD3;
		};
		
		v2f_tap vert4Tap ( appdata_img v )
		{
			v2f_tap o;

			o.pos = UnityObjectToClipPos (v.vertex);
        	o.uv20 = half4(v.texcoord.xy + _MainTex_TexelSize.xy, 0.0, 0.0);				
			o.uv21 = half4(v.texcoord.xy + _MainTex_TexelSize.xy * half2(-0.5h,-0.5h), 0.0, 0.0);	
			o.uv22 = half4(v.texcoord.xy + _MainTex_TexelSize.xy * half2(0.5h,-0.5h), 0.0, 0.0);		
			o.uv23 = half4(v.texcoord.xy + _MainTex_TexelSize.xy * half2(-0.5h,0.5h), 0.0, 0.0);		
  
			return o; 
		}		
		
		fixed4 fragDownsample ( v2f_tap i ) : SV_Target
		{				
			fixed4 color = tex2D (_MainTex, i.uv20.xy);
			color += tex2D (_MainTex, i.uv21.xy);
			color += tex2D (_MainTex, i.uv22.xy);
			color += tex2D (_MainTex, i.uv23.xy);
			return max(color/4, 0);
		}
		
		static const half curve[7] = { 0.0205, 0.0855, 0.232, 0.324, 0.232, 0.0855, 0.0205 };

		static const half4 curve4[7] = { half4(0.0205,0.0205,0.0205,0), 
										 half4(0.0855,0.0855,0.0855,0), 
										 half4(0.232,0.232,0.232,0),
										 half4(0.324,0.324,0.324,1), 
										 half4(0.232,0.232,0.232,0), 
										 half4(0.0855,0.0855,0.0855,0), 
										 half4(0.0205,0.0205,0.0205,0) };
										 
		
		struct v2f_withBlurCoords8 
		{
			float4 pos : SV_POSITION;
			half4 uv : TEXCOORD0;
			half4 offs : TEXCOORD1;
		};		
		
		v2f_withBlurCoords8 vertBlurHorizontal (appdata_img v)
		{
			v2f_withBlurCoords8 o;
			o.pos = UnityObjectToClipPos (v.vertex);
			
			o.uv = half4(v.texcoord.xy,1,1);
			o.offs = half4(_MainTex_TexelSize.xy * half2(1.0, 0.0) * _BlurSize,1,1);

			return o; 
		}
		
		v2f_withBlurCoords8 vertBlurVertical (appdata_img v)
		{
			v2f_withBlurCoords8 o;
			o.pos = UnityObjectToClipPos (v.vertex);
			
			o.uv = half4(v.texcoord.xy,1,1);
			o.offs = half4(_MainTex_TexelSize.xy * half2(0.0, 1.0) * _BlurSize,1,1);
			 
			return o; 
		}	

		half4 fragBlur8 ( v2f_withBlurCoords8 i ) : SV_Target
		{
			half2 uv = i.uv.xy; 
			half2 netFilterWidth = i.offs.xy;  
			half2 coords = uv - netFilterWidth * 3.0;  
			
			half4 color = 0;
  			for( int l = 0; l < 7; l++ )  
  			{   
				half4 tap = tex2D(_MainTex, coords);
				color += tap * curve4[l];
				coords += netFilterWidth;
  			}
			return color;
		}
		
		float3 FLOAT3(float x)
		{
			return float3(x,x,x);
		}
		
		half4 fragClamp(v2f_simple input) : SV_Target
		{
		    #if UNITY_UV_STARTS_AT_TOP
				float2 coord = input.uv2.xy;
			#else
				float2 coord = input.uv.xy;
			#endif
		
			half4 source = tex2D(_MainTex, coord.xy);
			float maximum = 100000.0;
			source.r = min(source.r, maximum);
			source.g = min(source.g, maximum);
			source.b = min(source.b, maximum);
			return source;
		}
		
	ENDCG

	SubShader 
	{
		ZTest Off Cull Off ZWrite Off Blend Off
		Fog {Mode off}
		
		Pass	//0 Main
		{
			CGPROGRAM
			#pragma vertex vertBloom
			#pragma fragment fragBloom
			#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG
		}
		
		Pass 	//1 Downsample
		{ 	
			CGPROGRAM			
			#pragma vertex vert4Tap
			#pragma fragment fragDownsample
			#pragma fragmentoption ARB_precision_hint_fastest 			
			ENDCG		 
		}
		
		Pass 	//2 Blur Vertical
		{ 	
			CGPROGRAM			
			#pragma vertex vertBlurVertical
			#pragma fragment fragBlur8
			#pragma fragmentoption ARB_precision_hint_fastest 			
			ENDCG		 
		}
		
		Pass 	//3 Blur Horizontal
		{ 	
			CGPROGRAM			
			#pragma vertex vertBlurHorizontal
			#pragma fragment fragBlur8
			#pragma fragmentoption ARB_precision_hint_fastest 			
			ENDCG		 
		}
		
		Pass 	//4 Clamp
		{
			CGPROGRAM
			#pragma vertex vertBloom
			#pragma fragment fragClamp
			#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG
		}
	} 
	FallBack Off
}
