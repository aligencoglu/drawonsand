Shader "CustomRenderTexture/SandDisp"
{
    Properties
    {
        [HideInInspector] _MousePos("Mouse Position", Vector) = (0, 0, 0, 0)
        [HideInInspector] _MouseVel("Mouse Velocity", Vector) = (0, 0, 0, 0)
        [HideInInspector] _MouseDown("Mouse Down", Float) = 0
        _Test ("Test", Float) = 0
        _Falloff ("Falloff", Range(0.0, 0.3)) = 0
        _BrushWeight ("Brush Weight", Range(0.0, 5.0)) = 1
        _BrushPixelWidth ("Brush Pixel Width", Float) = 10
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

            float4 _MousePos;
            float4 _MouseVel;
            float _MouseDown;
            float _Falloff;
            float _BrushPixelWidth;
            float _BrushWeight;
            float _Test;

            float4 frag(v2f_init_customrendertexture IN) : COLOR
            {
                float4 prevCol = tex2D(_SelfTexture2D, IN.texcoord.xy);
                float distToMouse = length(_MousePos.xy - IN.texcoord.xy);
                float radius = fwidth(IN.texcoord.x) * _BrushPixelWidth;

                // create the through
                float normalizedDist = distToMouse * (1 / radius) * 0.5;
                float mouseMask = (normalizedDist) * (normalizedDist) - 0.25;
                mouseMask *= (normalizedDist < 0.5);
                mouseMask *= _MouseDown;

                // create the bump
                float outsideMask = (normalizedDist > 0.5) * (normalizedDist < 1);
                outsideMask *= 0.25 - 4 * (normalizedDist - 0.75) * (normalizedDist - 0.75);
                outsideMask *= _MouseDown;

                float3 prevColFaded = prevCol.xyz - float3(1, 1, 1) * _Falloff;
                float3 col = saturate(prevColFaded);
                //float velMask = -dot(_MousePos.xy - IN.texcoord.xy, _MouseVel.xy);
                float3 mouseMaskReadied = (-mouseMask * _BrushWeight).xxx;
                float3 outsideMaskReadied = (-outsideMask * _BrushWeight).xxx;


                


                if (_MouseDown) {
                    col.x = lerp(col.x, mouseMaskReadied, normalizedDist < 0.5 && col.x < mouseMaskReadied.x);
                    //col = lerp(col, outsideMaskReadied, normalizedDist > 0.5 && normalizedDist < 1 && col.x >= 0);
                }

                return float4(col.x, lerp(fwidth(col.x), 0, normalizedDist < 0.5 && _MouseDown), 0, 1);
            }
           
            ENDCG
        }
    }
}