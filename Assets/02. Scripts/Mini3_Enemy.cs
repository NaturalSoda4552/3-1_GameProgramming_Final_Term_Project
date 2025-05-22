using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Mini3_Enemy : MonoBehaviour
{
    [Header("이동 속도")]
    [Tooltip("로컬 forward 방향으로 내려오는 속도")]
    public float moveSpeed = 3f;

    [Header("체력 설정")]
    [Tooltip("최대 체력")]
    public int maxHP = 2;

    int currentHP;

    Rigidbody rb;

    void Awake()
    {
        // 체력 초기화
        currentHP = maxHP;

        // Rigidbody 세팅
        rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.useGravity   = false;
            rb.isKinematic  = true; // Transform.Translate 로 이동
        }

        // Collider는 Trigger 여야 OnTriggerEnter 호출
        var col = GetComponent<Collider>();
        if (col && !col.isTrigger)
            col.isTrigger = true;
    }

    void Update()
    {
        // Z-축 감소 방향으로 이동 (로컬 기준)
        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 총알에 부딪히면
        if (other.CompareTag("BULLET"))
        {
            // 총알 제거
            Destroy(other.gameObject);

            // HP 감소
            currentHP--;
            Debug.Log($"{name} took damage, HP now {currentHP}");

            // HP 0 이하 시 자신 파괴
            if (currentHP <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}