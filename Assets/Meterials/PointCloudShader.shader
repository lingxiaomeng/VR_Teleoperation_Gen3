//This is a modified version of a shader published by smb02dunnal on the Unity forums:
//https://forum.unity3d.com/threads/billboard-geometry-shader.169415/

Shader "Custom/PointCloudShader"
{
	Properties
	{
		_PointSize("PointSize",float ) = 0.01
	}

		SubShader
	{
	Pass
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
#pragma target 5.0
#pragma vertex VS_Main
#pragma fragment FS_Main
#pragma geometry GS_Main
#include "UnityCG.cginc" 

		// **************************************************************
		// Data structures												*
		// **************************************************************
		struct appdata
		 {
			 float4 vertex : POSITION;
			 float4	color		: COLOR;
			 UNITY_VERTEX_INPUT_INSTANCE_ID
		 };


		struct GS_INPUT
		{
			float4	pos		: POSITION;
			float4	col		: COLOR;
			UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
		};

		struct FS_INPUT
		{
			float4	pos		: POSITION;
			float4  col		: COLOR;
			UNITY_VERTEX_OUTPUT_STEREO
		};


		// **************************************************************
		// Vars															*
		// **************************************************************

		float _PointSize;

		// **************************************************************
		// Shader Programs												*
		// **************************************************************

		// Vertex Shader ------------------------------------------------
		GS_INPUT VS_Main(appdata v)
		{
			GS_INPUT output;

			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_OUTPUT(GS_INPUT, output);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
			UNITY_TRANSFER_INSTANCE_ID(v, output);

			output.pos = v.vertex;
			output.col = v.color;

			return output;
		}

		// Geometry Shader -----------------------------------------------------
		[maxvertexcount(4)]
		void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
		{

			float3 up = float3(0, 1, 0);
			float3 look = _WorldSpaceCameraPos - p[0].pos;
			look.y = 1;
			look = normalize(look);
			float3 right = cross(up, look);
			float3 up1 = cross(right,look);
			float halfS = 0.5f * _PointSize;
			right = normalize(right);

			float4 v[4];
			v[0] = float4(p[0].pos + halfS * right - halfS * up, 0.0f);
			v[1] = float4(p[0].pos + halfS * right + halfS * up, 0.0f);
			v[2] = float4(p[0].pos - halfS * right - halfS * up, 0.0f);
			v[3] = float4(p[0].pos - halfS * right + halfS * up, 0.0f);


			FS_INPUT pIn;

			UNITY_SETUP_INSTANCE_ID(p[0]);
			UNITY_INITIALIZE_OUTPUT(FS_INPUT, pIn);
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(p[0]);

			pIn.pos = UnityObjectToClipPos(v[0]);
			pIn.col = p[0].col;


			UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], pIn);
			triStream.Append(pIn);

			pIn.pos = UnityObjectToClipPos(v[1]);
			pIn.col = p[0].col;

			UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], pIn);
			triStream.Append(pIn);

			pIn.pos = UnityObjectToClipPos(v[2]);
			pIn.col = p[0].col;

			UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], pIn);

			triStream.Append(pIn);

			pIn.pos = UnityObjectToClipPos(v[3]);
			pIn.col = p[0].col;

			UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], pIn);
			triStream.Append(pIn);
		}

		// Fragment Shader -----------------------------------------------
		float4 FS_Main(FS_INPUT input) : COLOR
		{

			float4 output;
			UNITY_INITIALIZE_OUTPUT(float4, output);
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
			output = input.col;
			return output;
		}

		ENDCG
	}
	}
}
