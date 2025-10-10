Shader "Light2D/Internal/MeshModeAdditive"
{
    Properties
    {
        _Lightmap ("Lightmap", 2D) = "black" {}
        _Sprite ("Sprite Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Invert("Invert", Float) = 0
    }

    Category
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha One
        Cull Off
        Lighting Off
        ZWrite Off

        SubShader
        {
            Pass
            {
                CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0

                #include "UnityCG.cginc"

                sampler2D _Lightmap;
                sampler2D _Sprite;
                float4 _Color;
                float _Invert;
                
                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord : TEXCOORD0;
                };
     
                v2f vert (appdata_t v)
                {
                    v2f o;

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = _Color;
                    o.texcoord = v.texcoord;
            
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    float alpha = 1 - tex2D(_Lightmap, i.texcoord).r;

                    float4 sprite = tex2D(_Sprite, i.texcoord);;

                    float4 color = float4(1, 1, 1, 1);

                    color.rgb *= sprite.rgb * sprite.a * i.color.rgb * i.color.a * alpha;

                    return color;
                }
                
                ENDCG
            }
        }
    }
}