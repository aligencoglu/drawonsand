Shader "CustomRenderTexture/CanvasTextureShader"
{
    Properties
    {
        _Input("Input", Vector) = (0, 0, 0, 0)
        _Input2("Input 2", Vector) = (0, 0, 0, 0)
        _StrokeWidth("Stroke Width", Float) = 1

        _Clear("Clear", Float) = 0
    }

    SubShader
    {
        Lighting Off
        Blend One Zero

        Pass
        {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"

            #pragma vertex InitCustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            float4 _Input;
            float4 _Input2;
            float _StrokeWidth;
            float _Clear;

            float line_segment(float2 p, float2 a, float2 b) {
                // modified from https://www.shadertoy.com/view/Wlfyzl
                float2 ba = b - a;
                float2 pa = p - a;
                float h = clamp(dot(pa, ba) / dot(ba, ba), 0, 1);
                return 1 - ((length(pa - h * ba) - _StrokeWidth) > 0);
            }

            float4 addShape(float4 input, float4 input2, float2 texcoord) 
            {
                if (length(input) == 0) 
                {
                    return float4(0, 0, 0, 0);
                }
                // circle
                if (input.w == 0) 
                {
                    float2 center = input.xy;
                    float radius = input.z;
                    float distFromCenter = distance(texcoord, input);
                    float circleMask = distFromCenter > radius - _StrokeWidth * 0.5 && distFromCenter < radius + _StrokeWidth;
                    return float4(circleMask, circleMask, circleMask, 1);
                }
                // line
                if (input.w == 1)
                {
                    float2 start = input.xy;
                    float2 end = float2(input.z, input2.x);
                    float lineMask = line_segment(texcoord, start, end);
                    return float4(lineMask, lineMask, lineMask, 1);
                }
                // triangle
                if (input.w == 2) 
                {
                    float2 p1 = input.xy;
                    float2 p2 = float2(input.z, input2.x);
                    float2 p3 = input2.yz;
                    float lineMask = (
                        line_segment(texcoord, p1, p2)
                        || line_segment(texcoord, p2, p3)
                        || line_segment(texcoord, p3, p1)
                    );
                    return float4(lineMask, lineMask, lineMask, 1); 
                }
                // rectangle
                if (input.w == 3)
                {
                    float2 upperLeft = input.xy;
                    float2 lowerLeft = float2(input.x, input2.x);
                    float2 upperRight = float2(input.z, input.y);
                    float2 lowerRight = float2(input.z, input2.x);
                    float lineMask = (
                        line_segment(texcoord, upperLeft, upperRight)
                        || line_segment(texcoord, upperRight, lowerRight)
                        || line_segment(texcoord, lowerRight, lowerLeft)
                        || line_segment(texcoord, lowerLeft, upperLeft)
                    );
                    return float4(lineMask, lineMask, lineMask, 1); 
                }
                return float4(0, 0, 0, 0);
            }

            float4 frag(v2f_init_customrendertexture IN) : COLOR
            {
                float4 updated = tex2D(_SelfTexture2D, IN.texcoord.xy) + addShape(_Input, _Input2, IN.texcoord.xy);
                float4 col = lerp(updated, float4(0, 0, 0, 1), _Clear);
                return saturate( col );
            }
            ENDCG
        }
    }
}