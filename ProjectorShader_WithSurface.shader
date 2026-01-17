Shader "Custom/ProjectorShader_Enhanced"
{
    Properties
    {
        _MainTex ("Projection Texture", 2D) = "white" {}
        _Brightness ("Brightness", Range(0, 10)) = 3.5
        _FalloffPower ("Falloff Power", Range(0.1, 5)) = 1.0

        // Projector Realism
        _BlackLevel ("Black Level (Ambient Light)", Range(0, 0.5)) = 0.08
        _CenterHotspotIntensity ("Center Hotspot Intensity", Range(0, 1)) = 0.15
        _CenterHotspotSize ("Center Hotspot Size", Range(0.1, 2)) = 1.0
        _ColorTemperature ("Color Temperature Tint", Color) = (0.98, 0.99, 1.0, 1.0)
        _HighlightDesaturation ("Highlight Desaturation", Range(0, 1)) = 0.1

        // _ColorTemperature ("Color Temperature Tint", Color) = (1.0, 0.99, 0.96, 1.0)
        // Base Material Textures
        [Header(Base Material Textures)]
        _BaseColor ("Base Albedo Texture", 2D) = "white" {}
        _BaseColorTint ("Base Color Tint", Color) = (1, 1, 1, 1)
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Map Strength", Range(0, 2)) = 1.0
        _HeightMap ("Height Map", 2D) = "gray" {}
        _HeightScale ("Height Scale", Range(0, 1)) = 0.5
        _AOMap ("Ambient Occlusion Map", 2D) = "white" {}
        _AOStrength ("AO Strength", Range(0, 1)) = 1.0
        _MetallicMap ("Metallic Map", 2D) = "white" {}
        _MetallicStrength ("Metallic Strength", Range(0, 1)) = 0.0

        // Surface Material Properties
        [Header(Surface Material)]
        _SurfaceAlbedo ("Surface Color (Fallback)", Color) = (0.8, 0.8, 0.8, 1)
        _Reflectance ("Surface Reflectance", Range(0, 1)) = 0.7
        _Roughness ("Surface Roughness", Range(0, 1)) = 0.5
        _Specular ("Specular Intensity", Range(0, 2)) = 0.1
        _SpecularPower ("Specular Sharpness", Range(1, 128)) = 32
        _ParallaxScale ("Surface Conformity Strength", Range(0, 3)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 projPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float3 worldTangent : TEXCOORD4;
                float3 worldBinormal : TEXCOORD5;
            };

            sampler2D _MainTex;
            sampler2D _BaseColor;
            float4 _BaseColor_ST;
            sampler2D _BumpMap;
            float _BumpScale;
            sampler2D _HeightMap;
            float _HeightScale;
            sampler2D _AOMap;
            float _AOStrength;
            sampler2D _MetallicMap;
            float _MetallicStrength;

            float4x4 _ProjectorMatrix;
            float3 _ProjectorPos;
            float _Brightness;
            float _FalloffPower;
            float _BlackLevel;
            float _CenterHotspotIntensity;
            float _CenterHotspotSize;
            float4 _ColorTemperature;
            float _HighlightDesaturation;

            // Surface properties
            float4 _SurfaceAlbedo;
            float4 _BaseColorTint;
            float _Reflectance;
            float _Roughness;
            float _Specular;
            float _SpecularPower;
            float _ParallaxScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.projPos = mul(_ProjectorMatrix, float4(o.worldPos, 1.0));

                // Pass UVs for base textures
                o.uv = TRANSFORM_TEX(v.uv, _BaseColor);

                // Calculate tangent space for normal mapping
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldBinormal = cross(o.worldNormal, o.worldTangent) * v.tangent.w;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // === STEP 1: SAMPLE BASE MATERIAL TEXTURES ===

                // Sample base color texture
                fixed4 baseColor = tex2D(_BaseColor, i.uv) * _BaseColorTint;

                // Sample height map (REAL height data for parallax)
                float heightValue = tex2D(_HeightMap, i.uv).r * _HeightScale;

                // Sample ambient occlusion
                float ao = lerp(1.0, tex2D(_AOMap, i.uv).r, _AOStrength);

                // Sample metallic
                float metallic = tex2D(_MetallicMap, i.uv).r * _MetallicStrength;

                // Sample and apply normal map
                float3 normalMap = UnpackNormal(tex2D(_BumpMap, i.uv));
                normalMap.xy *= _BumpScale;

                // Transform normal from tangent space to world space
                float3 worldNormal = normalize(
                    i.worldTangent * normalMap.x +
                    i.worldBinormal * normalMap.y +
                    i.worldNormal * normalMap.z
                );

                // === STEP 2: CALCULATE BASE LIGHTING ===

                // Simple directional light (sun)
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = saturate(dot(worldNormal, lightDir));

                // Diffuse lighting
                float3 diffuseLight = NdotL * _LightColor0.rgb;

                // Ambient light
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;

                // Combine lighting with AO
                float3 lighting = (diffuseLight + ambient) * ao;

                // Apply lighting to base color
                fixed4 litSurface = baseColor * float4(lighting, 1.0);

                // === STEP 3: CALCULATE PROJECTION UV ===

                float2 projUV = i.projPos.xy / i.projPos.w;
                projUV = projUV * 0.5 + 0.5;
                projUV.y = 1.0 - projUV.y;
                projUV.x = 1.0 - projUV.x;

                // Calculate projection direction (needed for parallax)
                float3 projDir = normalize(_ProjectorPos - i.worldPos);

                // **Height-based UV displacement for surface conformity**
                // Calculate projection direction in tangent space
                float3 projDirTangent;
                projDirTangent.x = dot(projDir, i.worldTangent);
                projDirTangent.y = dot(projDir, i.worldBinormal);
                projDirTangent.z = dot(projDir, i.worldNormal);

                // Apply parallax offset using REAL height map data
                float2 parallaxOffset = projDirTangent.xy / projDirTangent.z * heightValue * _ParallaxScale;
                projUV += parallaxOffset;

                // Check if within projection bounds
                bool inBounds = (projUV.x >= 0 && projUV.x <= 1 && projUV.y >= 0 && projUV.y <= 1);

                // If outside projection, just return lit surface
                if (!inBounds)
                {
                    return litSurface;
                }

                // === STEP 4: CALCULATE PROJECTION ===

                // Sample projection texture
                fixed4 projColor = tex2D(_MainTex, projUV);

                // Distance falloff
                float distance = length(i.worldPos - _ProjectorPos);
                float falloff = 1.0 / pow(max(distance, 0.01), _FalloffPower);

                // Angle falloff (use surface normal)
                float angleFalloff = saturate(dot(worldNormal, projDir));
                angleFalloff = pow(angleFalloff, lerp(1.0, 2.0, _Roughness));

                // Center hotspot
                float2 centerOffset = projUV - float2(0.5, 0.5);
                float distanceFromCenter = length(centerOffset);
                float centerHotspot = 1.0 - saturate(distanceFromCenter / _CenterHotspotSize);
                centerHotspot = pow(centerHotspot, 2.0);
                float hotspotMultiplier = 1.0 + (centerHotspot * _CenterHotspotIntensity);

                // Specular highlight (enhanced by metallic)
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 reflectDir = reflect(-projDir, worldNormal);
                float specularFactor = pow(saturate(dot(viewDir, reflectDir)), _SpecularPower) * _Specular;
                specularFactor *= (1.0 - _Roughness * 0.8);
                specularFactor *= (1.0 + metallic * 2.0); // Metallic boosts specular

                // // === STEP 5: COMBINE PROJECTION WITH BASE ===

                // // Projection layer (affected by surface reflectance)
                // float4 projection = projColor * _Reflectance;
                // projection *= _Brightness * falloff * angleFalloff * hotspotMultiplier;

                // // Add specular
                // projection += projColor * specularFactor * _Brightness * falloff;

                // // Apply color temperature to projection
                // projection.rgb *= _ColorTemperature.rgb;

                // // === STEP 6: BLEND PROJECTION ONTO SURFACE ===

                // // Additive blend: base surface + projection
                // fixed4 finalColor = litSurface + projection;

                // // Add black level (projector's minimum light)
                // finalColor.rgb += _BlackLevel;

                // return finalColor;
                // === STEP 5: COMBINE PROJECTION WITH BASE ===

                // Projection layer (affected by surface reflectance)
                float4 projection = projColor * _Reflectance;

                // Surface acts as a color filter (LIGHT filtering, not heavy)
                // projection.rgb *= lerp(float3(1,1,1), baseColor.rgb, 0.4); // Only 40% surface influence
                // projection.rgb *= lerp(float3(1,1,1), baseColor.rgb, 0.8); // 80% filtering
                projection.rgb *= baseColor.rgb;
                projection *= _Brightness * falloff * angleFalloff * hotspotMultiplier;

                // Add specular (less affected by surface color)
                // float3 specular = projColor.rgb * specularFactor * _Brightness * falloff;
                // specular *= lerp(float3(1,1,1), baseColor.rgb, 0.2); // Only 20% filtering for specular
                // projection.rgb += specular;

                // ADD THESE LINES:
                // Desaturate bright areas (realistic projector behavior)
                float maxChannel = max(projection.r, max(projection.g, projection.b));
                float luminance = dot(projection.rgb, float3(0.299, 0.587, 0.114));
                projection.rgb = lerp(projection.rgb, float3(luminance, luminance, luminance), _HighlightDesaturation * maxChannel);

                // Apply color temperature to projection
                projection.rgb *= _ColorTemperature.rgb;

                // === STEP 6: BLEND PROJECTION ONTO SURFACE ===

                // Additive blend: base surface + projection
                fixed4 finalColor = litSurface + projection;

                // Add black level (projector's minimum light)
                finalColor.rgb += _BlackLevel;

                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}