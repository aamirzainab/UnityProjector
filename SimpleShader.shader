Shader "Custom/SimpleProjector"
{
    Properties
    {
        _MainTex ("Projection Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 projPos : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4x4 _ProjectorMatrix;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.projPos = mul(_ProjectorMatrix, float4(worldPos, 1.0));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate projection UV
                float2 projUV = i.projPos.xy / i.projPos.w;
                projUV = projUV * 0.5 + 0.5;
                projUV.y = 1.0 - projUV.y;
                projUV.x = 1.0 - projUV.x;

                // Bounds check
                if (projUV.x < 0 || projUV.x > 1 || projUV.y < 0 || projUV.y > 1)
                {
                    return fixed4(0, 0, 0, 1);
                }

                // Sample and return projection texture
                return tex2D(_MainTex, projUV);
            }
            ENDCG
        }
    }
}