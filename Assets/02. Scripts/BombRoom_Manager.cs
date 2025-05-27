using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BombRoom_Manager : MonoBehaviour
{
    public GameObject mini_player; // 플레이어 프리팹
    public GameObject player; // 진짜 플레이어
    public float interactDistance = 3f;
    public Transform[] entryPoints;
    public string[] sectionKeys;
    public Transform[] spawnPoints;

    [Header("Guide UI")]
    public GameObject guidePanel;
    public Text guideText;

    enum Phase { InBomb, ShowingGuide, CountingDown, InMini }
    private Phase phase = Phase.InBomb;

    [SerializeField]
    private float cameraTransitionTime = 1f;
    [SerializeField]
    private float countdownSeconds = 3f;

    void Start()
    {
        // 1) 전체 타이머 5분 초기화 및 재생
        TimerandCountManager.Instance.Init(300f);

        // 2) 모든 미니게임 비활성화, 폭탄 내부 활성화
        foreach (var key in sectionKeys)
            SceneLoader.Instance.DeactivateSection(key);
        SceneLoader.Instance.ActivateSection("Scene2-InBomb");

        guidePanel.SetActive(false);
    }

    void Update()
    {
        if (phase != Phase.InBomb) return;

        // 1) 진입 구역에서 F키
        if (Input.GetKeyDown(KeyCode.F))
        {
            for (int i = 0; i < entryPoints.Length; i++)
            {
                var pt = entryPoints[i];
                if (pt == null) continue;
                if (Vector3.Distance(player.transform.position, pt.position) <= interactDistance)
                {
                    StartCoroutine(EnterSequence(i));
                    break;
                }
            }
        }
    }

    IEnumerator EnterSequence(int idx)
    {
        // 2) 진입 시 전체 타이머 일시정지
        phase = Phase.ShowingGuide;
        TimerandCountManager.Instance.Pause();

        // 섹션 전환 및 플레이어 설정
        // SceneLoader.Instance.DeactivateSection("Scene2-InBomb");
        SceneLoader.Instance.ActivateSection(sectionKeys[idx]);
        mini_player.SetActive(true);
        player.transform.position = new Vector3(50, 0, 0);
        player.SetActive(false);
        if (spawnPoints[idx] != null)
            mini_player.transform.position = spawnPoints[idx].position;

        // 3) 카메라 전환 (실시간)
        Time.timeScale = 1f;
        CameraManager.Instance.SwitchMode("Mini" + (idx + 1));
        yield return new WaitForSeconds(cameraTransitionTime);

        // 4) 가이드 UI 표시 후 F키 대기
        guideText.text = GetGuideMessage(idx);
        guidePanel.SetActive(true);
        while (Input.GetKey(KeyCode.F)) yield return null;
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
        guidePanel.SetActive(false);

        // 준비된 미니게임 타이머 리셋 및 일시정지
        var move = mini_player.GetComponent<PlayerInBombMove>();
        if (move != null)
        {
            move.ResetTimer();  // 미니1만 아니라, 모든 미니게임 컨트롤러에 맞춰서
            move.PauseTimer();
        }

        // 5) 3초 카운트다운 (Realtime, 게임 오브젝트 일시 정지)
        phase = Phase.CountingDown;
        Time.timeScale = 0f;
        for (int t = (int)countdownSeconds; t > 0; t--)
        {
            guideText.text = t.ToString();
            guidePanel.SetActive(true);
            yield return new WaitForSecondsRealtime(1f);
        }
        guidePanel.SetActive(false);

        // 6) 실제 게임 시작: 전체/미니 타이머 재개, 이동 허용
        Time.timeScale = 1f;
        phase = Phase.InMini;
        TimerandCountManager.Instance.Resume();
        if (move != null)
            move.ResumeTimer();
        move?.EnableMovement();

        // 미니게임 스크립트 시동
        var sectionObj = SceneLoader.Instance.GetSectionObject(sectionKeys[idx]);
        if (sectionObj != null)
        {
            foreach (var red in sectionObj.GetComponentsInChildren<RedShooter>())
                red.StartGame("Mini1");
            foreach (var rad in sectionObj.GetComponentsInChildren<RadialShooter>())
                rad.StartGame("Mini1");
            // ...추가 미니게임 StartGame 호출...
        }

        phase = Phase.InBomb;
    }

    string GetGuideMessage(int idx)
    {
        switch (idx)
        {
            case 0: return "Press F to start Dodge!";
            case 1: return "Press F to start Hide-and-Seek!";
            case 2: return "Press F to start Defense!";
            case 3: return "Press F to start Wire Connect!";
            case 4: return "Press F to start Ball Puzzle!";
            case 5: return "Press F to start Timing Catch!";
            default: return "Press F to start...";
        }
    }
    void OnDrawGizmos()
    {
        if (entryPoints == null) return;
        
        Gizmos.color = new Color(1f, 1f, 0f, 0.4f);  // 반투명 노란색
        for (int i = 0; i < entryPoints.Length; i++)
        {
            var pt = entryPoints[i];
            if (pt == null) continue;
            
            // 진입 거리만큼 반투명 구체로 표시
            Gizmos.DrawSphere(pt.position, interactDistance);
            
#if UNITY_EDITOR
            // 번호 라벨: "1", "2", … "6"
            Handles.Label(
                pt.position + Vector3.up * 0.2f,
                (i + 1).ToString(),
                EditorStyles.boldLabel
            );
#endif
        }
    }
}