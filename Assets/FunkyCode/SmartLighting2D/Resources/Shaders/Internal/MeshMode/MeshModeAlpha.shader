
Shader "Light2D/Internal/MeshModeAlpha"
{
    Properties
    {
        _Lightmap ("Lightmap", 2D) = "black" {}
        _Sprite ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Invert("Invert", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Blend One OneMinusSrcAlpha

        Cull Off Lighting Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _Lightmap;
            sampler2D _Sprite;
            float4 _Color;
            float _Invert;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float alpha = 1 - tex2D(_Lightmap, i.uv).r;

                float4 sprite = tex2D(_Sprite, i.uv);;

                float4 color = float4(1, 1, 1, 1);

                color.rgb *= sprite.rgb * sprite.a * alpha * i.color.a * i.color.rgb;
     
                color.a = alpha * sprite.a * sprite.r * i.color.a * i.color.rgb;

                return color;
            }
            ENDCG
        }
    }
}
