// Full-screen ink outline for URP's Full Screen Pass renderer feature (requires Depth + Normal).
// Draws lines where depth jumps (silhouettes) or normals turn sharply (creases).
// Distance handling, so far-away scenery doesn't turn into dark noise:
//  - creases (inner lines) fade out first, silhouettes later and only down to _FarStrength;
//  - lines get thinner with distance (sample offset shrinks towards 1px);
//  - the depth threshold grows on surfaces seen at a grazing angle (no false lines across floors);
//  - edges are smoothstepped instead of stepped, so they don't crawl/alias while the camera moves.
Shader "Supono/ToonOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0.13, 0.1, 0.17, 0.85)
        _Thickness ("Thickness (px @1080p)", Range(0.5, 4)) = 1.25
        _DepthThreshold ("Depth Threshold", Range(0.001, 0.3)) = 0.06
        _NormalThreshold ("Normal Threshold", Range(0.05, 2)) = 0.6
        _GrazingBoost ("Grazing Angle Threshold Boost", Range(0, 10)) = 4
        [Header(Distance (multiples of the camera focus distance))]
        _NearDistance ("Full Strength Until", Float) = 1.25
        _CreaseFadeDistance ("Creases Gone At", Float) = 2.2
        _FadeDistance ("Silhouettes Faded At", Float) = 3.5
        _FarStrength ("Far Silhouette Strength", Range(0, 1)) = 0.35
        _FallbackFocus ("Focus When No Camera Sets It", Float) = 12
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            Name "ToonOutline"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            // Global; unset = 0 = outlines on.
            float _ToonOutlineDisabled;
            half4 _OutlineColor;
            float _Thickness;
            float _DepthThreshold;
            float _NormalThreshold;
            float _GrazingBoost;
            float _NearDistance;
            float _CreaseFadeDistance;
            float _FadeDistance;
            float _FarStrength;
            float _FallbackFocus;
            // Global, set by the follow camera: distance to what it looks at (grows as the player grows).
            float _ToonOutlineFocusDistance;

            float EyeDepth(float2 uv)
            {
                return LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                float rawCenter = SampleSceneDepth(uv);
                float center = LinearEyeDepth(rawCenter, _ZBufferParams);
                float focus = _ToonOutlineFocusDistance > 0.0 ? _ToonOutlineFocusDistance : _FallbackFocus;
                float nearDistance = _NearDistance * focus;
                float far = smoothstep(nearDistance, _FadeDistance * focus, center);

                // Thickness is authored at 1080p and scales with resolution; far away it thins towards 1px.
                float pixels = max(1.0, lerp(_Thickness * (_ScreenSize.y / 1080.0), 1.0, far));
                float2 o = pixels * _ScreenSize.zw;
                float2 uvA = uv + float2(o.x, o.y);
                float2 uvB = uv - float2(o.x, o.y);
                float2 uvC = uv + float2(o.x, -o.y);
                float2 uvD = uv + float2(-o.x, o.y);

                // Grazing angle: the steeper we look along a surface, the bigger its natural depth gradient.
                float3 normal = SampleSceneNormals(uv);
                float3 positionWS = ComputeWorldSpacePosition(uv, rawCenter, UNITY_MATRIX_I_VP);
                float3 view = normalize(_WorldSpaceCameraPos - positionWS);
                float grazing = 1.0 - saturate(dot(normal, view));
                float depthThreshold = _DepthThreshold * (1.0 + grazing * grazing * _GrazingBoost);

                // Roberts cross on linear depth, relative to the center depth so distance doesn't matter.
                float dA = EyeDepth(uvA), dB = EyeDepth(uvB), dC = EyeDepth(uvC), dD = EyeDepth(uvD);
                float depthEdge = sqrt((dA - dB) * (dA - dB) + (dC - dD) * (dC - dD)) / max(center, 0.001);

                float3 nA = SampleSceneNormals(uvA), nB = SampleSceneNormals(uvB);
                float3 nC = SampleSceneNormals(uvC), nD = SampleSceneNormals(uvD);
                float normalEdge = sqrt(dot(nA - nB, nA - nB) + dot(nC - nD, nC - nD));

                // Soft thresholds (anti-aliased lines).
                float silhouette = smoothstep(depthThreshold, depthThreshold * 2.0, depthEdge);
                float crease = smoothstep(_NormalThreshold, _NormalThreshold * 1.5, normalEdge);
                crease *= 1.0 - smoothstep(nearDistance, _CreaseFadeDistance * focus, center);

                float edge = max(silhouette, crease) * lerp(1.0, _FarStrength, far);
                edge *= 1.0 - _ToonOutlineDisabled;
                return half4(lerp(color.rgb, _OutlineColor.rgb, edge * _OutlineColor.a), color.a);
            }
            ENDHLSL
        }
    }
}
