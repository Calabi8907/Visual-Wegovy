using UnityEngine;
using UnityEngine.UI;

public class VASController : MonoBehaviour
{
    [Tooltip("이 VAS가 기록될 인덱스 (0, 1, 2)")]
    public int vasIndex;

    [Tooltip("이 패널에 속한 슬라이더")]
    public Slider vasSlider;

    [Tooltip("다음으로 넘어가는 버튼")]
    public Button nextButton;

    void Start()
    {
        // 버튼 클릭 시 매니저에게 데이터를 보내고 다음 패널로 넘어가도록 이벤트 구독
        nextButton.onClick.AddListener(OnNextClicked);
    }

    private void OnNextClicked()
    {
        // 1. 매니저에게 내 슬라이더 값을 기록하라고 명령
        ExperimentManager.Instance.RecordVAS(vasIndex, vasSlider.value);

        // 2. 매니저에게 다음 패널로 넘어가라고 명령
        ExperimentManager.Instance.NextPanel();
    }
}