using UnityEngine;

/// <summary>
/// 화살표/ADWS 키로 MazeRoom을 X,Z축으로 기울이고,
/// 랜덤 선택된 시작 위치에서 공을 스폰하여 골 지점에 도착하면 클리어 메시지를 출력합니다.
/// </summary>
public class Mini5 : MonoBehaviour
{
    [System.Serializable]
    public struct StartEndPair
    {
        [Tooltip("MazeRoom 내부의 시작 지점 Transform")] public Transform start;
        [Tooltip("MazeRoom 내부의 도착 지점 Transform")] public Transform end;
    }

    [Header("Map Tilt Settings")]
    [Tooltip("Tilt Speed (deg/sec)")] public float speed = 30f;
    [Tooltip("Max Tilt Angle X axis")] public float maxAngleX = 15f;
    [Tooltip("Max Tilt Angle Z axis")] public float maxAngleZ = 15f;

    [Header("Ball & Goal Settings")]
    [Tooltip("Ball prefab with Rigidbody + Collider")] public GameObject ballPrefab;
    [Tooltip("Goal visual prefab (optional)")] public GameObject goalPrefab;
    [Tooltip("Distance within which arrival is detected")] public float winDistance = 0.5f;

    [Header("Start-End Pairs")]
    [Tooltip("설정된 시작-도착 지점(Transform) 쌍")] public StartEndPair[] pairs;

    private Vector3 initialEuler;
    private StartEndPair currentPair;
    private GameObject ballInstance;
    private bool gameCleared = false;

    void Start()
    {
        // 초기 회전값 저장
        initialEuler = transform.eulerAngles;

        // 유효한 페어가 있으면 하나 선택
        if (pairs != null && pairs.Length > 0)
        {
            currentPair = pairs[Random.Range(0, pairs.Length)];

            // Ball 스폰
            if (ballPrefab != null && currentPair.start != null)
            {
                ballInstance = Instantiate(
                    ballPrefab,
                    currentPair.start.position,
                    Quaternion.identity,
                    transform);

            // Rigidbody 관성 설정: 덜 끈적이도록 Drag/Angular Drag 크게
            var rb = ballInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass = 1f;             // 공을 가볍게
                rb.linearDamping = 0.1f;           // 선형 저항을 낮춰 속도 향상
                rb.angularDamping = 0.05f;    // 회전 저항 최소화      // 회전 저항 증가
            }
            }

            // Goal 스폰 (optional)
            if (goalPrefab != null && currentPair.end != null)
            {
                Instantiate(
                    goalPrefab,
                    currentPair.end.position,
                    Quaternion.identity,
                    transform);
            }
        }
        else Debug.LogWarning("[Mini5] Start-End pairs not defined.");
    }

    void Update()
    {
        // 맵 기울기 제어 (클리어 후에도 잠시 유지)
        float h = Input.GetAxis("Horizontal") * -1;
        float v = Input.GetAxis("Vertical") * -1;

        float targetX = Mathf.Clamp(
            initialEuler.x + (-v * maxAngleX),
            initialEuler.x - maxAngleX,
            initialEuler.x + maxAngleX);
        float targetZ = Mathf.Clamp(
            initialEuler.z + ( h * maxAngleZ),
            initialEuler.z - maxAngleZ,
            initialEuler.z + maxAngleZ);

        float newX = Mathf.LerpAngle(
            transform.eulerAngles.x, targetX, Time.deltaTime * speed);
        float newZ = Mathf.LerpAngle(
            transform.eulerAngles.z, targetZ, Time.deltaTime * speed);

        transform.eulerAngles = new Vector3(newX, initialEuler.y, newZ);

        // 도착 체크
        if (!gameCleared && ballInstance != null && currentPair.end != null)
        {
            if (Vector3.Distance(
                    ballInstance.transform.position,
                    currentPair.end.position) <= winDistance)
            {
                gameCleared = true;
                Debug.Log("🎉 게임 클리어! 🎉");
            }
        }
    }

    // 선택된 페어의 시작/도착 지점을 기즈모로 표시
    void OnDrawGizmosSelected()
    {
        if (pairs == null) return;
        Gizmos.color = Color.green;
        foreach (var p in pairs)
        {
            if (p.start != null)
                Gizmos.DrawWireSphere(p.start.position, 0.3f);
        }
        Gizmos.color = Color.red;
        foreach (var p in pairs)
        {
            if (p.end != null)
                Gizmos.DrawWireCube(p.end.position, Vector3.one * 0.5f);
        }
    }
}
