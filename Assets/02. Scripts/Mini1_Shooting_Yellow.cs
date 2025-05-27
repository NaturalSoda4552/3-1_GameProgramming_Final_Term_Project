using System.Collections;
using UnityEngine;

public class RadialShooter : MonoBehaviour
{
    [Header("발사체 프리팹")]
    public GameObject bulletPrefab;

    [Header("발사 개수 & 간격")]
    public int   bulletCount    = 30;
    public float spawnRadius    = 0f;
    public float spawnDelay     = 0f;    // 원한다면 0.1f 등으로 시간차 발사

    [Header("시작 대기 & 사이클 간격 (초)")]
    [Tooltip("게임 시작 후 처음 발사까지 대기 시간")]
    public float initialDelay   = 3f;
    [Tooltip("한 사이클(전부 발사) 후 다시 대기할 시간")]
    public float cycleInterval  = 3f;

    private bool hasStarted;
    private Transform sectionRoot;

    /// <summary>BombRoom_Manager에서 호출하여 발사를 시작합니다.</summary>
    public void StartGame(string sectionKey)
    {
        if (hasStarted) return;
        hasStarted = true;
        
        // 이 섹션의 루트 가져오기
        var go = SceneLoader.Instance.GetSectionObject(sectionKey);
        sectionRoot = go != null ? go.transform : null;
        
        StartCoroutine(ShootLoop());
    }

    IEnumerator ShootLoop()
    {
        // 1) 최초 대기
        yield return new WaitForSeconds(initialDelay);

        // 2) 발사 루프
        while (true)
        {
            // 순간 발사 vs 시간차 발사
            if (spawnDelay <= 0f)
                ShootAll();
            else
                yield return StartCoroutine(ShootOverTime());

            // 3) 다음 발사까지 대기
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
        Quaternion rotAroundUp = Quaternion.AngleAxis(angleDeg, transform.up);
        Vector3    dir         = rotAroundUp * transform.forward;
        Quaternion lookRot     = Quaternion.LookRotation(dir, transform.up);
        Vector3    angles      = lookRot.eulerAngles;
        angles.x = 90f;

        var bullet = Instantiate(
            bulletPrefab,
            transform.position + dir * spawnRadius,
            Quaternion.Euler(angles)
        );
        
        if (sectionRoot != null)
            bullet.transform.SetParent(sectionRoot, true);
    }
}