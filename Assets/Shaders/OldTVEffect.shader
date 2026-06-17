Shader "Custom/UI/OldTVOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _ScanlineColor ("Scanline Color", Color) = (0, 0, 0, 0.6)
        _ScanlineCount ("Scanline Count", Float) = 250
        _ScanlineSpeed ("Scanline Speed", Float) = 5
        
        // Transparansi putih sangat dikurangi (alpha 0.03) agar lebih membaur
        _NoiseColor ("Noise Color", Color) = (1, 1, 1, 0.03)
        _NoiseSpeed ("Noise Speed", Float) = 50
        
        _VignetteIntensity ("Vignette Intensity", Float) = 1.2
        _Curvature ("CRT Curvature", Float) = 4.0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _TextureSampleAdd; 
            
            float4 _ScanlineColor;
            float _ScanlineCount;
            float _ScanlineSpeed;
            
            float4 _NoiseColor;
            float _NoiseSpeed;
            
            float _VignetteIntensity;
            float _Curvature;

            // Noise statik yang lebih halus
            float random(float2 uv)
            {
                // Sedikit membulatkan UV agar noisenya kotak-kotak halus (retro) tidak tajam per-pixel
                float2 uvScaled = floor(uv * 400.0) / 400.0;
                return frac(sin(dot(uvScaled, float2(12.9898, 78.233))) * 43758.5453123);
            }

            // Fungsi untuk membuat efek cembung TV Tabung (CRT)
            float2 crt_curve(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / float2(_Curvature, _Curvature);
                uv = uv + uv * offset * offset;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 tint = IN.color;
                
                // Menerapkan efek cembung pada kordinat layar
                float2 curvedUV = crt_curve(IN.texcoord);
                
                // Jika UV melengkung keluar batas, jadikan area tersebut warna hitam (Border TV)
                if (curvedUV.x < 0.0 || curvedUV.x > 1.0 || curvedUV.y < 0.0 || curvedUV.y > 1.0)
                {
                    return float4(0, 0, 0, 1) * tint;
                }

                float4 finalColor = float4(0,0,0,0);

                // --- 1. SCANLINES ---
                // Gunakan curvedUV agar garis scanline ikut melengkung cembung
                float scanline = sin((curvedUV.y - _Time.y * _ScanlineSpeed) * _ScanlineCount * 3.14159);
                scanline = (scanline * 0.5) + 0.5;
                
                // --- 2. NOISE ---
                // Gunakan curvedUV agar semutnya juga melengkung
                float noise = random(curvedUV + _Time.y * _NoiseSpeed);
                
                // --- KOMBINASI ---
                finalColor += _NoiseColor * noise;
                finalColor = lerp(finalColor, _ScanlineColor, (1.0 - scanline) * _ScanlineColor.a);
                
                // --- 3. VIGNETTE ---
                // Gunakan curvedUV agar gelapnya mengikuti cembungan
                float2 distFromCenter = curvedUV - 0.5;
                float vignette = 1.0 - dot(distFromCenter, distFromCenter) * _VignetteIntensity;
                vignette = saturate(vignette);
                
                float vignetteAlpha = (1.0 - vignette) * 0.8;
                
                // Terapkan vignette 
                finalColor.rgb = lerp(finalColor.rgb, float3(0,0,0), vignetteAlpha);
                finalColor.a = saturate(finalColor.a + vignetteAlpha);
                
                // Kalikan dengan warna panel UI
                finalColor *= tint;

                return finalColor;
            }
            ENDCG
        }
    }
}
