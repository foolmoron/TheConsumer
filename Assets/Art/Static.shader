Shader "Static"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _FuzzinessX ("Fuzziness X", Float) = 100
        _FuzzinessY ("Fuzziness Y", Float) = 1000
        _ScaleX ("Scale X", Float) = 1
        _ScaleY ("Scale Y", Float) = 3
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
            float _FuzzinessX;
            float _FuzzinessY;
            float _ScaleX;
            float _ScaleY;

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

            float rand(float n){ return frac(sin(n) * 43758.5453123); }

			fixed4 frag (v2f i) : SV_Target
			{
                //i.uv = i.screen;

                i.uv.x += 2.0*sin(i.uv.y*0.2+_Time.w*0.1);
                float lum = rand(_ScaleX*floor(i.uv.x*_FuzzinessX)/_FuzzinessX + _ScaleY*floor(i.uv.y*_FuzzinessY)/_FuzzinessY + abs(sin(_Time.w*0.15 + i.uv.y*0.003)));
                fixed4 finalColor = float4(lum, lum, lum, 1);

				return finalColor;
			}
			ENDCG
		}
	}
}
