using UnityEngine;

public class Mini2_Detecting : MonoBehaviour
{
    [Header("검출 대상")]
    public Transform player;
    public Transform playerStartPoint;

    [Header("시야 설정")]
    public float viewAngle = 30f;
    public float viewRadius = 5f;
    public float detectTimeRequired = 0.5f;

    [Header("시야 높이(머리 위치)")]
    public float eyeHeight = 1.5f;

    [Header("Spot Light (별도 오브젝트로)")]
    public Light spotLight;            // Inspector 에 할당

    private float detectTimer = 0f;

    // 색상 보간용
    private Color defaultColor = Color.yellow;
    private Color alertColor   = Color.red;

    void Start()
    {
        // Spot Light 세팅 (Inspector 에서 오브젝트 연결 후 한 번만)
        spotLight.type      = LightType.Spot;
        spotLight.spotAngle = viewAngle * 2f;
        spotLight.range     = viewRadius;
        spotLight.shadows        = LightShadows.Soft;
        spotLight.shadowStrength = 1f;
        spotLight.shadowBias     = 0.05f;
        spotLight.color = defaultColor;
    }

    void Update()
    {
        // 눈 위치
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
        Vector3 toPlayer = player.position - eyePos;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        if (dist <= viewRadius && Vector3.Angle(transform.forward, toPlayer) <= viewAngle)
        {
            if (Physics.Raycast(eyePos, toPlayer.normalized, out var hit, viewRadius)
                && hit.transform == player)
            {
                detectTimer += Time.deltaTime;
                float t = Mathf.Clamp01(detectTimer / detectTimeRequired);
                spotLight.color = Color.Lerp(defaultColor, alertColor, t);

                if (detectTimer >= detectTimeRequired)
                {
                    ReturnPlayerToStart();
                    ResetDetection();
                }
            }
            else ResetDetection();
        }
        else ResetDetection();
    }

    private void ResetDetection()
    {
        detectTimer = 0f;
        spotLight.color = defaultColor;
    }

    private void ReturnPlayerToStart()
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = playerStartPoint.position;
        player.rotation = playerStartPoint.rotation;

        if (cc != null) cc.enabled = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        Vector3 l = Quaternion.Euler(0, -viewAngle, 0) * transform.forward;
        Vector3 r = Quaternion.Euler(0,  viewAngle, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + l * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + r * viewRadius);
    }
}