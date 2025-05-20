using UnityEngine;

public class RedShooter : MonoBehaviour
{
    [Header("발사 대상(플레이어)")]
    public Transform playerTr;                  

    [Header("발사체 설정")]
    public GameObject bulletPrefab;       
    public float     fireInterval    = 1.0f;  

    private float fireTimer = 0f;

    void Update()
    {
        if (playerTr == null || bulletPrefab == null) 
            return;

        // 1) 타이머 갱신
        fireTimer += Time.deltaTime;
        if (fireTimer < fireInterval) 
            return;
        fireTimer = 0f;

        // 2) 플레이어 방향 계산
        Vector3 dir = (playerTr.position - transform.position).normalized;

        // 3) 본래 LookRotation으로 yaw·pitch 계산
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
        Vector3  angles  = lookRot.eulerAngles;

        // 4) x축은 90도로 고정, y/z는 계산된 대로 사용
        angles.x = 90f;

        // 5) 인스턴스화
        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position,
            Quaternion.Euler(angles)
        );

        // 6) 자동 파괴
        Destroy(bullet, 20f);
    }
}