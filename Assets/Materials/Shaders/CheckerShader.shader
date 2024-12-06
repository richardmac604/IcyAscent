Shader "Custom/CheckeredPattern"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0, 0, 0, 1) // Black
        _Color2 ("Color 2", Color) = (1, 1, 1, 1) // White
        _Tiling ("Tiling", Float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Tiling;
            float4 _Color1;
            float4 _Color2;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _Tiling; // Apply tiling to UV coordinates
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Create a checkered pattern
                float checker = fmod(floor(i.uv.x) + floor(i.uv.y), 2.0);
                return lerp(_Color1, _Color2, checker);
            }
            ENDCG
        }
    }
}
