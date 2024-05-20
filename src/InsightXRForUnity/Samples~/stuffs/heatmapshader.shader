Shader "Custom/HeatmapShader"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _HeatmapTex("Heatmap Texture", 2D) = "white" {}

        _Color0("Color 0", Color) = (0, 0, 0, 1)
        _Color1("Color 1", Color) = (0, .9, .2, 1)
        _Color2("Color 2", Color) = (.9, 1, .3, 1)
        _Color3("Color 3", Color) = (.9, .7, .1, 1)
        _Color4("Color 4", Color) = (1, 0, 0, 1)

        _Range0("Range 0", Range(0, 1)) = 0.
        _Range1("Range 1", Range(0, 1)) = 0.25
        _Range2("Range 2", Range(0, 1)) = 0.5
        _Range3("Range 3", Range(0, 1)) = 0.75
        _Range4("Range 4", Range(0, 1)) = 1

        _Diameter("Diameter", Range(0, 1)) = 1.0
        _Strength("Strength", Range(.1, 4)) = 1.0
        _PulseSpeed("Pulse Speed", Range(0, 5)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert

        sampler2D _MainTex;
        sampler2D _HeatmapTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_HeatmapTex;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.uv_MainTex = v.texcoord;
            o.uv_HeatmapTex = v.texcoord;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 heatmap = tex2D(_HeatmapTex, IN.uv_HeatmapTex);

            // Blend heatmap with original texture
            fixed4 finalColor = tex + heatmap * _Strength;

            o.Albedo = finalColor.rgb;
            o.Alpha = finalColor.a;
        }
        ENDCG
    }
}
