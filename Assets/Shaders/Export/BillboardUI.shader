
Shader "Custom/BillboardUI"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
		_Scaling("Scaling", Float) = 1.0
		[Toggle] _KeepConstantScaling("Keep Constant Scaling", Float) = 1
		[Enum(RenderOnTop, 0, RenderWithTest, 4)] _ZTest("Render on top", Int) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "DisableBatching"="True"
        }

        Cull Back
        Lighting Off
        ZWrite Off
        ZTest [_ZTest]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

			sampler2D _MainTex;
            fixed4 _TextureSampleAdd;
			float _KeepConstantScaling;
			float _Scaling;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				float relativeScaler =  (_KeepConstantScaling) ? distance(mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)), _WorldSpaceCameraPos) : 1;
				OUT.vertex = mul(UNITY_MATRIX_P, float4(UnityObjectToViewPos(float3(0.0, 0.0, 0.0)), 1.0) + float4(v.vertex.x, v.vertex.y, 0.0, 0.0) * relativeScaler * _Scaling);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                return (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
            }
        ENDCG
        }
    }
}
