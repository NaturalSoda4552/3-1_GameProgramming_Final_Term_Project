using System;
using UnityEngine;
using UnityEngine.UI;

public class MiniTimerManager : MonoBehaviour
{
    public static MiniTimerManager Instance { get; private set; }

    [Header("타이머 UI")]
    public Text timerText;

    public event Action OnTimerEnd;      // ← 추가: 타임업 이벤트

    private float totalTime;
    private float remainTime;
    private bool  running;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (!running) return;

        remainTime -= Time.deltaTime;
        if (remainTime < 0f) remainTime = 0f;

        UpdateUI();

        if (remainTime <= 0f)
        {
            running = false;
            OnTimeUp();
        }
    }

    void UpdateUI()
    {
        if (timerText == null) return;
        int mm = Mathf.FloorToInt(remainTime / 60f);
        int ss = Mathf.FloorToInt(remainTime % 60f);
        timerText.text = $"{mm:00}:{ss:00}";
    }

    public void Init(float seconds)
    {
        totalTime  = seconds;
        remainTime = seconds;
        running    = true;
        if (timerText != null) timerText.gameObject.SetActive(true);
        UpdateUI();
    }

    public void Pause()
    {
        running = false;
    }

    public void Resume()
    {
        running = true;
    }

    public void AddTime(float seconds)
    {
        remainTime += seconds;
        if (remainTime > totalTime) totalTime = remainTime;
        UpdateUI();
    }

    public void Hide()
    {
        running = false;
        if (timerText != null) timerText.gameObject.SetActive(false);
    }

    private void OnTimeUp()
    {
        Debug.Log("⏰ MiniTimerManager: Time's up!");
        OnTimerEnd?.Invoke();           // ← 추가: 이벤트 호출
    }
}