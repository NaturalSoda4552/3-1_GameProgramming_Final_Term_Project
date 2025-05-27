using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mini6_Manager : MonoBehaviour
{
    [Header("Child GameSets")]
    public List<Mini6_GameSet> gameSets = new List<Mini6_GameSet>();

    [Header("Speed & Keys")]
    public float   minSpeed     = 2f;
    public float   maxSpeed     = 5f;
    public KeyCode[] possibleKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    private int currentIndex = 0;

    void Start()
    {
        // 자식 GameSet 수집
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
            return;
        }

        // 첫 세트 바로 시작
        StartCoroutine(DelayedStartNextSet());
    }

    private IEnumerator DelayedStartNextSet()
    {
        yield return null;  // 한 프레임 대기
        StartNextSet();
    }

    private void StartNextSet()
    {
        if (currentIndex >= gameSets.Count)
        {
            Debug.Log("🎉 Mini6 All Sets Cleared! 🎉");
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

        // 0.5초 후에 다음 세트 또는 재시도
        StartCoroutine(NextSetWithDelay());
    }

    private IEnumerator NextSetWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        StartNextSet();
    }
}