using UnityEngine;
using UnityEngine.UI;

public class MiniGameTimeoutHandler : MonoBehaviour
{
    [Header("복귀할 플레이어 루트")]
    public GameObject mini_player; // 플레이어 프리팹
    public GameObject player; // 진짜 플레이어
    

    [Header("해당 미니게임의 카운트다운 텍스트")]
    public Text countdownText;

    [Header("SceneLoader에 등록된 이 섹션 키")]
    public string sectionKey;

    void OnEnable()
    {
        if (MiniTimerManager.Instance != null)
            MiniTimerManager.Instance.OnTimerEnd += HandleTimeout;
    }

    void OnDisable()
    {
        if (MiniTimerManager.Instance != null)
            MiniTimerManager.Instance.OnTimerEnd -= HandleTimeout;
    }

    private void HandleTimeout()
    {
        // Debug.Log("time out!");
        var mgr = FindObjectOfType<BombRoom_Manager>();
        if (mgr != null)
            mgr.mini2_active = false;

        // 1) 플레이어 복귀 및 프리팹 비활성화
        if (player != null)       player.SetActive(true);
        if (mini_player != null)  mini_player.SetActive(false);

        // 2) 해당 미니게임 타이머 UI 숨김
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        // 3) 카메라 3인칭 시점 복원
        CameraManager.Instance.SwitchMode("Start");

        // 4) 이 미니게임 섹션을 비활성화하기 전에 내부 오브젝트 정리
        if (!string.IsNullOrEmpty(sectionKey))
        {
            var sectionObj = SceneLoader.Instance.GetSectionObject(sectionKey);
            if (sectionObj != null)
            {
                // 이 섹션 아래에 남아있는 자식들을 전부 삭제
                foreach (Transform child in sectionObj.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            // 5) 섹션 비활성화
            SceneLoader.Instance.DeactivateSection(sectionKey);
        }

        // 6) 전체 미니게임 완료 카운트 증가
        TimerandCountManager.Instance.IncrementCount();
    }
}