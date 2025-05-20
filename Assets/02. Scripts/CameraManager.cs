using System.Collections.Generic;
using UnityEngine;

// 공통 카메라 모드 인터페이스
public interface ICameraMode
{
    void Enter();              // 모드 진입 시 호출
    void Exit();               // 모드 이탈 시 호출
    void Update();             // 매 프레임마다 호출
}

// A. 플레이어를 따라다니며 충돌 회피까지 하는 모드
public class SphereCastFollowMode : ICameraMode
{
    private CameraManager mgr;
    private Transform target;
    // 오프셋
    private float distance;
    private float height;
    private float lookOffset;
    // 부드러운 이동
    private Vector3 velocity;
    private float damping;
    // 충돌 회피
    private float sphereRadius;
    private float buffer;
    private LayerMask collisionMask;

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
        this.velocity      = Vector3.zero;
        this.damping       = damping;
        this.sphereRadius  = sphereRadius;
        this.buffer        = buffer;
        this.collisionMask = collisionMask;
    }

    public void Enter() { }
    public void Exit()  { }

    public void Update()
    {
        if (target == null) return;
        // 기본 위치 계산
        Vector3 targetPos  = target.position;
        Vector3 desiredPos = targetPos
                            + (-target.forward * distance)
                            + (Vector3.up * height);
        Vector3 dir        = (desiredPos - targetPos).normalized;
        float   maxDist    = (desiredPos - targetPos).magnitude;

        Vector3 finalPos = desiredPos;
        // SphereCast 충돌 검사
        if (Physics.SphereCast(targetPos, sphereRadius, dir, out RaycastHit hit, maxDist, collisionMask))
        {
            float hitDist         = Mathf.Max(hit.distance - buffer, 0f);
            Vector3 correctedPos  = targetPos + dir * hitDist;
            correctedPos.y        = Mathf.Max(correctedPos.y, desiredPos.y);
            finalPos              = correctedPos;
        }
        // 부드러운 이동
        mgr.MainCamera.transform.position = Vector3.SmoothDamp(
            mgr.MainCamera.transform.position,
            finalPos,
            ref velocity,
            damping);
        // 바라보는 지점
        Vector3 lookPoint = targetPos + Vector3.up * lookOffset;
        mgr.MainCamera.transform.LookAt(lookPoint);
    }
}

// B. 탑다운 모드 (예시)
public class TopDownMode : ICameraMode
{
    private CameraManager mgr;
    private Vector3 fixedPosition;
    private Quaternion fixedRotation;
    private float blendSpeed;

    public TopDownMode(CameraManager mgr, Vector3 pos, Quaternion rot, float blendSpeed)
    {
        this.mgr           = mgr;
        this.fixedPosition = pos;
        this.fixedRotation = rot;
        this.blendSpeed    = blendSpeed;
    }

    public void Enter() { }
    public void Exit()  { }

    public void Update()
    {
        var cam = mgr.MainCamera.transform;
        cam.position = Vector3.Lerp(cam.position, fixedPosition, blendSpeed * Time.deltaTime);
        cam.rotation = Quaternion.Lerp(cam.rotation, fixedRotation, blendSpeed * Time.deltaTime);
    }
}

// 카메라 전환 매니저
public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("카메라 참조")]
    public Camera MainCamera;

    [Header("Follow 모드 설정")]
    public float followDistance     = 10f;
    public float followHeight       = 2f;
    public float followLookOffset   = 2f;
    public float followDamping      = 1f;
    public float sphereRadius       = 0.3f;
    public float collisionBuffer    = 0.1f;
    public LayerMask collisionMask;

    [Header("TopDown 모드 예시")]
    public Vector3 topDownPosition  = new Vector3(0, 12, 0);
    public Quaternion topDownRot    = Quaternion.Euler(90f, 0f, 0f);
    public float    topDownBlend    = 2f;

    private Dictionary<string, ICameraMode> modes;
    private ICameraMode currentMode;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        modes = new Dictionary<string, ICameraMode>();
    }

    void Start()
    {
        // Player Transform
        var player = GameObject.FindWithTag("Player").transform;
        // 모드 등록
        modes.Add("Start",  new SphereCastFollowMode(this, player, followDistance, followHeight, followLookOffset, followDamping, sphereRadius, collisionBuffer, collisionMask));
        modes.Add("InBomb", new SphereCastFollowMode(this, player, followDistance, followHeight, followLookOffset, followDamping, sphereRadius, collisionBuffer, collisionMask));
        modes.Add("Mini1",  new TopDownMode(this, topDownPosition, topDownRot, topDownBlend));
        // 추가 모드도 동일하게 등록

        // 초기 모드
        // SwitchMode("Start");
        SwitchMode("Mini1");
    }

    void LateUpdate()
    {
        currentMode?.Update();
    }

    public void SwitchMode(string key)
    {
        if (currentMode != null) currentMode.Exit();
        if (modes.TryGetValue(key, out var mode))
        {
            currentMode = mode;
            currentMode.Enter();
        }
        else Debug.LogWarning($"CameraManager: 모드 '{key}' 가 없습니다.");
    }
}