using UnityEngine;

/// <summary>
/// Mini5: 미로 방, 시작지점에서 공을 굴려서 도착지점에 닿으면 클리어.
/// 도착지점 위에 투명 초록색 바닥과 트리거 존을 자동 생성해 줍니다.
/// </summary>
public class Mini5 : MonoBehaviour
{
    [Header("플레이어 설정")]
    public GameObject mainPlayer;
    public GameObject miniPlayer;

    [Header("SceneLoader 키")]
    public string sectionKey;
    
    [System.Serializable]
    public struct StartEndPair
    {
        [Tooltip("MazeRoom 내부 시작 지점")] public Transform start;
        [Tooltip("MazeRoom 내부 도착 지점")] public Transform end;
    }

    [Header("맵 기울기 세팅")]
    public float speed      = 30f;
    public float maxAngleX  = 15f;
    public float maxAngleZ  = 15f;

    [Header("Ball 설정")]
    public GameObject ballPrefab;

    [Header("도착지점 표시용 머티리얼 (투명 초록)")]
    public Material endFloorMaterial;

    [Header("출발–도착 쌍")]
    public StartEndPair[] pairs;

    [Header("도착 인식 거리")]
    public float winDistance = 0.5f;

    private Vector3 initialEuler;
    private StartEndPair currentPair;
    private GameObject   ballInstance;
    private bool         gameCleared = false;

    void Start()
    {
        initialEuler = transform.eulerAngles;

        if (pairs == null || pairs.Length == 0)
        {
            Debug.LogWarning("[Mini5] StartEndPair를 하나라도 설정해주세요.");
            return;
        }

        // 1) 랜덤 페어 선택
        currentPair = pairs[Random.Range(0, pairs.Length)];

        // 2) 공 스폰
        if (ballPrefab != null && currentPair.start != null)
        {
            ballInstance = Instantiate(
                ballPrefab,
                currentPair.start.position,
                Quaternion.identity);
            ballInstance.tag = "Ball"; // 반드시 Ball 태그 설정
            ballInstance.transform.SetParent(transform, worldPositionStays: true);

            var rb = ballInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.mass         = 1f;
                rb.linearDamping         = 0.1f;
                rb.angularDamping  = 0.05f;
            }
        }

        // 3) 도착지점 위에 투명 초록 바닥 생성
        if (currentPair.end != null && endFloorMaterial != null)
            CreateEndFloor(currentPair.end.position);

        // 4) 이 오브젝트(루트)에 SphereCollider 트리거 추가
        if (currentPair.end != null)
            CreateGoalTrigger(currentPair.end.position);
    }

    void Update()
    {
        // 맵 기울기
        float h = Input.GetAxis("Horizontal") * -1f;
        float v = Input.GetAxis("Vertical")   * -1f;

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
    }

    // 루트 오브젝트의 Collider와 Rigidbody가 있으므로 이쪽에서 트리거 감지
    void OnTriggerEnter(Collider other)
    {
        // 들어온 오브젝트 태그가 "Ball" 이면 클리어
        if (!gameCleared && other.CompareTag("Ball"))
            OnClear();
    }

    private void OnClear()
    {
        gameCleared = true;
        Debug.Log("🎉 Mini5 클리어! 🎉");

        // 플레이어 활성
        mainPlayer.SetActive(true);

        // 카메라 복원
        CameraManager.Instance.SwitchMode("Start");

        // 섹션 내부 정리 & 비활성화
        var sectionObj = SceneLoader.Instance.GetSectionObject(sectionKey);
        if (sectionObj != null)
        {
            foreach (Transform ch in sectionObj.transform)
                Destroy(ch.gameObject);
            SceneLoader.Instance.DeactivateSection(sectionKey);
        }

        // 전체 카운트 증가
        TimerandCountManager.Instance.IncrementCount();
    }

    /// <summary>
    /// 도착 지점 위에 투명한 녹색 쿼드(바닥) 하나를 생성합니다.
    /// </summary>
    void CreateEndFloor(Vector3 pos)
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
        floor.name = "EndFloor";
        floor.transform.SetParent(transform, worldPositionStays: true);
        floor.transform.position = pos + Vector3.up * 0.01f;
        floor.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        floor.transform.localScale = Vector3.one * (winDistance * 2f);
        Destroy(floor.GetComponent<Collider>());

        var rend = floor.GetComponent<Renderer>();
        if (rend != null)
            rend.material = endFloorMaterial;
    }

    /// <summary>
    /// 루트 오브젝트에 SphereCollider(trigger)와 Rigidbody를 붙여
    /// 지정된 world 위치에 센터를 맞춥니다.
    /// </summary>
    void CreateGoalTrigger(Vector3 pos)
    {
        // SphereCollider
        var sc = gameObject.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius    = winDistance;

        // local center 계산
        Vector3 local = transform.InverseTransformPoint(pos);
        sc.center = local;

        // Rigidbody (trigger 이벤트를 위해)
        var rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void OnDrawGizmosSelected()
    {
        if (pairs == null) return;
        // Start ▶ 녹색 점
        Gizmos.color = Color.green;
        foreach (var p in pairs)
            if (p.start != null)
                Gizmos.DrawSphere(p.start.position, 0.3f);
        // End ▶ 빨간 박스
        Gizmos.color = Color.red;
        foreach (var p in pairs)
            if (p.end != null)
                Gizmos.DrawWireCube(p.end.position, Vector3.one * 0.5f);
    }
}