using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerInBombMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("플레이어 이동 속도 (단위: m/s)")]
    public float moveSpeed = 5f;

    [Header("Invulnerability Settings")]
    [Tooltip("피격 시 무적 유지 시간 (초)")]
    public float invulnDuration = 2f;

    [Header("Mini1 Timer Settings")]
    [Tooltip("총알 맞을 때 추가될 시간 (초)")]
    public float addTimePerHit = 5f;

    Rigidbody _rb;
    Renderer  _rend;
    bool      isInvulnerable;
    bool      canMove;
    private int s = 10;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _rend = GetComponentInChildren<Renderer>();
    }

    void Start()
    {
        // 초기 상태: 움직임 금지
        canMove = false;
    }

    void Update()
    {
        if (!canMove) return;

        // WASD 이동
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = new Vector3(h, 0f, v);
        if (dir.sqrMagnitude > 1f) dir.Normalize();
        Vector3 vel = dir * moveSpeed;
        vel.y = _rb.linearVelocity.y;
        _rb.linearVelocity = vel;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BULLET") && !isInvulnerable)
        {
            Destroy(other.gameObject);

            // MiniTimerManager 에 5초 추가
            MiniTimerManager.Instance.AddTime(addTimePerHit);

            StartCoroutine(InvulnerableBlink());
        }
    }

    IEnumerator InvulnerableBlink()
    {
        isInvulnerable = true;
        float elapsed = 0f;
        while (elapsed < invulnDuration)
        {
            if (_rend != null) _rend.enabled = !_rend.enabled;
            yield return new WaitForSeconds(0.2f);
            elapsed += 0.2f;
        }
        if (_rend != null) _rend.enabled = true;
        isInvulnerable = false;
    }

    /// <summary>이동 허용</summary>
    public void EnableMovement()
    {
        canMove = true;
    }

    /// <summary>이동 금지</summary>
    public void DisableMovement()
    {
        canMove = false;
        if (_rb != null) _rb.linearVelocity = Vector3.zero;
    }

    /// <summary>타이머 리셋 & 중지</summary>
    public void ResetTimer()
    {
        MiniTimerManager.Instance.Init(s);
        MiniTimerManager.Instance.Pause();
    }

    /// <summary>타이머 일시정지</summary>
    public void PauseTimer()
    {
        MiniTimerManager.Instance.Pause();
    }

    /// <summary>타이머 재개</summary>
    public void ResumeTimer()
    {
        MiniTimerManager.Instance.Resume();
    }
}