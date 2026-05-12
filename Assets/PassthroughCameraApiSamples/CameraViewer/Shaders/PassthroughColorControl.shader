Shader "Meta/PCA/PassthroughColorControl" {
    Properties{
        // PassthroughCameraAccess 스크립트가 텍스처를 전달하는 기본 통로
        _MainTex("Camera Texture", 2D) = "white" {}

    // 외부 UI에서 제어할 속성들
    _RGBSat("RGB Saturation (XYZ)", Vector) = (1, 1, 1, 1) // X:R, Y:G, Z:B 채도
    _Brightness("Brightness", Float) = 1.0
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
                    UNITY_VERTEX_OUTPUT_STEREO // VR 스테레오 출력 지원
                };

                sampler2D _MainTex;
                float4 _RGBSat;
                float _Brightness;

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

                // 1. 카메라 텍스처 샘플링 (상하 반전 보정 포함 가능)
                float2 uv = i.uv;
                uv.y = 1 - uv.y;
                uv.x = 1 - uv.x;
                // 필요한 경우 샘플 코드처럼 반전:
                fixed4 col = tex2D(_MainTex, uv);

                // 2. 휘도(Luminance) 계산 - 표준 가중치 사용
                float lum = dot(col.rgb, float3(0.299, 0.587, 0.114));

                // 3. R, G, B 채널별 개별 채도 조절
                // 각 채널을 무채색(lum)과 원본 색상 사이에서 선형 보간(lerp) 합니다.
                float3 finalRGB;
                finalRGB.r = lerp(lum, col.r, _RGBSat.x);
                finalRGB.g = lerp(lum, col.g, _RGBSat.y);
                finalRGB.b = lerp(lum, col.b, _RGBSat.z);

                // 4. 전체 명도(Brightness) 적용
                finalRGB *= _Brightness;

                return fixed4(finalRGB, 1.0);
            }
            ENDCG
        }
    }
}