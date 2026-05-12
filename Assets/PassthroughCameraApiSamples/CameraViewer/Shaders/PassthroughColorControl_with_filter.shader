Shader "Meta/PCA/PassthroughColorControl_Filtered" {
    Properties{
        _MainTex("Camera Texture", 2D) = "white" {}

    // 기존 속성
    _RGBSat("RGB Saturation (XYZ)", Vector) = (1, 1, 1, 1)
    _Brightness("Brightness", Float) = 1.0

        // 2번 방식: 필터 효과를 위한 새 속성
        _FilterColor("Filter Color (Blue/Warm etc.)", Color) = (1, 1, 1, 1)
        _FilterStrength("Filter Strength", Range(0, 1)) = 0.3
    }

        SubShader{
            Tags {"RenderType" = "Opaque"}
            LOD 100

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_instancing

                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                float4 _RGBSat;
                float _Brightness;

                // 필터 변수 선언
                fixed4 _FilterColor;
                float _FilterStrength;

                v2f vert(appdata v) {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // 1. 카메라 텍스처 샘플링 (상하/좌우 반전 보정)
                float2 uv = i.uv;
                uv.y = 1 - uv.y;
                uv.x = 1 - uv.x;
                fixed4 col = tex2D(_MainTex, uv);

                // 2. 휘도(Luminance) 계산
                float lum = dot(col.rgb, float3(0.299, 0.587, 0.114));

                // 3. R, G, B 채널별 개별 채도 조절
                float3 processedRGB;
                processedRGB.r = lerp(lum, col.r, _RGBSat.x);
                processedRGB.g = lerp(lum, col.g, _RGBSat.y);
                processedRGB.b = lerp(lum, col.b, _RGBSat.z);

                // 4. 전체 명도(Brightness) 적용
                processedRGB *= _Brightness;

                // 5. [추가] Overlay 블렌딩 모드 구현
                // 공식: Base < 0.5 ? (2 * Base * Blend) : (1 - 2 * (1 - Base) * (1 - Blend))
                float3 overlay;

                // Red Channel
                overlay.r = processedRGB.r < 0.5 ?
                    (2.0 * processedRGB.r * _FilterColor.r) :
                    (1.0 - 2.0 * (1.0 - processedRGB.r) * (1.0 - _FilterColor.r));

                // Green Channel
                overlay.g = processedRGB.g < 0.5 ?
                    (2.0 * processedRGB.g * _FilterColor.g) :
                    (1.0 - 2.0 * (1.0 - processedRGB.g) * (1.0 - _FilterColor.g));

                // Blue Channel
                overlay.b = processedRGB.b < 0.5 ?
                    (2.0 * processedRGB.b * _FilterColor.b) :
                    (1.0 - 2.0 * (1.0 - processedRGB.b) * (1.0 - _FilterColor.b));

                // 6. 원본(채도 조절본)과 필터 적용본을 강도(_FilterStrength)에 따라 합성
                float3 finalRGB = lerp(processedRGB, overlay, _FilterStrength);

                return fixed4(finalRGB, 1.0);
            }
            ENDCG
        }
    }
}