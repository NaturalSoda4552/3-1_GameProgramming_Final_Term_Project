using UnityEngine;

public class Mini3_Bullet : MonoBehaviour
{
    [Tooltip("Z 축 증가 방향으로 올라가는 속도")]
    public float speed = 10f;

    void Update()
    {
        transform.position += Vector3.forward * speed * Time.deltaTime;
    }
}