Shader "Hidden/Custom/URPOldTV"
{
    Properties
    {
        // Full Screen Pass Renderer Feature menggunakan _MainTex sebagai input layar
        _MainTex ("Screen Texture", 2D) = "white" {}
        
        _ScanlineCount ("Scanline Count", Float) = 250
        _ScanlineSpeed ("Scanline Speed", Float) = 5
        _NoiseSpeed ("Noise Speed", Float) = 50
        _VignetteIntensity ("Vignette Intensity", Float) = 1.2
        _Curvature ("CRT Curvature", Float) = 4.0
        _Intensity ("Effect Intensity", Range(0, 1)) = 1.0
        _ScanlineColor ("Scanline Color", Color) = (0, 0, 0, 0.6)
        _NoiseColor ("Noise Color", Color) = (1, 1, 1, 0.05)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off
        Cull Off
        ZTest Always

        Pass
        {
            Name "OldTVEffect"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Parameter efek TV
            float _ScanlineCount;
            float _ScanlineSpeed;
            float _NoiseSpeed;
            float _VignetteIntensity;
            float _Curvature;
            float _Intensity;
            float4 _ScanlineColor;
            float4 _NoiseColor;

            // Texture layar dari Full Screen Pass
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float random(float2 uv)
            {
                float2 uvSnapped = floor(uv * 400.0) / 400.0;
                return frac(sin(dot(uvSnapped, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float2 crt_curve(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / _Curvature;
                uv = uv + uv * offset * offset;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                
                // Jika intensity 0, kembalikan layar asli tanpa distorsi apapun
                if (_Intensity <= 0.0)
                {
                    return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                }
                
                // Lerp antara UV asli dan UV cembung berdasarkan Intensity
                // Saat intensity mendekati 0, distorsi berkurang; saat 1, distorsi penuh
                float2 curvedUV = lerp(uv, crt_curve(uv), _Intensity);

                // Pinggiran layar cembung → hitam solid (border TV tabung)
                if (curvedUV.x < 0.0 || curvedUV.x > 1.0 || curvedUV.y < 0.0 || curvedUV.y > 1.0)
                {
                    return half4(0, 0, 0, 1);
                }

                // Baca piksel asli dari layar dengan UV yang sudah melengkung
                // FullScreenPassRendererFeature menyediakan layar via _BlitTexture (dari Blit.hlsl)
                half4 screenColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, curvedUV);

                half4 finalColor = screenColor;

                // --- Scanlines ---
                float scanline = sin((curvedUV.y - _Time.y * _ScanlineSpeed) * _ScanlineCount * 3.14159);
                scanline = saturate((scanline * 0.5) + 0.5);

                // --- Noise / Semut TV ---
                float noise = random(curvedUV + _Time.y * _NoiseSpeed);

                // Gabungkan efek
                finalColor.rgb += _NoiseColor.rgb * noise * _NoiseColor.a;
                finalColor.rgb = lerp(finalColor.rgb, _ScanlineColor.rgb, (1.0 - scanline) * _ScanlineColor.a);

                // --- Vignette (sudut layar gelap) ---
                float2 d = curvedUV - 0.5;
                float vignette = saturate(1.0 - dot(d, d) * _VignetteIntensity);
                float vignetteAlpha = (1.0 - vignette) * 0.85;
                finalColor.rgb = lerp(finalColor.rgb, half3(0, 0, 0), vignetteAlpha);

                // Blend antara layar asli dan efek TV berdasarkan Intensity
                return lerp(screenColor, finalColor, _Intensity);
            }
            ENDHLSL
        }
    }
}
