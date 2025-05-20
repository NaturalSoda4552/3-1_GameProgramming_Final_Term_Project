using Unity.VisualScripting;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    [Header("추적 대상")]
    public Transform targetTr;

    [Header("기본 카메라 오프셋")]
    [Range(2.0f, 20.0f)] public float distance = 10.0f;
    [Range(0.0f, 10.0f)] public float height = 2.0f;
    [Range(0.0f, 5.0f)]  public float targetOffset = 2.0f;

    [Header("부드러운 이동")]
    public Vector3 velocity = Vector3.zero;
    [Range(0.0f, 2.0f)] public float damping = 1.0f;

    [Header("충돌 회피 설정")]
    public float sphereRadius    = 0.3f;
    public float collisionBuffer = 0.1f;
    public LayerMask collisionLayers;

    private Transform camTr;

    void Start()
    {
        camTr = transform;
    }

    void LateUpdate()
    {
        if (targetTr == null) return;

        Vector3 targetPos    = targetTr.position;
        Vector3 desiredPos   = targetPos 
                             + (-targetTr.forward * distance) 
                             + (Vector3.up * height);
        Vector3 dir          = (desiredPos - targetPos).normalized;
        float   maxDist      = (desiredPos - targetPos).magnitude;

        // 기본은 원하는 위치
        Vector3 finalPos = desiredPos;

        // 1) 플레이어→카메라 방향으로 충돌 검사
        if (Physics.SphereCast(
                targetPos, 
                sphereRadius, 
                dir, 
                out RaycastHit hit, 
                maxDist, 
                collisionLayers))
        {
            // 2) 충돌 지점까지 당긴 위치 계산
            float hitDist = Mathf.Max(hit.distance - collisionBuffer, 0f);
            Vector3 corrected = targetPos + dir * hitDist;

            // 3) 높이는 최소 desiredPos.y 이상으로 유지
            corrected.y = Mathf.Max(corrected.y, desiredPos.y);

            finalPos = corrected;
        }

        // 4) 부드럽게 이동
        camTr.position = Vector3.SmoothDamp(
            camTr.position, 
            finalPos, 
            ref velocity, 
            damping);

        // 5) 항상 플레이어 위 targetOffset 만큼 바라보기
        Vector3 lookPoint = targetPos + Vector3.up * targetOffset;
        camTr.LookAt(lookPoint);
    }
}