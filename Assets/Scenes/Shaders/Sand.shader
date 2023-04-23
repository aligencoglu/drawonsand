Shader "Unlit/Sand"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma target 5.0
            #pragma vertex Vertex
            #pragma hull Hull
            #pragma domain Domain
            #pragma fragment Fragment

            #include "UnityCG.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
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
                f.edge[0] = 100;
                f.edge[1] = 100;
                f.edge[2] = 100;
                f.inside = 100;
                return f;
            }

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

                output.vertex = UnityObjectToClipPos(BARYCENTRIC_INTERPOLATE(vertex));
                output.uv = BARYCENTRIC_INTERPOLATE(uv);

                return output;

            }

            float4 Fragment(Interpolators i) : SV_Target
            {
                return float4(1, 1, 1, 1);
            }
            ENDCG
        }
    }
}
