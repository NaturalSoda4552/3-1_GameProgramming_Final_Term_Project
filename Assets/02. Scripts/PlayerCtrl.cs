using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorCtrl : MonoBehaviour
{
    [Header("이동/회전 속도")]
    public float moveSpeed     = 6f;
    public float rotationSpeed = 80f;

    [Header("애니메이터 댐핑")]
    public float dampTime = 0.1f;

    private Transform tr;
    private Animator  animator;

    void Awake()
    {
        tr       = transform;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 1) Raw 입력: 누르면 즉시 ±1, 떼면 즉시 0
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        float r = Input.GetAxis("Mouse X");  // 회전은 부드러워도 OK

        Vector3 moveDir = new Vector3(h, 0f, v);

        // 2) 일정 이상 입력이 들어와야만 Translate/Rotate
        if (moveDir.sqrMagnitude > 0.01f)
        {
            // 이동 (월드 공간)
            tr.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);
        }
        
        // 회전
        tr.Rotate(Vector3.up * rotationSpeed * Time.deltaTime * r);

        // 3) 애니메이터 파라미터 세팅 (댐핑 적용)
        animator.SetFloat("MoveX", h, dampTime, Time.deltaTime);
        animator.SetFloat("MoveY", v, dampTime, Time.deltaTime);
    }
}