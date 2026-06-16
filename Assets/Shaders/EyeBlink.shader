Shader "UI/EyeBlink"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Blink ("Blink Amount", Range(0, 1)) = 0
        _Color ("Blink Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"}
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
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Blink;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Normalize UV coordinate to center (-0.5 to 0.5)
                float2 uv = IN.texcoord - 0.5; 
                
                // Buat lengkungan seperti kelopak mata
                float curve = uv.x * uv.x * 0.8;
                
                // 0 = mata terbuka penuh, 1 = mata tertutup penuh (hitam)
                float openAmount = 1.0 - _Blink;
                
                // Ambang batas kelopak mata
                float threshold = openAmount * 0.55 - curve * openAmount;
                
                // Buat tepian sedikit blur (anti-aliasing)
                float edge = 0.05;
                float alpha = smoothstep(threshold - edge, threshold, abs(uv.y));
                
                // Kembalikan warna (biasanya hitam) dengan alpha dari kelopak mata
                return fixed4(IN.color.rgb, alpha * IN.color.a);
            }
            ENDCG
        }
    }
}
