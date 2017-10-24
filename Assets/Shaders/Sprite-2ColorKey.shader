// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Sprites/Sprite-2ColorKey"
{
	Properties
	{

		_MainTex ("Texture", 2D) = "black" {}
		_ColorK1 ("Color Key 1",Color) =(1,1,1,1)
		_ColorK2 ("Color Key 2",Color) =(1,1,1,1)
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector" = "True" }
		LOD 200


		Pass
		{
			ColorMask RGBA
			Cull Off
			Lighting Off
			Fog {Mode Off}
			ZWrite Off
			Offset -1,-1

			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			fixed4 _ColorK1;
			fixed4 _ColorK2;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f IN) : COLOR//SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, IN.texcoord)*IN.color;
				// apply fog
				//UNITY_APPLY_FOG(i.fogCoord, col);
				if( (col.r >= 0.8 && col.r <= 0.9) && (col.g <= 0.2 && col.g >= 0.1) && col.b == 0 && col.a == 1)
					col = _ColorK1;

				if( (col.r >= 0.74 && col.r <= 0.8) && (col.g <= 0.15 && col.g >= 0.09) && col.b == 0 && col.a == 1)
					col = _ColorK1;

				if( (col.r >= 0.57 && col.r <= 0.59) && (col.g <= 0.083 && col.g >= 0.081) && col.b == 0 && col.a == 1 )
					col = _ColorK2;

				return col;
			}
			ENDCG
		}
	}
}
