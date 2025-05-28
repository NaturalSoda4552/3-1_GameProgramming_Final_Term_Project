using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Mini3_Manager : MonoBehaviour
{
    [Header("■ 적 스폰 설정")]
    public List<Transform> spawnZoneCenters;     // 스폰 포인트 리스트
    public float           enemySpawnInterval = 2f;
    public GameObject      enemyPrefab;          // 반드시 EnemyMovement(또는 Mini3_Bullet) 스크립트가 붙어 있어야 함

    [Header("■ 적 이동 속도")]
    public float enemySpeed = 3f;

    [Header("■ 플레이어 이동 영역")]
    public Transform movementAreaCenter;
    public Vector3   movementAreaSize   = new Vector3(20f, 1f, 20f);

    // 내부
    private float  spawnTimer = 0f;
    private bool   gameStarted;
    private Transform sectionRoot;

    /// <summary>
    /// BombRoom_Manager에서 해당 섹션을 활성화한 직후 호출하세요.
    /// </summary>
    public void StartGame(string sectionKey)
    {
        if (gameStarted) return;
        gameStarted = true;
        spawnTimer  = 0f;

        // 스폰된 적들을 이 루트 아래에 붙여두면, 방을 비활성화할 때 깔끔하게 정리됩니다.
        var so = SceneLoader.Instance.GetSectionObject(sectionKey);
        sectionRoot = so != null ? so.transform : null;
    }

    /// <summary>
    /// Mini3 클리어 시(혹은 섹션 비활성화 직전에) 호출해서 스폰을 멈추고
    /// 혹시 남아있는 적(자식)들을 정리할 수 있습니다.
    /// </summary>
    public void StopGame()
    {
        gameStarted = false;
        // 자식으로 붙은 모든 적(=sectionRoot 하위)을 삭제
        if (sectionRoot != null)
        {
            for (int i = sectionRoot.childCount - 1; i >= 0; i--)
                DestroyImmediate(sectionRoot.GetChild(i).gameObject);
        }
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        if (!gameStarted)            return;
        if (enemyPrefab == null)     return;
        if (spawnZoneCenters.Count==0)return;

        // 1) 스폰 주기
        spawnTimer += Time.deltaTime;
        if (spawnTimer < enemySpawnInterval) return;
        spawnTimer = 0f;

        // 2) 랜덤 스폰 지점
        int idx = Random.Range(0, spawnZoneCenters.Count);
        Transform sp = spawnZoneCenters[idx];
        Vector3 spawnPos = sp.position + Vector3.up * 1f;
        Quaternion spawnRot = sp.rotation;

        // 3) 인스턴스화
        var go = Instantiate(enemyPrefab, spawnPos, spawnRot);
        // 꼭 ‘Mini3_Bullet.speed’ 처럼 발사체 속도 세팅
        var mb = go.GetComponent<Mini3_Bullet>();
        // if (mb != null) mb.force = enemySpeed;    // Mini3_Bullet 의 force 변수
        // else Debug.LogWarning("Spawned enemy missing Mini3_Bullet!");

        // 4) sectionRoot 하위로 부모 설정
        if (sectionRoot != null)
            go.transform.SetParent(sectionRoot, true);
    }

    void OnDrawGizmos()
    {
        // 적 스폰 구역: 붉은 와이어 박스
        Gizmos.color = new Color(1f,0f,0f,0.5f);
        foreach (var c in spawnZoneCenters)
            if (c!=null)
                Gizmos.DrawWireCube(c.position + Vector3.up * 1f, Vector3.one);

        // 플레이어 이동 영역: 초록 와이어 박스
        Gizmos.color = new Color(0f,1f,0f,0.5f);
        if (movementAreaCenter != null)
            Gizmos.DrawWireCube(movementAreaCenter.position, movementAreaSize);
    }
}