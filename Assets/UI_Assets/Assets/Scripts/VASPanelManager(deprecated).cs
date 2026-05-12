using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// VAS Panel Manager with JSON Export
/// 
/// 데이터 흐름:
/// T0 VAS 입력 → T0_hunger 저장
/// T1 VAS 입력 → T1_hunger 저장
/// T3 평가 입력 → T3_taste_score 저장
/// SaveResultsAsJSON() 호출 → JSON 파일 생성
/// </summary>
public class VASPanelManager : MonoBehaviour
{
    [Header("패널들")]
    public GameObject Panel_VAS;
    public GameObject Panel_T0_Intro;
    public GameObject Panel_T2_notice;
    public GameObject Panel_T3_Evaluation;
    
    [Header("VAS 패널 요소")]
    public Slider vasSlider;
    public TextMeshProUGUI vasValueText;
    public Button vasConfirmButton;
    
    [Header("T0 패널 요소")]
    public Button continueButton;
    
    [Header("T3 평가 버튼")]
    public Button[] evaluationButtons = new Button[5];
    
    [Header("참가자 정보")]
    public string participantID = "P001"; // 개발자가 실험 시작 전 설정
    
    // ========== 이벤트 ==========
    public event System.Action<int> OnVASSubmitted;
    public event System.Action OnT0IntroComplete;
    public event System.Action<int> OnEvaluationSubmitted;
    
    // ========== 결과 저장소 ==========
    /// <summary>
    /// 모든 VAS 결과를 담는 객체
    /// T0, T1, T3 단계에서 순차적으로 채워짐
    /// </summary>
    private VASResults allResults = new VASResults();
    
    /// <summary>
    /// 현재 VAS 측정 단계 ("T0" 또는 "T1")
    /// VAS 제출 시 어느 단계인지 구분하기 위함
    /// </summary>
    private string currentPhase = "";
    
    void Start()
    {
        //시작 자체를 Deactivate 하고 bulid. 때문에 함수만 남기고, 초기 비활성화는 제거.
        //HideAllPanels();
        
        // VAS 슬라이더 초기 설정
        vasSlider.minValue = 0;
        vasSlider.maxValue = 100;
        vasSlider.value = 50;
        vasSlider.onValueChanged.AddListener((value) =>
        {
            vasValueText.text = ((int)value) + " mm";
        });
        
        // 버튼 이벤트 리스너 등록
        vasConfirmButton.onClick.AddListener(OnVASConfirmClicked);
        continueButton.onClick.AddListener(OnT0ContinueClicked);
        
        for (int i = 0; i < evaluationButtons.Length; i++)
        {
            int score = i + 1;
            evaluationButtons[i].onClick.AddListener(() => OnEvaluationClicked(score));
        }
        
        Debug.Log("✓ VASPanelManager 초기화 완료");
        Debug.Log($"✓ Participant ID: {participantID}");
    }
    
    void HideAllPanels()
    {
        Panel_VAS.SetActive(false);
        Panel_T0_Intro.SetActive(false);
        Panel_T2_notice.SetActive(false);
        Panel_T3_Evaluation.SetActive(false);
    }
    
    // ========== 패널 표시 함수 ==========
    
    public void ShowT0Intro()
    {
        HideAllPanels();
        Panel_T0_Intro.SetActive(true);
        Debug.Log("✓ T0 안내 패널 표시");
    }
    
    /// <summary>
    /// VAS 패널 표시
    /// phase: "T0" (배고픔 측정) 또는 "T1" (식사 후 배고픔)
    /// </summary>
    public void ShowVAS(string phase = "T0")
    {
        HideAllPanels();
        Panel_VAS.SetActive(true);
        vasSlider.value = 50;
        vasValueText.text = "50 mm";
        currentPhase = phase;
        Debug.Log($"✓ VAS 패널 표시 ({phase})");
    }
    
    public void ShowT2Notice()
    {
        HideAllPanels();
        Panel_T2_notice.SetActive(true);
        Debug.Log("✓ T2 안내 패널 표시");
    }
    
    public void ShowT3Evaluation()
    {
        HideAllPanels();
        Panel_T3_Evaluation.SetActive(true);
        Debug.Log("✓ T3 평가 패널 표시");
    }
    
    public void HideAllPanelsPublic()
    {
        HideAllPanels();
    }
    
    // ========== 버튼 이벤트 핸들러 ==========
    
    /// <summary>
    /// VAS 제출 버튼 클릭
    /// 현재 단계(T0 또는 T1)의 배고픔 값을 저장
    /// </summary>
    void OnVASConfirmClicked()
    {
        int value = (int)vasSlider.value;
        Debug.Log($"✓ VAS 제출 ({currentPhase}): {value}mm");
        
        // ▼▼▼ 핵심: 데이터 저장 ▼▼▼
        if (currentPhase == "T0")
        {
            allResults.T0_hunger = value;  // T0 배고픔 저장
            Debug.Log($"  └─ allResults.T0_hunger = {value}");
        }
        else if (currentPhase == "T1")
        {
            allResults.T1_hunger = value;  // T1 배고픔 저장
            Debug.Log($"  └─ allResults.T1_hunger = {value}");
        }
        // ▲▲▲ 데이터 저장 끝 ▲▲▲
        
        // 외부 스크립트에 이벤트 발생
        OnVASSubmitted?.Invoke(value);
        Panel_VAS.SetActive(false);
    }
    
