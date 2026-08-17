Shader "ZombieWar/URP/Zombie Simple Dissolve"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _DissolveNoise ("Dissolve Noise", 2D) = "gray" {}
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0.001,0.25)) = 0.06
        _DissolveEdgeColor ("Dissolve Edge Color", Color) = (1.0,0.35,0.05,1.0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Cull Off
            ZWrite On
            ZTest LEqual
            Blend One Zero

            CGPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            // Built-in Unity include.
            // No dependency on com.unity.render-pipelines.universal.
            #include "UnityCG.cginc"

            sampler2D _BaseMap;
            float4 _BaseMap_ST;

            sampler2D _DissolveNoise;
            float4 _DissolveNoise_ST;

            fixed4 _BaseColor;
            fixed4 _DissolveEdgeColor;
            float _DissolveAmount;
            float _DissolveEdgeWidth;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 baseUV : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.baseUV = TRANSFORM_TEX(v.uv, _BaseMap);
                o.noiseUV = TRANSFORM_TEX(v.uv, _DissolveNoise);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_BaseMap, i.baseUV) * _BaseColor;

                float amount = saturate(_DissolveAmount);

                // Important:
                // amount == 0 means absolutely no clipping.
                if (amount <= 0.0001)
                {
                    col.a = 1.0;
                    return col;
                }

                float noise = tex2D(_DissolveNoise, i.noiseUV).r;

                // 1.001 makes amount=1 fully disappear.
                float threshold = amount * 1.001;
                float d = noise - threshold;

                clip(d);

                float edgeWidth = max(_DissolveEdgeWidth, 0.001);
                float edge = 1.0 - smoothstep(0.0, edgeWidth, d);

                col.rgb += _DissolveEdgeColor.rgb * edge;
                col.a = 1.0;

                return col;
            }
            ENDCG
        }
    }

    Fallback "Unlit/Texture"
}
