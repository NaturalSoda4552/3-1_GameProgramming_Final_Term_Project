using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mini6_Manager : MonoBehaviour
{
    [Header("플레이어 설정")]
    public GameObject mainPlayer;    // 3인칭 플레이어
    public GameObject miniPlayer;    // 미니게임 전용 플레이어 prefab

    [Header("SceneLoader 키")]
    public string sectionKey;        // 이 매니저가 속한 섹션 키

    [Header("Child GameSets")]
    public List<Mini6_GameSet> gameSets = new List<Mini6_GameSet>();

    [Header("Speed & Keys")]
    public float   minSpeed     = 2f;
    public float   maxSpeed     = 5f;
    public KeyCode[] possibleKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    private int currentIndex = 0;
    private bool isRunning   = false;

    /// <summary>
    /// 외부에서 미니게임을 시작할 때 호출합니다.
    /// </summary>
    public void StartGame(string sectionKey)
    {
        this.sectionKey = sectionKey;

        // ① 플레이어 전환: main 끄고 mini 켜기
        if (mainPlayer != null) mainPlayer.SetActive(false);
        if (miniPlayer != null) miniPlayer.SetActive(true);

        // ② 자식 GameSet 수집
        gameSets.Clear();
        foreach (Transform child in transform)
        {
            var gs = child.GetComponent<Mini6_GameSet>();
            if (gs != null)
                gameSets.Add(gs);
        }

        if (gameSets.Count == 0)
        {
            Debug.LogWarning("[Mini6_Manager] No Mini6_GameSet found under manager.");
            OnAllCleared();
            return;
        }

        // ③ 초기화 및 시작
        currentIndex = 0;
        isRunning    = true;
        StartCoroutine(DelayedStartNextSet());
    }

    private IEnumerator DelayedStartNextSet()
    {
        // 프레임 보장 후 바로 첫 세트 시작
        yield return null;
        StartNextSet();
    }

    private void StartNextSet()
    {
        if (!isRunning) return;

        if (currentIndex >= gameSets.Count)
        {
            OnAllCleared();
            return;
        }

        float speed = Random.Range(minSpeed, maxSpeed);
        KeyCode key = possibleKeys[Random.Range(0, possibleKeys.Length)];

        gameSets[currentIndex].Initialize(key, speed, OnSetCompleted);
    }

    private void OnSetCompleted(Mini6_GameSet set, bool success)
    {
        if (success)
            currentIndex++;

        StartCoroutine(NextSetWithDelay());
    }

    private IEnumerator NextSetWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        StartNextSet();
    }

    private void OnAllCleared()
    {
        isRunning = false;
        Debug.Log("🎉 Mini6 All Sets Cleared! 🎉");

        // 1) 플레이어 복귀 및 미니플레이어 비활성화
        if (mainPlayer != null) mainPlayer.SetActive(true);
        if (miniPlayer != null) miniPlayer.SetActive(false);

        // 2) 카메라 복원
        CameraManager.Instance.SwitchMode("Start");

        // 3) 이 섹션 내부 정리 & 비활성화
        var sectionObj = SceneLoader.Instance.GetSectionObject(sectionKey);
        if (sectionObj != null)
        {
            foreach (Transform child in sectionObj.transform)
                Destroy(child.gameObject);
            SceneLoader.Instance.DeactivateSection(sectionKey);
        }

        // 4) 전체 미니게임 완료 카운트 증가
        TimerandCountManager.Instance.IncrementCount();
    }
}