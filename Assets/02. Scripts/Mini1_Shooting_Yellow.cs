using System.Collections;
using UnityEngine;

public class RadialShooter : MonoBehaviour
{
    [Header("발사체 프리팹")]
    public GameObject bulletPrefab;

    [Header("발사 개수 & 간격")]
    public int  bulletCount = 36;     // 한 바퀴에 생성할 총알 수
    public float spawnRadius = 0f;    // 중앙에서 떨어진 거리
    public float spawnDelay  = 0f;    // 0이면 동시 생성, >0 이면 시간차 생성

    void Start()
    {
        if (spawnDelay <= 0f)
            ShootAll();
        else
            StartCoroutine(ShootOverTime());
    }

    void ShootAll()
    {
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = 360f / bulletCount * i;
            SpawnBullet(angle);
        }
    }

    IEnumerator ShootOverTime()
    {
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = 360f / bulletCount * i;
            SpawnBullet(angle);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnBullet(float angleDeg)
    {
        // 1) Up 축을 기준으로 forward 벡터를 회전시켜 방향 벡터를 구함
        Quaternion rotAroundUp = Quaternion.AngleAxis(angleDeg, transform.up);
        Vector3    dir         = rotAroundUp * transform.forward;

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
    }
}