using System.Collections;
using UnityEngine;

public class RadialShooter : MonoBehaviour
{
    [Header("발사체 프리팹")]
    public GameObject bulletPrefab;

    [Header("발사 개수 & 간격")]
    public int   bulletCount = 30;     // 한 바퀴에 생성할 총알 수
    public float spawnRadius = 0f;     // 중앙에서 떨어진 거리
    public float spawnDelay  = 0.1f;   // 0이면 동시 생성, >0 이면 시간차 생성

    [Header("사이클 간격")]
    public float cycleInterval = 6f;   // 한 사이클(360°) 후 대기할 시간

    void Start()
    {
        // 반복 코루틴 시작
        StartCoroutine(ShootLoop());
    }

    IEnumerator ShootLoop()
    {
        while (true)
        {
            // 1) 한 사이클 발사
            if (spawnDelay <= 0f)
                ShootAll();
            else
                yield return StartCoroutine(ShootOverTime());

            // 2) 한 사이클이 끝나면 대기
            yield return new WaitForSeconds(cycleInterval);
        }
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
        // Up 축을 기준으로 forward 벡터 회전 → 발사 방향
        Quaternion rotAroundUp = Quaternion.AngleAxis(angleDeg, transform.up);
        Vector3    dir         = rotAroundUp * transform.forward;

        // 원기둥이 local Up축으로 앞으로 날아가도록 회전 조정
        Quaternion lookRot = Quaternion.LookRotation(dir, transform.up);
        Vector3  angles   = lookRot.eulerAngles;
        angles.x = 90f;  // 필요시 조정

        // 생성
        Instantiate(
            bulletPrefab,
            transform.position + dir * spawnRadius,
            Quaternion.Euler(angles)
        );
    }
}