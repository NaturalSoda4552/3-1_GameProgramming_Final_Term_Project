using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Mini6_GameSet : MonoBehaviour
{
    [Header("Bar Movement")]
    public Transform barObject;
    public Transform leftPoint;
    public Transform rightPoint;

    [Header("Hit Zone")]
    public Collider hitZone;

    [Header("UI Prompt")]
    public Text uiPrompt;  // Canvas 상의 Text 컴포넌트

    // 외부에서 주입되는 값들
    private KeyCode targetKey;
    private float   speed;
    private Action<Mini6_GameSet, bool> onComplete;

    // 내부 상태
    private bool  isAwaiting;
    private float startTime;

    /// <summary>
    /// 매니저가 호출: 한 번의 시도를 위해 키·속도·콜백을 설정하고 시작합니다.
    /// </summary>
    public void Initialize(KeyCode key, float speed, Action<Mini6_GameSet, bool> callback)
    {
        // 파라미터 저장
        this.targetKey  = key;
        this.speed      = speed;
        this.onComplete = callback;

        // 바를 시작점으로 리셋
        if (barObject != null && leftPoint != null)
            barObject.position = leftPoint.position;

        // UI는 우선 숨기고, SetupUI 코루틴으로 한 프레임 뒤 배치
        if (uiPrompt != null)
        {
            uiPrompt.gameObject.SetActive(false);
            StartCoroutine(SetupUI());
        }

        // 이동 시작
        isAwaiting = true;
        startTime  = Time.time;
    }

    void Update()
    {
        if (!isAwaiting) return;

        // Bar 왕복 이동
        if (barObject != null && leftPoint != null && rightPoint != null)
        {
            float t = Mathf.PingPong((Time.time - startTime) * speed, 1f);
            barObject.position = Vector3.Lerp(leftPoint.position, rightPoint.position, t);
        }

        // 키 입력 체크
        if (Input.GetKeyDown(targetKey))
            CheckHit();
    }

    private void CheckHit()
    {
        isAwaiting = false;

        // 성공 여부 판단
        bool success = false;
        if (hitZone != null && barObject != null)
            success = hitZone.bounds.Contains(barObject.position);

        // UI 숨김
        if (uiPrompt != null)
            uiPrompt.gameObject.SetActive(false);

        if (success)
        {
            // 성공 시 즉시 콜백
            onComplete?.Invoke(this, true);
        }
        else
        {
            // 실패 시 깜빡이고 콜백
            StartCoroutine(BlinkFailAndRetry());
        }
    }

    private IEnumerator BlinkFailAndRetry()
    {
        // 2초 동안 0.5초 간격으로 깜빡임
        float elapsed = 0f;
        while (elapsed < 2f)
        {
            if (uiPrompt != null)
            {
                uiPrompt.text = "❌ Miss!";
                uiPrompt.gameObject.SetActive(!uiPrompt.gameObject.activeSelf);
            }
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }

        // 깜빡임 끝나면 UI 완전히 숨김
        if (uiPrompt != null)
            uiPrompt.gameObject.SetActive(false);

        // 같은 세트 재시도 콜백 (UI 재표시는 Initialize에서)
        onComplete?.Invoke(this, false);
    }

    private IEnumerator SetupUI()
    {
        // 레이아웃 갱신을 위해 한 프레임 대기
        yield return null;

        if (uiPrompt == null || leftPoint == null || rightPoint == null)
            yield break;

        // Bar 경로의 중간 월드 좌표
        Vector3 worldCenter = (leftPoint.position + rightPoint.position) * 0.5f;
        // 화면 좌표로 변환
        Vector3 screenPos   = Camera.main.WorldToScreenPoint(worldCenter);

        // Screen Space–Overlay 기준으로 localPosition 계산
        float halfW = Screen.width  * 0.5f;
        float halfH = Screen.height * 0.5f;
        Vector2 local = new Vector2(screenPos.x - halfW, screenPos.y - halfH);

        // 중앙 아래로 30px 오프셋
        uiPrompt.rectTransform.localPosition = local + new Vector2(0f, -30f);
        uiPrompt.text  = $"Press [{targetKey}]";
        uiPrompt.gameObject.SetActive(true);
    }

    void OnDrawGizmosSelected()
    {
        // 왼쪽 포인트
        if (leftPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(leftPoint.position, 0.2f);
        }
        // 오른쪽 포인트
        if (rightPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rightPoint.position, 0.2f);
        }
        // 히트 존
        if (hitZone != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(hitZone.bounds.center, hitZone.bounds.size);
        }
        // 이동 경로
        if (leftPoint != null && rightPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(leftPoint.position, rightPoint.position);
        }
    }
}