using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트 사용을 위해 필요

public class ShaderController : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("셰이더가 적용된 머티리얼을 넣어주세요.")]
    public Material targetMaterial;

    [Header("UI Sliders")]
    public Slider redSatSlider;
    public Slider greenSatSlider;
    public Slider blueSatSlider;
    public Slider brightnessSlider;

    // 셰이더 프로퍼티 이름을 미리 ID로 변환하여 성능을 최적화합니다.
    private int _rgbSatId;
    private int _brightnessId;

    void Awake()
    {
        // 셰이더 내부의 변수 이름을 ID로 가져옵니다.
        _rgbSatId = Shader.PropertyToID("_RGBSat");
        _brightnessId = Shader.PropertyToID("_Brightness");
    }

    void Start()
    {
        // 각 슬라이더에 값이 변할 때 실행될 함수를 연결합니다.
        redSatSlider.onValueChanged.AddListener(delegate { UpdateShader(); });
        greenSatSlider.onValueChanged.AddListener(delegate { UpdateShader(); });
        blueSatSlider.onValueChanged.AddListener(delegate { UpdateShader(); });
        brightnessSlider.onValueChanged.AddListener(delegate { UpdateShader(); });

        // 시작 시 초기 값 적용
        UpdateShader();
    }

    public void UpdateShader()
    {
        if (targetMaterial == null) return;

        // 1. R, G, B 채도 값을 Vector4로 묶어서 전달합니다.
        Vector4 rgbSat = new Vector4(
            redSatSlider.value,
            greenSatSlider.value,
            blueSatSlider.value,
            1.0f // W값은 사용하지 않으므로 기본값 1
        );
        targetMaterial.SetVector(_rgbSatId, rgbSat);

        // 2. 명도 값을 전달합니다.
        targetMaterial.SetFloat(_brightnessId, brightnessSlider.value);
    }
}