    void OnT0ContinueClicked()
    {
        Debug.Log("✓ T0 안내 완료");
        OnT0IntroComplete?.Invoke();
        Panel_T0_Intro.SetActive(false);
    }
    
    /// <summary>
    /// T3 평가 버튼 클릭
    /// 맛 평가 점수(1~5)를 저장
    /// </summary>
    void OnEvaluationClicked(int score)
    {
        Debug.Log($"✓ 맛 평가: {score}점");
        
        // ▼▼▼ 핵심: 데이터 저장 ▼▼▼
        allResults.T3_taste_score = score;  // T3 맛 평가 저장
        Debug.Log($"  └─ allResults.T3_taste_score = {score}");
        // ▲▲▲ 데이터 저장 끝 ▲▲▲
        
        OnEvaluationSubmitted?.Invoke(score);
        Panel_T3_Evaluation.SetActive(false);
    }
    
    // ========== JSON 저장 함수 ==========
    
    /// <summary>
    /// 모든 VAS 결과를 JSON으로 저장
    /// 
    /// 저장 단계:
    /// 1. timestamp 추가 (저장 시간)
    /// 2. participant_id 추가 (참가자 ID)
    /// 3. 기존 결과들과 함께 JSON으로 변환
    /// 4. 폴더가 없으면 생성
    /// 5. 파일명: VAS_{participantID}_{timestamp}.json
    /// 6. 파일 저장
    /// </summary>
    public void SaveResultsAsJSON()
    {
        // 1️⃣ timestamp 설정 (저장 시간)
        allResults.timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 2️⃣ participant_id 설정
        allResults.participant_id = participantID;
        
        // 3️⃣ JSON으로 변환 (들여쓰기 포함)
        string json = JsonUtility.ToJson(allResults, true);
        
        // 4️⃣ 저장 폴더 설정
        // Mac: ~/Library/Logs/Unity/[ProjectName]/VAS_Results/
        // Android: /sdcard/Android/data/[AppName]/files/Documents/VAS_Results/
        string directory = Path.Combine(
            Application.persistentDataPath,  // 플랫폼별 데이터 경로
            "VAS_Results"                    // 폴더명
        );
        
        // 5️⃣ 폴더가 없으면 생성
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Debug.Log($"✓ 폴더 생성: {directory}");
        }
        
        // 6️⃣ 파일명 생성
        // 형식: VAS_P001_20260508_143045.json
        string filename = $"VAS_{participantID}_{System.DateTime.Now:yyyyMMdd_HHmmss}.json";
        string path = Path.Combine(directory, filename);
        
        // 7️⃣ 파일 저장
        File.WriteAllText(path, json);
        
        // 디버그 정보 출력
        Debug.Log($"✓ JSON 저장 완료!");
        Debug.Log($"  └─ 경로: {path}");
        Debug.Log($"  └─ 저장된 내용:");
        Debug.Log(json);
    }
    
    /// <summary>
    /// 현재 수집된 결과를 Console에 출력 (디버깅용)
    /// </summary>
    public void PrintCurrentResults()
    {
        Debug.Log("════════════════════════════════════");
        Debug.Log("현재 수집된 VAS 결과");
        Debug.Log("════════════════════════════════════");
        Debug.Log($"Participant ID: {participantID}");
        Debug.Log($"T0 배고픔:       {allResults.T0_hunger}mm");
        Debug.Log($"T1 배고픔:       {allResults.T1_hunger}mm");
        Debug.Log($"T3 맛 평가:     {allResults.T3_taste_score}점");
        Debug.Log("════════════════════════════════════");
    }
    
    // ========== 테스트용 함수 ==========
    
    public void Test_ShowT0() => ShowT0Intro();
    public void Test_ShowVAS_T0() => ShowVAS("T0");
    public void Test_ShowVAS_T1() => ShowVAS("T1");
    public void Test_ShowT2() => ShowT2Notice();
    public void Test_ShowT3() => ShowT3Evaluation();
    public void Test_SaveJSON() => SaveResultsAsJSON();
    public void Test_PrintResults() => PrintCurrentResults();
}

/// <summary>
/// VAS 결과 데이터 (JSON 직렬화용)
/// 
/// 각 필드:
/// - timestamp: 데이터 저장 시간
/// - participant_id: 참가자 ID
/// - T0_hunger: T0 단계 배고픔 (0~100mm VAS)
/// - T1_hunger: T1 단계 배고픔 (0~100mm VAS)
/// - T3_taste_score: T3 단계 맛 평가 (1~5점)
/// </summary>
[System.Serializable]
public class VASResults
{
    public string timestamp = "";       // "2026-05-08 14:30:45"
    public string participant_id = "";  // "P001"
    public int T0_hunger = 0;           // 0~100
    public int T1_hunger = 0;           // 0~100
    public int T3_taste_score = 0;      // 1~5
}