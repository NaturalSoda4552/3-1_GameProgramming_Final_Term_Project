using UnityEngine;
using UnityEngine.UI;

public class Mini2_Manager : MonoBehaviour
{
    [Header("플레이어 설정")]
    public GameObject mainPlayer;
    public GameObject miniPlayer;

    [Header("SceneLoader 키")]
    public string sectionKey;

    // GoalZone 태그를 가진 Trigger Collider에 들어왔을 때만 동작
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PLAYERINBOMB")) 
            return;
        
        // 미니게임 내부 비활성화
        // m.mini2_active = false;

        // 1) 플레이어 복귀 및 미니플레이어 비활성화
        if (mainPlayer != null) mainPlayer.SetActive(true);
        if (miniPlayer != null) miniPlayer.SetActive(false);

        // 3) 카메라 복원
        CameraManager.Instance.SwitchMode("Start");

        // 4) 해당 섹션 오브젝트 내부 정리 & 비활성화
        var sectionObj = SceneLoader.Instance.GetSectionObject(sectionKey);
        if (sectionObj != null)
        {
            foreach (Transform child in sectionObj.transform)
                Destroy(child.gameObject);
            SceneLoader.Instance.DeactivateSection(sectionKey);
        }

        // 5) 전체 클리어 카운트 증가
        TimerandCountManager.Instance.IncrementCount();
    }
}