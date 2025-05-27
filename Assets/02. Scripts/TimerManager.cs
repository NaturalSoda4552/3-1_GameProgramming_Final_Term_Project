using UnityEngine;
using UnityEngine.UI;

public class TimerandCountManager : MonoBehaviour
{
    public static TimerandCountManager Instance { get; private set; }

    [Header("타이머 UI")]
    public Text timerText;

    [Header("진행도 UI")]
    [Tooltip("몇 개의 미니게임을 클리어했는지 표시할 텍스트(예: 3/6)")]
    public Text countText;
    [Tooltip("총 미니게임 수")]
    public int totalGames = 6;

    private float totalTime;
    private float remainTime;
    private bool  running;

    private int clearedCount;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        // 초기 카운트 리셋
        clearedCount = 0;
        UpdateCountUI();
    }

    void Update()
    {
        if (!running) return;
        remainTime -= Time.deltaTime;
        if (remainTime <= 0f)
        {
            remainTime = 0f;
            UpdateTimerUI();
            TimeUp();
            return;
        }
        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        int mm = Mathf.FloorToInt(remainTime / 60f);
        int ss = Mathf.FloorToInt(remainTime % 60f);
        timerText.text = $"{mm:00}:{ss:00}";
    }

    private void UpdateCountUI()
    {
        if (countText == null) return;
        countText.text = $"{clearedCount}/{totalGames}";
    }

    /// <summary>타이머 초기화 및 시작</summary>
    public void Init(float seconds)
    {
        totalTime  = seconds;
        remainTime = seconds;
        running    = true;
        if (timerText != null) timerText.gameObject.SetActive(true);
        UpdateTimerUI();
    }

    /// <summary>타이머 일시정지</summary>
    public void Pause()
    {
        running = false;
    }

    /// <summary>타이머 재개</summary>
    public void Resume()
    {
        running = true;
    }

    /// <summary>타이머 숨김 및 정지</summary>
    public void Hide()
    {
        running = false;
        if (timerText != null) timerText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 미니게임을 하나 클리어할 때마다 호출하세요.
    /// </summary>
    public void IncrementCount()
    {
        clearedCount++;
        if (clearedCount > totalGames) clearedCount = totalGames;
        UpdateCountUI();
        if (clearedCount >= totalGames)
            AllCleared();
    }

    private void TimeUp()
    {
        // 제한 시간 만료 시
        if (clearedCount < totalGames)
            NotAllClearedInTime();
        else
            AllCleared();
    }

    private void NotAllClearedInTime()
    {
        Debug.Log("⏱ 제한 시간 내에 모든 미니게임을 클리어하지 못했습니다.");
        // TODO: 실패 처리 로직 호출
    }

    private void AllCleared()
    {
        running = false;
        Debug.Log("🎉 모든 미니게임 클리어! 🎉");
        // TODO: 성공 처리 로직 호출
    }
}