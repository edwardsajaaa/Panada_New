Shader "UI/PixelTransitionUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1)
        
        [Header(Pixel Transition Settings)]
        _Progress ("Transition Progress", Range(0, 1)) = 0.0
        _PixelSize ("Pixel Size", Float) = 40.0
        
        [KeywordEnum(Diamond, Checkerboard, Slide)] _TransitionType ("Transition Mode", Float) = 0
        _Invert ("Invert Transition", Range(0,1)) = 0

        // Required for UI
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma multi_compile_local _TRANSITIONTYPE_DIAMOND _TRANSITIONTYPE_CHECKERBOARD _TRANSITIONTYPE_SLIDE

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
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            float _Progress;
            float _PixelSize;
            float _Invert;

            // Random function for checkerboard/mosaic variation
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123);
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
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                // Get screen aspect ratio to keep pixels square
                float aspect = _ScreenParams.x / _ScreenParams.y;
                
                // Pixelate coordinates
                float2 uv = IN.texcoord;
                uv.x *= aspect;
                
                // Snap UVs to pixel grid
                float2 pixelUV = floor(uv * _PixelSize) / _PixelSize;
                
                float alphaMask = 1.0;
                float prog = _Progress;

                #if _TRANSITIONTYPE_DIAMOND
                    // Diamond wipe from center
                    float2 center = float2(0.5 * aspect, 0.5);
                    float dist = abs(pixelUV.x - center.x) + abs(pixelUV.y - center.y);
                    
                    // Scale progress so 0 is fully closed (black), 1 is fully open (transparent)
                    // Max dist from center is roughly (0.5 * aspect) + 0.5
                    float maxDist = (0.5 * aspect) + 0.5;
                    float wipeThreshold = prog * maxDist * 1.5;
                    
                    if (dist > wipeThreshold)
                    {
                        alphaMask = 1.0; // Covered
                    }
                    else
                    {
                        alphaMask = 0.0; // Revealed
                    }
                #elif _TRANSITIONTYPE_CHECKERBOARD
                    // Checkerboard random dissolve
                    float randVal = random(pixelUV);
                    if (randVal < prog)
                    {
                        alphaMask = 0.0;
                    }
                    else
                    {
                        alphaMask = 1.0;
                    }
                #else // _TRANSITIONTYPE_SLIDE
                    // Vertical pixel slide
                    float slideProg = prog * 1.5;
                    // Add slight random offset per column to make it look jagged
                    float offset = (random(float2(pixelUV.x, 0)) - 0.5) * 0.2;
                    if (pixelUV.y > slideProg + offset)
                    {
                        alphaMask = 1.0;
                    }
                    else
                    {
                        alphaMask = 0.0;
                    }
                #endif

                // Invert the mask if going backwards (Out transition vs In transition)
                if (_Invert > 0.5)
                {
                    alphaMask = 1.0 - alphaMask;
                }

                color.a *= alphaMask;

                return color;
            }
        ENDCG
        }
    }
}
