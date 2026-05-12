using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 버튼 클릭 시 자식 객체인 plane과 plane_filter를 토글합니다.
/// </summary>
public class FilterManager : MonoBehaviour
{
    [Header("Input System")]
    public InputActionReference toggleAction; // 필터 전환을 위한 액션 (예: X 버튼 또는 트리거)

    [Header("Target Objects")]
    [SerializeField] private GameObject plane;
    [SerializeField] private GameObject plane_filter;

    private void Awake()
    {
        // 자식 객체를 찾지 못했을 경우 경고 출력
        if (plane == null || plane_filter == null)
        {
            Debug.LogWarning("FilterManager: 'plane' 또는 'plane_filter' 자식 객체를 찾을 수 없습니다.");
        }
    }

    // New Input System 활성화 및 비활성화
    private void OnEnable() => toggleAction?.action.Enable();
    private void OnDisable() => toggleAction?.action.Disable();

    void Update()
    {
        // 액션이 트리거되었을 때(버튼을 눌렀을 때) 실행
        if (toggleAction != null && toggleAction.action.triggered)
        {
            TogglePlanes();
        }
    }

    /// <summary>
    /// 두 객체의 활성화 상태를 서로 반전시킵니다.
    /// </summary>
    private void TogglePlanes()
    {
        if (plane == null || plane_filter == null) return;

        // 현재 상태 확인 후 반전
        bool isPlaneActive = plane.activeSelf;

        plane.SetActive(!isPlaneActive);
        plane_filter.SetActive(isPlaneActive);

        // 디버그 로그 (필요 없으면 삭제 가능)
        Debug.Log($"Filter Switched: Filter is now {(plane_filter.activeSelf ? "ON" : "OFF")}");
    }
}