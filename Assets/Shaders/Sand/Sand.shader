Shader "Unlit/Sand"
{
    Properties
    {
        _SandTex("Sand Texture", 2D) = "" {}
        _WetSandTex("Wet Sand Texture", 2D) = "" {}
        _DispTex ("Displacement Texture", 2D) = "" {}
        _NoiseTex("Noise Texture", 2D) = "" {}
        _DispAmt ("Displacement Amount", Range(0.0, 1.0)) = 1
        _NoiseSize ("Noise Size", Float) = 1
        _NoiseDispAmt ("Noise Displacement Amount", Float) = 1
        _TessAmt ("Tesselation Amount", Integer) = 10
        [HideInInspector] _MouseDown ("Mouse Down", Float) = 0
        _Test ("Test", Range(0, 1)) = 0
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
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Autolight.cginc"
            #include "UnityLightingCommon.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 disp : TEXCOORD1;
                float4 _ShadowCoord : TEXCOORD2;
                float3 normal : NORMAL;
            };

            struct TessellationControlPoint {
                float3 vertex : INTERNALTESSPOS;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            TessellationControlPoint Vertex(Attributes input) {
                TessellationControlPoint output;
                output.vertex = input.vertex;
                output.uv = input.uv;
                output.normal = input.normal;

                return output;
            }

            sampler2D _DispTex;
            sampler2D _SandTex;
            sampler2D _WetSandTex;
            sampler2D _NoiseTex;
            float4 _SandTex_ST;
            float _DispAmt;
            float _NoiseSize;
            float _NoiseDispAmt;
            float _MouseDown;
            float _Test;
            float _TessAmt;
            
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
                f.edge[0] = _TessAmt;
                f.edge[1] = _TessAmt;
                f.edge[2] = _TessAmt;
                f.inside = _TessAmt;
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

                // get uvs
                output.uv = BARYCENTRIC_INTERPOLATE(uv);
                
                // get normals
                output.normal = BARYCENTRIC_INTERPOLATE(normal);
                output.normal = UnityObjectToWorldNormal(output.normal);

                // get vertex pos
                float3 interpolated_vertex = BARYCENTRIC_INTERPOLATE(vertex);

                // get displacement from disptex
                float4 disp = tex2Dlod(_DispTex, float4(output.uv, 0, 0));
                output.uv = TRANSFORM_TEX(output.uv, _SandTex);

                // get perlin noise from noisetex
                float noise = tex2Dlod(_NoiseTex, float4(output.uv * _NoiseSize, 0, 0)) - 0.25;

                output.disp = disp;

                output.vertex = UnityObjectToClipPos(interpolated_vertex - float3(0, lerp(disp.x, -disp.y * 2, disp.y != 0) * _DispAmt + noise * _NoiseDispAmt, 0));
                output._ShadowCoord = ComputeScreenPos(output.vertex);

                #if UNITY_PASS_SHADOWCASTER
                    // Applying the bias prevents artifacts from appearing on the surface.
                    o.pos = UnityApplyLinearShadowBias(o.pos);
                #endif

                return output;

            }

            float4 Fragment(Interpolators i) : SV_Target
            {
                float shadow = SHADOW_ATTENUATION(i);
                float NdotL = saturate(saturate(dot(i.normal, _WorldSpaceLightPos0)) + 0.5) * shadow;

                float3 ambient = ShadeSH9(float4(i.normal, 1));
                float4 lightIntensity = NdotL * _LightColor0 + float4(ambient, 1);
                float4 testColors = tex2D(_DispTex, i.uv * 0.2);
                float4 col = tex2D(_SandTex, i.uv) * (1 - i.disp.x * 0.5) * lightIntensity;
                return lerp(testColors, col, 1-_Test);
            }
            ENDCG
        }

        // Add below the existing Pass.
        Pass
        {
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

            struct v2f {
                V2F_SHADOW_CASTER;
            };

            v2f vert(appdata_base v) {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;

            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }

            ENDCG
        }
    }
}
