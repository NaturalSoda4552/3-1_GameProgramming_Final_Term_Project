using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BombRoom_Manager : MonoBehaviour
{
    public GameObject mini_player;    // 플레이어 프리팹
    public GameObject player;         // 진짜 플레이어
    public float interactDistance = 3f;

    [Header("미니게임 입장 포인트")]
    public Transform[] entryPoints;   // e1, e2, ... (빈 오브젝트)
    public string[]    sectionKeys;
    public Transform[] spawnPoints;

    [Header("Guide UI")]
    public GameObject guidePanel;
    public Text       guideText;

    enum Phase { InBomb, ShowingGuide, CountingDown, InMini }
    private Phase phase = Phase.InBomb;

    [SerializeField] 
    private float cameraTransitionTime = 1f;
    [SerializeField]
    private float countdownSeconds     = 3f;

    // 클리어 여부
    private bool[] cleared;
    
    // 미니게임2에 사용하는 bool변수
    public bool mini2_active = false;

    void Start()
    {
        // 1) 초기화
        TimerandCountManager.Instance.Init(300f);

        // 2) 클리어 플래그 배열 초기화
        cleared = new bool[entryPoints.Length];
        for (int i = 0; i < cleared.Length; i++)
            cleared[i] = false;

        // 3) 모든 미니게임 비활성, 폭탄 내부 활성
        foreach (var key in sectionKeys)
            SceneLoader.Instance.DeactivateSection(key);
        SceneLoader.Instance.ActivateSection("Scene2-InBomb");

        guidePanel.SetActive(false);
    }

    void Update()
    {
        if (phase != Phase.InBomb) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            for (int i = 0; i < entryPoints.Length; i++)
            {
                // 이미 클리어된 방은 건너뛰기
                if (cleared[i]) continue;

                var pt = entryPoints[i];
                if (pt == null) continue;

                // 플레이어가 범위 내이고
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
        mini_player.SetActive(false);
        
        phase = Phase.ShowingGuide;
        TimerandCountManager.Instance.Pause();

        // 방 활성화, 플레이어 셋업
        SceneLoader.Instance.ActivateSection(sectionKeys[idx]);
        if (idx == 0 || idx == 1 || idx == 2)
        {
            mini_player.SetActive(true);
        }
        player.transform.position = new Vector3(50, 0, 0);  // 폭탄방 스폰위치로 이동
        player.SetActive(false);
        if (spawnPoints[idx] != null)
            mini_player.transform.position = spawnPoints[idx].position;

        // 이동 비활성
        var move = mini_player.GetComponent<PlayerInBombMove>();
        move.DisableMovement();

        // 카메라 전환
        Time.timeScale = 1f;
        CameraManager.Instance.SwitchMode("Mini" + (idx+1));
        yield return new WaitForSeconds(cameraTransitionTime);

        // 가이드 표시 & F 대기
        guideText.text = GetGuideMessage(idx);
        guidePanel.SetActive(true);
        while (Input.GetKey(KeyCode.F)) yield return null;
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));
        guidePanel.SetActive(false);

        // 미니게임 타이머 리셋 (1,3번만)
        if ((idx == 0 || idx == 2) && move != null)
        {
            move.ResetTimer();
            move.PauseTimer();
        }

        // 3초 카운트다운
        phase = Phase.CountingDown;
        Time.timeScale = 0f;
        for (int t = (int)countdownSeconds; t > 0; t--)
        {
            guideText.text = t.ToString();
            guidePanel.SetActive(true);
            yield return new WaitForSecondsRealtime(1f);
        }
        guidePanel.SetActive(false);

        // 실제 게임 시작
        Time.timeScale = 1f;
        phase = Phase.InMini;
        TimerandCountManager.Instance.Resume();
        if ((idx == 0 || idx == 2) && move != null)
            move.ResumeTimer();
        move.EnableMovement();

        // 미니게임 스크립트 기동
        var sectionObj = SceneLoader.Instance.GetSectionObject(sectionKeys[idx]);
        if (sectionObj != null)
        {
            foreach (var red in sectionObj.GetComponentsInChildren<RedShooter>())
                red.StartGame(sectionKeys[idx]);
            foreach (var rad in sectionObj.GetComponentsInChildren<RadialShooter>())
                rad.StartGame(sectionKeys[idx]);
            // 미니게임 3 진입 시
            var m3 = sectionObj.GetComponentInChildren<Mini3_Manager>();
            if (m3 != null) m3.StartGame(sectionKeys[idx]);
            // 미니게임6 진입 시
            var mgr6 = sectionObj.GetComponentInChildren<Mini6_Manager>();
            if (mgr6 != null)
                mgr6.StartGame(sectionKeys[idx]);
        }

        mini2_active = true;

        // → 클리어 처리
        cleared[idx] = true;
        MarkClearedPlatform(idx);

        phase = Phase.InBomb;
    }

    // 클리어된 발판(tile)만 컬러 변경
    private void MarkClearedPlatform(int idx)
    {
        var parent = entryPoints[idx];
        if (parent == null) return;
        // 부모(e1 등) 아래 tile이라는 이름의 child가 있다면
        var tile = parent.Find("tile");
        if (tile == null && parent.childCount>0)
            tile = parent.GetChild(0);  // 혹은 첫 번째 child
        if (tile != null)
        {
            var rend = tile.GetComponent<Renderer>();
            if (rend != null)
            {
                // 기존 머티리얼 인스턴스에 알파 유지한 채 RGB만 초록으로
                var c = rend.material.color;
                rend.material.color = new Color(0f,1f,0f,c.a);
            }
        }
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
            default:return "Press F to start...";
        }
    }

    void OnDrawGizmos()
    {
        if (entryPoints == null) return;

        for (int i = 0; i < entryPoints.Length; i++)
        {
            var pt = entryPoints[i];
            if (pt == null) continue;

            // 클리어 여부에 따라 색상
            var col = (cleared != null && i<cleared.Length && cleared[i])
                ? new Color(0f,1f,0f,0.4f)   // 초록
                : new Color(1f,0f,0f,0.4f);  // 빨강

            Gizmos.color = col;
            Gizmos.DrawSphere(pt.position, interactDistance);
#if UNITY_EDITOR
            Handles.color = col;
            Handles.Label(pt.position + Vector3.up*0.2f, (i+1).ToString(), EditorStyles.boldLabel);
#endif
        }
    }
}