using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Mini3_Manager : MonoBehaviour
{
    [Header("■ 적 스폰 설정")]
    public List<Transform> spawnZoneCenters;    // 스폰 포인트 리스트
    public float           enemySpawnInterval = 2f;
    public GameObject      enemyPrefab;         // 반드시 EnemyMovement 스크립트가 붙어 있어야 함

    [Header("■ 적 이동 속도")]
    public float enemySpeed = 3f;

    [Header("■ 플레이어 이동 영역")]
    public Transform movementAreaCenter;
    public Vector3   movementAreaSize   = new Vector3(20f, 1f, 20f);

    float spawnTimer = 0f;

    void Update()
    {
        if (!Application.isPlaying) return;
        if (enemyPrefab == null || spawnZoneCenters == null || spawnZoneCenters.Count == 0) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer < enemySpawnInterval) return;
        spawnTimer = 0f;

        // 1) 랜덤 스폰 위치 선택
        int idx = Random.Range(0, spawnZoneCenters.Count);
        Transform sp = spawnZoneCenters[idx];

        // 2) Y+1 높이 보정
        Vector3 spawnPos = sp.position + Vector3.up * 1f;
        // 3) 스폰 회전: 스폰 포인트가 바라보는 방향
        Quaternion spawnRot = sp.rotation;

        // 4) 인스턴스화
        GameObject go = Instantiate(enemyPrefab, spawnPos, spawnRot);

        // 5) EnemyMovement 속도 덮어쓰기
        var em = go.GetComponent<Mini3_Bullet>();
        if (em != null)
            em.speed = enemySpeed;
        else
            Debug.LogWarning("Spawned enemy has no EnemyMovement component!");
    }

    void OnDrawGizmos()
    {
        // 적 스폰 구역: 붉은 박스
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        if (spawnZoneCenters != null)
        {
            foreach (var c in spawnZoneCenters)
                if (c != null)
                    Gizmos.DrawWireCube(c.position + Vector3.up * 1f, Vector3.one);
        }

        // 플레이어 이동 영역: 초록 박스
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        if (movementAreaCenter != null)
            Gizmos.DrawWireCube(movementAreaCenter.position, movementAreaSize);
    }
}