using UnityEngine;

public class Mini3_Shooting : MonoBehaviour
{
    [Header("발사 설정")]
    public GameObject bulletPrefab;
    public float      bulletSpeed          = 10f;

    [Header("이동 가능 영역")]
    public Transform  movementAreaCenter;
    public Vector3    movementAreaSize     = new Vector3(20f,1f,20f);

    float fixedY;

    void Start()
    {
        fixedY = transform.position.y;
    }

    void Update()
    {
        if (bulletPrefab == null || movementAreaCenter == null) 
            return;
        
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        Vector3 pos = transform.position;
        Vector3 c   = movementAreaCenter.position;
        Vector3 h   = movementAreaSize * 0.5f;

        bool inArea = 
            pos.x >= c.x - h.x && pos.x <= c.x + h.x &&
            pos.z >= c.z - h.z && pos.z <= c.z + h.z;

        if (!inArea) return;

        // <- 여기만 fixedY+1f 로 변경
        Vector3 spawnPos = new Vector3(pos.x, 1f, pos.z);
        GameObject b = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        var rb = b.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity   = new Vector3(0f, 0f, bulletSpeed);
        }
        else
        {
            var bm = b.AddComponent<Mini3_Bullet>();
            bm.speed = bulletSpeed;
        }
    }
}