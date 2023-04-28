Shader "Unlit/Sand"
{
    Properties
    {
        _SandTex("Sand Texture", 2D) = "" {}
        _WetSandTex("Wet Sand Texture", 2D) = "" {}
        _DispTex ("Displacement Texture", 2D) = "" {}
        _DispAmt ("Displacement Amount", Range(0.0, 1.0)) = 1
        [HideInInspector] _MouseDown ("Mouse Down", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }

        Pass
        {
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex Vertex
            #pragma hull Hull
            #pragma domain Domain
            #pragma fragment Fragment

            #include "UnityCG.cginc"

            #pragma multi_compile_fwdbase
            #include "AutoLight.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 disp : TEXCOORD1;
                LIGHTING_COORDS(2,3)
            };

            struct TessellationControlPoint {
                float3 vertex : INTERNALTESSPOS;
                float2 uv : TEXCOORD0;
            };

            TessellationControlPoint Vertex(Attributes input) {
                TessellationControlPoint output;
                output.vertex = input.vertex;
                output.uv = input.uv;

                return output;
            }
            
            // The hull function runs once per vertex. You can use it to modify vertex
            // data based on values in the entire triangle
            [domain("tri")] // Signal we're inputting triangles
            [outputcontrolpoints(3)] // Triangles have three points
            [outputtopology("triangle_cw")] // Signal we're outputting triangles
            [patchconstantfunc("PatchConstantFunction")] // Register the patch constant function
            [partitioning("integer")] // Select a partitioning mode: integer, fractional_odd, fractional_even or pow2
            TessellationControlPoint Hull(
                InputPatch<TessellationControlPoint, 3> patch, // Input triangle
                uint id : SV_OutputControlPointID) { // Vertex index on the triangle

                return patch[id];
            }

            struct TessellationFactors {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            // The patch constant function runs once per triangle, or "patch"
            // It runs in parallel to the hull function
            TessellationFactors PatchConstantFunction(
                InputPatch<TessellationControlPoint, 3> patch) {
                UNITY_SETUP_INSTANCE_ID(patch[0]); // Set up instancing
                // Calculate tessellation factors
                TessellationFactors f;
                f.edge[0] = 10000;
                f.edge[1] = 10000;
                f.edge[2] = 10000;
                f.inside = 10000;
                return f;
            }

            sampler2D _DispTex;
            float _DispAmt;
            float _MouseDown;

            #define BARYCENTRIC_INTERPOLATE(fieldName) \
		            patch[0].fieldName * barycentricCoordinates.x + \
		            patch[1].fieldName * barycentricCoordinates.y + \
		            patch[2].fieldName * barycentricCoordinates.z

            // The domain function runs once per vertex in the final, tessellated mesh
            // Use it to reposition vertices and prepare for the fragment stage
            [domain("tri")] // Signal we're inputting triangles
            Interpolators Domain(
                TessellationFactors factors, // The output of the patch constant function
                OutputPatch<TessellationControlPoint, 3> patch, // The Input triangle
                float3 barycentricCoordinates : SV_DomainLocation) { // The barycentric coordinates of the vertex on the triangle

                Interpolators output;
                output.uv = BARYCENTRIC_INTERPOLATE(uv);

                float3 interpolated_vertex = BARYCENTRIC_INTERPOLATE(vertex);
                float4 disp = tex2Dlod(_DispTex, float4(output.uv, 0, 0));
                output.disp = disp;
                output.pos = UnityObjectToClipPos(interpolated_vertex + float3(0, 0, lerp(disp.x, -disp.y * 2, disp.y != 0)) * _DispAmt);


                TRANSFER_VERTEX_TO_FRAGMENT(output);
                

                return output;

            }

            sampler2D _SandTex;
            sampler2D _WetSandTex;

            float4 Fragment(Interpolators i) : SV_Target
            {
                float attenuation = LIGHT_ATTENUATION(i);
                return tex2D(_SandTex, i.uv) * (1-i.disp.x * 0.5) * attenuation;
            }
            ENDCG
        }
    }

    Fallback "VertexLit"
}
