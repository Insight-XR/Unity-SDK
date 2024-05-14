Shader "Custom/ConcentricCirclesShader"
{
    Properties
    {
        _MainTEX ("Base (RGB)", 2D) = "white" {}
        
        // Properties for the first circle
        _FirstCircleColor ("First Circle Color", Color) = (1, 0, 0)
        _FirstCircleRadius ("First Circle Radius", Range(0, 100)) = 10
        _FirstCircleBorder ("First Circle Border", Range(0, 100)) = 12
        
        // Properties for the second circle
        _SecondCircleColor ("Second Circle Color", Color) = (0, 1, 0)
        _SecondCircleRadius ("Second Circle Radius", Range(0, 500)) = 150
        _SecondCircleBorder ("Second Circle Border", Range(0, 100)) = 12
        
        // Properties for the third circle
        _ThirdCircleColor ("Third Circle Color", Color) = (0, 0, 1)
        _ThirdCircleRadius ("Third Circle Radius", Range(0, 500)) = 225
        _ThirdCircleBorder ("Third Circle Border", Range(0, 100)) = 12
        
        // Property for the center position
        _CenterPosition ("Center Position", Vector) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        
        // Properties for the first circle
        fixed3 _FirstCircleColor;
        float _FirstCircleRadius;
        float _FirstCircleBorder;
        
        // Properties for the second circle
        fixed3 _SecondCircleColor;
        float _SecondCircleRadius;
        float _SecondCircleBorder;
        
        // Properties for the third circle
        fixed3 _ThirdCircleColor;
        float _ThirdCircleRadius;
        float _ThirdCircleBorder;
        
        // Property for the center position
        float4 _CenterPosition;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            half4 c = tex2D (_MainTex, IN.uv_MainTex);

            ///////////////////////////0TH ONE
            //////////////////////////
            ///////////////////////////
            /////////////////////////
            float distToFirstCircle = distance(_CenterPosition.xyz, IN.worldPos);
            float distToSecondCircle = distance(_CenterPosition.xyz, IN.worldPos);
            float distToThirdCircle = distance(_CenterPosition.xyz, IN.worldPos);
            
            if (distToFirstCircle > 0 && distToFirstCircle < (_FirstCircleRadius + _FirstCircleBorder))
            {
                o.Albedo = _FirstCircleColor;
            }
            else if (distToSecondCircle > _FirstCircleRadius && distToSecondCircle < (_SecondCircleRadius + _SecondCircleBorder))
            {
                o.Albedo = _SecondCircleColor;
            }
            else if (distToThirdCircle > _SecondCircleRadius && distToThirdCircle < (_ThirdCircleRadius + _ThirdCircleBorder))
            {
                o.Albedo = _ThirdCircleColor;
            }
            else
            {
                o.Albedo = c.rgb;
            }
            ///////////////////////////////////
            /////////////////////////////////

            /////////////////////////
            
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
