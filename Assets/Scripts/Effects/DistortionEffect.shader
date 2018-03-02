Shader "Particles/DistortionEffect"
{
	Properties
	{
		_MainTex ("Frame (RGB)", 2D) = "white" {}
		_DistortionLayer ("Distortion Mask (RGB)", 2D) = "white" {}
		_DistortionST ("Distortion Mask ST", Vector) = (1, 1, 0, 0)
		_Strength ("Strength", Float) = 0.02
		_Alpha ("Alpha", Float) = 1
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent +10"
			"IgnoreProjector"="True"
			"RenderType"="Transparent" 
		}
		BindChannels
		{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}

		AlphaTest Greater 0.01
		ColorMask RGB
		Cull Back Lighting Off ZWrite Off
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				//float2 screenposition : TEXCOORD1;
				//float2 screendepth : TEXCOORD2;
				float2 worldxy : TEXCOORD3;
				float2 worldzw : TEXCOORD4;
			};
			
			sampler2D _MainTex;
			sampler2D _DistortionLayer;
			sampler2D _CameraDepthTexture;
			float4 _DistortionST;
			half _Alpha;
			half _Strength;
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				float4 vertexProj = mul(UNITY_MATRIX_MVP, v.vertex);
				o.vertex = vertexProj;
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex) * _DistortionST.xy + _DistortionST.zw;
				//o.screenposition.xy = half2(vertexProj.x / vertexProj.w * 0.5 + 0.5, vertexProj.y / vertexProj.w * 0.5 + 0.5);
				//o.screendepth.xy = half2(vertexProj.z / vertexProj.w, vertexProj.w);
				o.worldxy = v.vertex.xy;
				o.worldzw = v.vertex.zw;
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				float4 fragProj = mul(UNITY_MATRIX_MVP, float4(i.worldxy, i.worldzw));
				half2 screenPosition = half2(fragProj.x / fragProj.w * 0.5 + 0.5, fragProj.y / fragProj.w * 0.5 + 0.5);
				half2 screenDepth = half2(fragProj.z / fragProj.w, fragProj.w);
				//float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, screenPosition));
				float selfDepth = screenDepth.x;
				half4 originalTex = tex2D(_MainTex, screenPosition);
				half4 distortionTexBase = tex2D(_DistortionLayer, i.texcoord);
				half4 distortionTex = tex2D(_DistortionLayer, i.texcoord + float2(_Time.y, _Time.y));
				half2 distortion = distortionTex - distortionTexBase;
				float depthDistorted = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, screenPosition + distortion.xy * _Strength));
				half4 renderTex = tex2D(_MainTex, screenPosition + distortion.xy * _Strength);
				
				if(depthDistorted >= selfDepth)
					return half4(lerp(originalTex, renderTex, _Alpha).rgb, distortionTex.w * _Alpha);
				else
					return half4(0, 0, 0, 0);

			}
			
			ENDCG
		}
	}
}