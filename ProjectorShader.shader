Shader "Custom/ProjectorShader"
{
    Properties
    {
        _MainTex ("Projection Texture", 2D) = "white" {}
        _SurfaceColor ("Surface Color", Color) = (1, 1, 1, 1)
        _Brightness ("Brightness", Range(0, 5)) = 1
        _FalloffPower ("Falloff Power", Range(0.1, 5)) = 2

        // Projector Realism Properties
        _BlackLevel ("Black Level (Ambient Light)", Range(0, 0.3)) = 0.15
        _CenterHotspotIntensity ("Center Hotspot Intensity", Range(0, 1)) = 0.25
        _CenterHotspotSize ("Center Hotspot Size", Range(0.1, 2)) = 0.8
        _ColorTemperature ("Color Temperature Tint", Color) = (0.95, 0.98, 1.0, 1.0)
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

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 projPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4x4 _ProjectorMatrix;
            float3 _ProjectorPos;
            float4 _SurfaceColor;
            float _Brightness;
            float _FalloffPower;

            // Projector realism properties
            float _BlackLevel;
            float _CenterHotspotIntensity;
            float _CenterHotspotSize;
            float4 _ColorTemperature;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);

                // Calculate projection coordinates
                o.projPos = mul(_ProjectorMatrix, float4(o.worldPos, 1.0));

                return o;
            }
fixed4 frag (v2f i) : SV_Target
{
    float2 uv = i.projPos.xy / i.projPos.w;
    uv = uv * 0.5 + 0.5;
    uv.y = 1.0 - uv.y;
    uv.x = 1.0 - uv.x;

    if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
    {
        return fixed4(0, 0, 0, 1);
    }

    fixed4 projColor = tex2D(_MainTex, uv);

    // Distance falloff
    float distance = length(i.worldPos - _ProjectorPos);
    float falloff = 1.0 / pow(max(distance, 0.01), _FalloffPower);

    // Angle falloff
    float3 projDir = normalize(_ProjectorPos - i.worldPos);
    float angleFalloff = saturate(dot(i.worldNormal, projDir));

    // Center hotspot
    float2 centerOffset = uv - float2(0.5, 0.5);
    float distanceFromCenter = length(centerOffset);
    float centerHotspot = 1.0 - saturate(distanceFromCenter / _CenterHotspotSize);
    centerHotspot = pow(centerHotspot, 2.0);
    float hotspotMultiplier = 1.0 + (centerHotspot * _CenterHotspotIntensity);

    // Apply all effects
    fixed4 finalColor = projColor * _SurfaceColor * _Brightness * falloff * angleFalloff * hotspotMultiplier;

    // ADD: Color temperature
    finalColor.rgb *= _ColorTemperature.rgb;

    // ADD: Black level
    finalColor.rgb += _BlackLevel;

    return finalColor;
}
            ENDCG
        }
    }
}