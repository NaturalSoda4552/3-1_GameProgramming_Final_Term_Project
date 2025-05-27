using UnityEngine;

public class RedShooter : MonoBehaviour
{
    public Transform playerTr;  
    public GameObject bulletPrefab;
    public float     fireInterval = 1f;

    private float fireTimer;
    private bool  gameStarted;
    private Transform sectionRoot;

    public void StartGame(string sectionKey)
    {
        if (gameStarted) return;
        gameStarted = true;
        fireTimer = 0f;
        // 발사체를 하위에 두기 위한 섹션 루트 캐싱
        var go = SceneLoader.Instance.GetSectionObject(sectionKey);
        sectionRoot = go != null ? go.transform : null;
    }

    void Update()
    {
        if (!gameStarted) return;
        fireTimer += Time.deltaTime;
        if (fireTimer < fireInterval) return;
        fireTimer = 0f;

        Vector3 dir = (playerTr.position - transform.position).normalized;
        var look = Quaternion.LookRotation(dir, Vector3.up);
        look = Quaternion.Euler(90, look.eulerAngles.y, look.eulerAngles.z);

        // Instantiate & parent
        var bullet = Instantiate(bulletPrefab, transform.position, look);
        if (sectionRoot != null)
            bullet.transform.SetParent(sectionRoot, true);
        Destroy(bullet, 20f);
    }
}