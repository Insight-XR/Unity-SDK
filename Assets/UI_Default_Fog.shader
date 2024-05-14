Shader "Custom/ColorChange"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)
        _ColorChangeTex ("Color Change Texture", 2D) = "white" {}
        _ColorChangeStrength ("Color Change Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        sampler2D _ColorChangeTex;
        fixed4 _Color;
        float _ColorChangeStrength;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Sample the main texture
            fixed4 mainTexColor = tex2D(_MainTex, IN.uv_MainTex);

            // Sample the color change texture
            fixed4 colorChange = tex2D(_ColorChangeTex, IN.uv_MainTex);

            // Calculate the final color with the color change effect
            fixed4 finalColor = lerp(mainTexColor, colorChange, _ColorChangeStrength);

            // Apply the base color
            finalColor *= _Color;

            // Output the final color
            o.Albedo = finalColor.rgb;
            o.Alpha = finalColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
