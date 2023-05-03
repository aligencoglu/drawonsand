Shader "Unlit/Water"
{
    Properties
    {
        _NoiseTexture("Noise Texture", 2D) = "" {}
        _DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 1)
        _BubbleColor("Bubble Color", Color) = (0.891, 0.972, 0.969, 0.9)
        _WaveColor("Wave Color", Color) = (0.891, 0.972, 0.969, 0.9)
        _DepthMaxDistance("Depth Maximum Distance", Float) = 1
        _BubbleDepth("Bubble Depth", Float) = 0.1
        _BubbleFullColorStart("Bubble Full Color Start", Float) = 0.05
        _BubbleWaveNoiseSize("Bubble Wave Noise Size", Float) = 2
        _WaveSize("Wave Size", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenSpace : TEXCOORD1;
            };

            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;
            float4 _BubbleColor;
            float4 _WaveColor;
            float _DepthMaxDistance;
            float _BubbleDepth;
            float _BubbleFullColorStart;
            float _BubbleWaveNoiseSize;
            float _WaveSize;
            sampler2D _CameraDepthTexture;
            sampler2D _NoiseTexture;

            v2f vert (appdata v)
            {
                v2f o;

                // adding waves
                float3 vertex = v.vertex;


                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenSpace = ComputeScreenPos(o.vertex);
                return o;
            }

            // inverse lerp function from Freya Holmer
            float InvLerp(float a, float b, float v) {
                return (v - a) / (b - a);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 screenSpaceUV = i.screenSpace.xy / i.screenSpace.w;

                // LinearEyeDepth gives camera space unit distance from the camera
                // Linear01Depth gives LinearEyeDepth but normalized so that the near plane is 0 and the far plane is 1.
                float depthTex = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenSpaceUV);
                float depthEye = LinearEyeDepth(depthTex);
                float depth01 = Linear01Depth(depthTex);
                

                // get perlin noise
                float noise = tex2D(_NoiseTexture, i.uv * _BubbleWaveNoiseSize + float2(0, _Time.y * 0.1));

                // get cam space depth where the surface level is 0
                float depthFromSurface = depthEye - i.screenSpace.w;

                // remap depth so that surface level is 0 and max depth is 1
                float depthAdjusted = saturate(InvLerp(0, _DepthMaxDistance, depthFromSurface));

                // surface bubble mask
                float bubbleMask = saturate(1-InvLerp(_BubbleFullColorStart, _BubbleDepth, depthFromSurface + noise));

                float2 waveUV = i.uv * (1 / _WaveSize);
                float waveNoise = tex2D(_NoiseTexture, waveUV + sin(_Time.y) * 0.1);
                float waveMask = (waveNoise > 0.4 && waveNoise < 0.4 + fwidth(i.uv * (1 / _WaveSize)) * 20);
                //return float4((waveMask).xxx, 1);


                float4 depthAdjustedWaterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, depthAdjusted);
                float4 waterPlusWave = lerp(depthAdjustedWaterColor, _WaveColor, waveMask);

                float4 col = lerp(waterPlusWave, _BubbleColor, bubbleMask);
                return col;
            }
            ENDCG
        }
    }
}
