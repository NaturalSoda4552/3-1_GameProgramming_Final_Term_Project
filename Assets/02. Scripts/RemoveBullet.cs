using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveBullet : MonoBehaviour
{
    // 충돌이 시작할 때 발생하는 이벤트
    void OnTriggerEnter(Collider coll)
    {
        // 충돌한 게임오브젝트의 태그값 비교
        if (coll.GetComponent<Collider>().CompareTag("BULLET"))
        {
            // 충돌한 게임오브젝트 삭제
            Destroy(coll.gameObject, 0.05f);
        }
    }
}