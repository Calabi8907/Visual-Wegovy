using UnityEngine;
using System.IO;
using System;

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager Instance { get; private set; }

    [Header("실험 환경 설정")]
    public string filterType = "None"; // "None" or "Blue"

    [Header("패널 흐름 순서")]
    [Tooltip("순서대로 패널을 등록하세요")]
    public GameObject[] panelSequence;

    private int currentIndex = 0;
    private ExperimentData currentData;
    private string saveDirectory;

    void Awake()
    {
        // 싱글톤 패턴 설정 (다른 스크립트에서 쉽게 접근 가능하도록)
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializeExperiment();
        ShowCurrentPanel();
    }

    private void InitializeExperiment()
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, "Experiment_Results");
        if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);

        currentData = new ExperimentData
        {
            filterType = this.filterType,
            participantID = GenerateNextParticipantID()
        };

        Debug.Log($"실험 시작 - 참가자 번호: {currentData.participantID}");
    }

    // --- 패널 흐름 제어 ---

    public void NextPanel()
    {
        if (currentIndex < panelSequence.Length)
        {
            panelSequence[currentIndex].SetActive(false); // 현재 패널 끄기
        }

        currentIndex++;

        if (currentIndex < panelSequence.Length)
        {
            panelSequence[currentIndex].SetActive(true); // 다음 패널 켜기
        }
    }

    private void ShowCurrentPanel()
    {
        // 모든 패널 끄고 현재 인덱스만 켜기
        foreach (var panel in panelSequence) panel.SetActive(false);
        if (panelSequence.Length > 0) panelSequence[currentIndex].SetActive(true);
    }

    // --- 데이터 기록 인터페이스 (다른 스크립트에서 호출) ---

    public void RecordVAS(int vasIndex, float value)
    {
        switch (vasIndex)
        {
            case 0: currentData.VAS_0_value = value; break;
            case 1: currentData.VAS_1_value = value; break;
            case 2: currentData.VAS_2_value = value; break;
            default: Debug.LogWarning($"알 수 없는 VAS 인덱스: {vasIndex}"); break;
        }
    }

    public void RecordEvaluationAndFinish(int score)
    {
        currentData.evaluation_value = score;
        SaveDataToJson();
        NextPanel(); // End 패널로 이동
    }

    // --- 파일 I/O ---
    private int GenerateNextParticipantID()
    {
        int maxID = 0;
        string[] files = Directory.GetFiles(saveDirectory, "*.json");

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            // 파일명 규칙: Result_None_P001 또는 Result_Blue_P001
            string[] parts = fileName.Split('_');

            // parts 배열 중 'P'로 시작하는 부분을 찾아 번호를 추출하도록 유연하게 변경
            foreach (string part in parts)
            {
                if (part.StartsWith("P") && int.TryParse(part.Substring(1), out int parsedID))
                {
                    if (parsedID > maxID)
                    {
                        maxID = parsedID;
                    }
                    break; // ID를 찾았으면 더 이상 쪼갠 문자열을 검사할 필요 없음
                }
            }
        }
        return maxID + 1;
    }

    private void SaveDataToJson()
    {
        // JSON 파일 '내부'에 기록되는 타임스탬프 (데이터 분석용으로 유지는 권장하나, 필요 없다면 이 줄도 삭제 가능합니다)
        currentData.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        string json = JsonUtility.ToJson(currentData, true);

        // 형식 예시: Result_None_P001.json / Result_Blue_P002.json
        string filename = $"Result_{filterType}_P{currentData.participantID:D3}.json";
        string path = Path.Combine(saveDirectory, filename);

        File.WriteAllText(path, json);
        Debug.Log($"데이터 JSON 저장 완료: {filename}");
    }
}

[System.Serializable]
public class ExperimentData
{
    public string filterType;
    public int participantID;
    public string timestamp;
    public float VAS_0_value;
    public float VAS_1_value;
    public float VAS_2_value;
    public int evaluation_value;
}