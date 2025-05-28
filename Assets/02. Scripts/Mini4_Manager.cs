using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class Mini4_Manager : MonoBehaviour
{
    [Header("플레이어 설정")]
    public GameObject mainPlayer;
    public GameObject miniPlayer;

    [Header("SceneLoader 키")]
    public string sectionKey;
    
    [Header("왼쪽 터미널 순서대로(5개)")]  
    public Mini4_Terminal[] leftTerminals;

    [Header("가운데 터미널 순서대로(5개)")]
    public Mini4_Terminal[] centerTerminals;

    [Header("오른쪽 터미널 순서대로(5개)")]  
    public Mini4_Terminal[] rightTerminals;

    [Header("사용할 색 5개: Unlit/Color 머티리얼 색상")]  
    public Color[] possibleColors = new Color[5]
        { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };

    int connectedCount;

    void Start()
    {
        Initialize();
    }

    void OnValidate()
    {
        // 에디터에서 배열 바뀔 때마다 바로 반영
        if (!Application.isPlaying)
            Initialize();
    }

    void Initialize()
    {
        if (leftTerminals == null || centerTerminals == null || rightTerminals == null) return;
        int N = possibleColors.Length;
        if (leftTerminals.Length != N || centerTerminals == null || centerTerminals.Length != N || rightTerminals.Length != N) return;

        // 1) 왼쪽 터미널 설정
        for (int i = 0; i < N; i++)
        {
            var t = leftTerminals[i];
            if (t == null) continue;
            t.id          = i;
            t.isRight     = false;
            t.isConnected = false;
            t.SetColor(possibleColors[i]);
        }

        // 2) 가운데 터미널 색 셔플 및 설정
        var centerIdxs = Enumerable.Range(0, N).ToArray();
        for (int i = N - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (centerIdxs[i], centerIdxs[j]) = (centerIdxs[j], centerIdxs[i]);
        }
        for (int i = 0; i < N; i++)
        {
            var ct = centerTerminals[i];
            if (ct == null) continue;
            ct.id          = centerIdxs[i];
            ct.isRight     = false;
            ct.isConnected = false;
            ct.SetColor(possibleColors[centerIdxs[i]]);
        }

        // 3) 오른쪽 터미널 설정
        var idx = Enumerable.Range(0, N).ToArray();
        for (int i = N - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (idx[i], idx[j]) = (idx[j], idx[i]);
        }

        // 3) 오른쪽 터미널 설정
        for (int i = 0; i < N; i++)
        {
            var t = rightTerminals[i];
            if (t == null) continue;
            t.id          = idx[i];
            t.isRight     = true;
            t.isConnected = false;
            t.SetColor(possibleColors[idx[i]]);
        }

        connectedCount = 0;
    }

    /// <summary>WireConnector가 연결에 성공할 때 호출</summary>
    public void NotifyConnection()
    {
        connectedCount++;
        if (connectedCount >= leftTerminals.Length)
        {
            // Debug.Log("🎉 Mini-Game 4 Cleared! 🎉");
            // 1) 플레이어 복귀 및 미니플레이어 비활성화
            if (mainPlayer != null) mainPlayer.SetActive(true); 
            // if (miniPlayer != null) miniPlayer.SetActive(false); 

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
}