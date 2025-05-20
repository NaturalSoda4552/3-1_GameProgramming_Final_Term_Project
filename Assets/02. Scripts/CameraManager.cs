using System.Collections.Generic;
using UnityEngine;

// 카메라 모드 인터페이스
public interface ICameraMode
{
    void Enter();              // 모드 진입 시 실행
    void Exit();               // 모드 이탈 시 실행
    void Update();             // 매 프레임 호출
}

/// <summary>
/// 플레이어를 따라다니며 충돌 회피하는 모드
/// </summary>
public class SphereCastFollowMode : ICameraMode
{
    private CameraManager mgr;
    private Transform     target;
    private float         distance;
    private float         height;
    private float         lookOffset;
    private float         damping;
    private Vector3       velocity;
    private float         sphereRadius;
    private float         buffer;
    private LayerMask     collisionMask;

    public SphereCastFollowMode(
        CameraManager mgr,
        Transform target,
        float distance,
        float height,
        float lookOffset,
        float damping,
        float sphereRadius,
        float buffer,
        LayerMask collisionMask)
    {
        this.mgr           = mgr;
        this.target        = target;
        this.distance      = distance;
        this.height        = height;
        this.lookOffset    = lookOffset;
        this.damping       = damping;
        this.velocity      = Vector3.zero;
        this.sphereRadius  = sphereRadius;
        this.buffer        = buffer;
        this.collisionMask = collisionMask;
    }

    public void Enter() { }
    public void Exit()  { }

    public void Update()
    {
        if (target == null) return;

        // 목표 위치 계산
        Vector3 targetPos  = target.position;
        Vector3 desiredPos = targetPos
                            + (-target.forward * distance)
                            + (Vector3.up * height);

        Vector3 dir   = (desiredPos - targetPos).normalized;
        float   maxD  = Vector3.Distance(desiredPos, targetPos);
        Vector3 final = desiredPos;

        // 충돌 감지 후 카메라 당기기
        if (Physics.SphereCast(targetPos, sphereRadius, dir, out RaycastHit hit, maxD, collisionMask))
        {
            float dist   = Mathf.Max(hit.distance - buffer, 0f);
            Vector3 pos  = targetPos + dir * dist;
            pos.y        = Mathf.Max(pos.y, desiredPos.y);
            final        = pos;
        }

        // 부드러운 이동
        Transform camT = mgr.MainCamera.transform;
        camT.position  = Vector3.SmoothDamp(camT.position, final, ref velocity, damping);
        camT.LookAt(targetPos + Vector3.up * lookOffset);
    }
}

/// <summary>
/// 특정 스테이지 루트 위에서 지정 높이 & 회전으로 고정 뷰
/// </summary>
public class TopDownMode : ICameraMode
{
    private CameraManager mgr;
    private Transform     stageRoot;
    private float         height;
    private Quaternion    rotation;
    private float         blendSpeed;

    public TopDownMode(CameraManager mgr, Transform stageRoot, float height, Quaternion rotation, float blendSpeed)
    {
        this.mgr        = mgr;
        this.stageRoot  = stageRoot;
        this.height     = height;
        this.rotation   = rotation;
        this.blendSpeed = blendSpeed;
    }

    public void Enter() { }
    public void Exit()  { }

    public void Update()
    {
        if (stageRoot == null) return;
        Transform camT = mgr.MainCamera.transform;

        Vector3 targetPos = stageRoot.position + Vector3.up * height;
        camT.position     = Vector3.Lerp(camT.position, targetPos, blendSpeed * Time.deltaTime);
        camT.rotation     = Quaternion.Lerp(camT.rotation, rotation, blendSpeed * Time.deltaTime);
    }
}

/// <summary>
/// 카메라 모드를 전환하여 다양한 뷰를 지원하는 매니저
/// </summary>
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("카메라 참조")]
    public Camera MainCamera;

    [Header("Follow 모드 설정")]
    public float    followDistance   = 10f;
    public float    followHeight     = 2f;
    public float    followLookOffset = 2f;
    public float    followDamping    = 1f;
    public float    sphereRadius     = 0.3f;
    public float    collisionBuffer  = 0.1f;
    public LayerMask collisionMask;

    [Header("스테이지별 탑다운 설정")]
    public StageConfig[] stageConfigs;

    [System.Serializable]
    public struct StageConfig
    {
        public string    key;         // 모드 호출 키, 예: "Mini1"
        public Transform stageRoot;  // 스테이지 루트 오브젝트
        public float     height;     // 카메라 높이
        public Vector3   eulerRot;   // 카메라 회전(예: 90,0,0)
        public float     blendSpeed; // 전환 속도
    }

    private Dictionary<string, ICameraMode> modes;
    private ICameraMode currentMode;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            modes = new Dictionary<string, ICameraMode>();
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1) 플레이어 Follow 모드 등록
        Transform player = GameObject.FindWithTag("Player")?.transform;
        modes.Add("Start",  new SphereCastFollowMode(this, player, followDistance, followHeight, followLookOffset, followDamping, sphereRadius, collisionBuffer, collisionMask));
        modes.Add("InBomb", new SphereCastFollowMode(this, player, followDistance, followHeight, followLookOffset, followDamping, sphereRadius, collisionBuffer, collisionMask));

        // 2) Inspector 설정된 스테이지 탑다운 모드 등록
        foreach (var cfg in stageConfigs)
        {
            if (cfg.stageRoot == null)
            {
                Debug.LogWarning($"CameraManager: '{cfg.key}' 스테이지 루트가 없습니다.");
                continue;
            }
            Quaternion rot = Quaternion.Euler(cfg.eulerRot);
            modes.Add(cfg.key, new TopDownMode(this, cfg.stageRoot, cfg.height, rot, cfg.blendSpeed));
        }

        // 초기 모드
        // SwitchMode("Start");
        SwitchMode("Mini1");
    }
    

    void LateUpdate()
    {
        currentMode?.Update();
    }

    /// <summary>
    /// key에 매핑된 모드로 전환합니다.
    /// </summary>
    public void SwitchMode(string key)
    {
        currentMode?.Exit();
        if (modes.TryGetValue(key, out var mode))
        {
            currentMode = mode;
            currentMode.Enter();
        }
        else Debug.LogWarning($"CameraManager: 모드 '{key}'가 없습니다.");
    }
}