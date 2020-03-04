Shader "CZ/Night2d_Building" {
	
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_AlphaChanel ("Alpha(R) Light(G) Tex", 2D) = "white" {}
		_Color ("Main Color", Color) = (1, 1, 1, 1)
		_AddOnColorA ("Light Color A", Color) = (1, 1, 1, 1)
		_AddOnColorB ("Light Color B", Color) = (1, 1, 1, 1)
		_FlickerTime ("Flicker Enable", Range(0, 1)) = 0
		_LightEnable ("Light Enable", Range(0, 1)) = 0
		
		//[Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Float) = 1
		//_XRayColor ("XRay Color", Color) = (1, 1, 1, 1)
	}
	
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		LOD 100
		Cull Off
		Lighting Off
		
		/* 遮挡效果渲染
		Pass {
			Blend SrcAlpha One
			ZWrite Off
			ZTest Greater
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			
			sampler2D _AlphaChanel;
			float4 _AlphaChanel_ST;
			
			float4 _XRayColor;
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);	
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);
				
				float3 ctrl = tex2D(_AlphaChanel, i.uv).rgb;
				col.a = ctrl.r;
				
				return col * _XRayColor;
			}
			ENDCG
		}*/
		
		// 正常效果渲染
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			//ZWrite [_ZWrite]
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _AlphaChanel;
			float4 _AlphaChanel_ST;
			
			float4 _Color;

			float4 _AddOnColorA;
			float4 _AddOnColorB;
			fixed _FlickerTime;
			fixed _LightEnable;

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = tex2D(_MainTex, i.uv);
				
				fixed lerpT = abs(cos(_Time.w * _FlickerTime));
				fixed4 addOnColor = lerp(_AddOnColorA, _AddOnColorB, lerpT);
				
				float3 ctrl = tex2D(_AlphaChanel, i.uv).rgb;
				col.a = ctrl.r;

				return col * _Color + addOnColor * ctrl.g * _LightEnable;
			}
			ENDCG
		}
	}
}
