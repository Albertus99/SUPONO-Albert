// Full-screen ink outline for URP's Full Screen Pass renderer feature (requires Depth + Normal).
// Draws lines where depth jumps (silhouettes) or normals turn sharply (creases), fading with distance.
Shader "Supono/ToonOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0.13, 0.1, 0.17, 0.85)
        _Thickness ("Thickness (px)", Range(0.5, 4)) = 1.25
        _DepthThreshold ("Depth Threshold", Range(0.001, 0.3)) = 0.06
        _NormalThreshold ("Normal Threshold", Range(0.05, 2)) = 0.6
        _FadeDistance ("Fade Distance", Float) = 90
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

            // Global, set by the debug menu; unset = 0 = outlines on.
            float _ToonOutlineDisabled;
            half4 _OutlineColor;
            float _Thickness;
            float _DepthThreshold;
            float _NormalThreshold;
            float _FadeDistance;

            float EyeDepth(float2 uv)
            {
                return LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                // Thickness is authored at 1080p and scales with resolution so lines read the same everywhere.
                float2 o = _Thickness * (_ScreenSize.y / 1080.0) * _ScreenSize.zw;
                float2 uvA = uv + float2(o.x, o.y);
                float2 uvB = uv - float2(o.x, o.y);
                float2 uvC = uv + float2(o.x, -o.y);
                float2 uvD = uv + float2(-o.x, o.y);

                // Roberts cross on linear depth, relative to the center depth so distance doesn't matter.
                float center = EyeDepth(uv);
                float dA = EyeDepth(uvA), dB = EyeDepth(uvB), dC = EyeDepth(uvC), dD = EyeDepth(uvD);
                float depthEdge = sqrt((dA - dB) * (dA - dB) + (dC - dD) * (dC - dD)) / max(center, 0.001);

                float3 nA = SampleSceneNormals(uvA), nB = SampleSceneNormals(uvB);
                float3 nC = SampleSceneNormals(uvC), nD = SampleSceneNormals(uvD);
                float normalEdge = sqrt(dot(nA - nB, nA - nB) + dot(nC - nD, nC - nD));

                float edge = max(step(_DepthThreshold, depthEdge), step(_NormalThreshold, normalEdge));
                edge *= saturate(1.0 - center / _FadeDistance) * (1.0 - _ToonOutlineDisabled);
                return half4(lerp(color.rgb, _OutlineColor.rgb, edge * _OutlineColor.a), color.a);
            }
            ENDHLSL
        }
    }
}
