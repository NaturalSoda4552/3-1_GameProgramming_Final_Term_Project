using UnityEngine;


public class Mini2_Detecting : MonoBehaviour
{
    [Header("검출 대상")]
    public Transform player;
    public Transform playerStartPoint;

    [Header("시야 설정")]
    [Tooltip("시야각(±)")]
    public float viewAngle = 30f;
    [Tooltip("감지 반경")]
    public float viewRadius = 5f;
    [Tooltip("감지 완료까지 필요한 시간(초)")]
    public float detectTimeRequired = 0.5f;

    [Header("시야 높이(머리 위치)")]
    public float eyeHeight = 0.5f;

    [Header("Spot Light (Inspector 에 할당)")]
    public Light spotLight;

    [Header("장애물 마스크 (벽 레이어)")]
    public LayerMask obstacleMask;

    // 내부
    private float detectTimer;
    private Color defaultColor = Color.yellow;
    private Color alertColor   = Color.red;

    public bool isDetected = false;

    void Start()
    {
        // Spot Light 세팅
        spotLight.type      = LightType.Spot;
        spotLight.spotAngle = viewAngle * 2f;
        spotLight.range     = viewRadius;
        spotLight.shadows        = LightShadows.Soft;
        spotLight.shadowStrength = 1f;
        spotLight.shadowBias     = 0.05f;
        spotLight.color          = defaultColor;

        detectTimer = 0f;
    }

    void Update()
    {
        // 1) 눈 위치 & 플레이어 머리 위치 계산
        Vector3 eyePos    = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPos = player.position   + Vector3.up * eyeHeight;
        Vector3 toPlayer  = targetPos - eyePos;
        float   dist      = toPlayer.magnitude;

        bool inSight = false;

        // 2) 거리 & 각도 검사
        if (dist <= viewRadius &&
            Vector3.Angle(transform.forward, toPlayer) <= viewAngle)
        {
            // 3) 벽(장애물)만 검사해서 가로막혔는지 확인
            bool blocked = Physics.Raycast(
                eyePos,
                toPlayer.normalized,
                dist,           // 플레이어까지의 거리까지만
                obstacleMask
            );

            if (!blocked)
                inSight = true;
        }

        // 4) 감지 유지/해제 처리
        if (inSight)
        {
            // 감지 조건 true
            isDetected = true;
            
            detectTimer += Time.deltaTime;
            spotLight.color = Color.Lerp(defaultColor, alertColor, detectTimer / detectTimeRequired);

            if (detectTimer >= detectTimeRequired)
            {
                
                ReturnPlayerToStart();
                ResetDetection();
            }
        }
        else
        {
            ResetDetection();
        }
    }

    private void ResetDetection()
    {
        detectTimer     = 0f;
        spotLight.color = defaultColor;
    }

    private void ReturnPlayerToStart()
    {
        // CharacterController가 있으면 잠시 껐다 켜서 위치 보정
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = playerStartPoint.position;
        player.rotation = playerStartPoint.rotation;

        if (cc != null) cc.enabled = true;
        
        // 감지 조건 false → 모든 Mini2_Detecting 인스턴스 리셋
        foreach (var det in FindObjectsOfType<Mini2_Detecting>())
        {
            det.isDetected = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        // 감지 반경
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // 시야 각도
        Vector3 leftDir  = Quaternion.Euler(0, -viewAngle, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0,  viewAngle, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + leftDir  * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * viewRadius);
    }
}