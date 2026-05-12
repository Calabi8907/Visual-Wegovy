using UnityEngine;
using UnityEngine.UI;

public class EvaluationController : MonoBehaviour
{
    [Header("평가 버튼들 (순서대로 5점 ~ 1점 매핑)")]
    public Button btn_E_Good; // 5
    public Button btn_Good;   // 4
    public Button btn_Neutral;// 3
    public Button btn_Bad;    // 2
    public Button btn_E_Bad;  // 1

    void Start()
    {
        btn_E_Good.onClick.AddListener(() => SubmitScore(5));
        btn_Good.onClick.AddListener(() => SubmitScore(4));
        btn_Neutral.onClick.AddListener(() => SubmitScore(3));
        btn_Bad.onClick.AddListener(() => SubmitScore(2));
        btn_E_Bad.onClick.AddListener(() => SubmitScore(1));
    }

    private void SubmitScore(int score)
    {
        // 점수를 기록하고 종료(JSON 저장 및 End 패널 전환) 처리
        ExperimentManager.Instance.RecordEvaluationAndFinish(score);
    }
}