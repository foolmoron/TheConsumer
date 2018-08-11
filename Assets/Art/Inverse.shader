Shader "INverse"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue" = "Geometry+40" "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _CameraDepthTexture;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 world : TEXCOORD0;
				float4 screen : TEXCOORD1;
				float2 uv : TEXCOORD2;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex);
				o.screen = ComputeScreenPos(o.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			// Keijiro's hue to rgb
			half3 Hue2RGB(half h)
			{
				h = frac(h) * 6 - 2;
				half3 rgb = saturate(half3(abs(h - 1) - 1, 2 - abs(h), 2 - abs(h - 2)));
				return rgb;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, i.uv);

                fixed4 finalColor = 1 - c;

				return finalColor;
			}
			ENDCG
		}
	}
}
