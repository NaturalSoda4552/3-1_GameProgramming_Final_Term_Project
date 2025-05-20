using UnityEngine;
using UnityEngine.SceneManagement;

public class BombInteract : MonoBehaviour
{
    [Header("플레이어 참조")]
    public Transform player;             // 플레이어 Transform

    [Header("상호작용 UI")]
    public GameObject interactUI;        // Canvas 위에 만든 '왼클릭: Interact' 텍스트나 이미지

    [Header("상호작용 거리")]
    public float interactDistance = 3f;  // 이 거리 이내일 때 UI 노출

    [Header("이동 지점")]
    public Transform destinationPoint;   // 클릭 시 플레이어가 이동할 목표 지점

    void Start()
    {
        // 시작 시 UI는 비활성화
        if (interactUI != null) 
            interactUI.SetActive(false);
    }

    void Update()
    {
        if (player == null || interactUI == null || destinationPoint == null)
            return; // 필요한 참조가 없으면 아무것도 하지 않음
        
        // 플레이어와 폭탄 간 거리 계산
        float dist = Vector3.Distance(player.position, transform.position);
        // Debug.Log($"[BombInteract] dist = {dist:F2}, threshold = {interactDistance:F2}");

        if (dist <= interactDistance)
        {
            
            // 거리가 충분히 가까우면 UI 활성화
            if (!interactUI.activeSelf) 
                interactUI.SetActive(true);
            

            // 왼쪽 마우스 버튼 클릭 감지
            if (Input.GetMouseButtonDown(0))
            {
                // 즉시 지정 지점으로 이동 (텔레포트)
                player.position = destinationPoint.position;
                SceneLoader.Instance.ActivateSection("Scene2-InBomb");
                SceneLoader.Instance.DeactivateSection("Scene1-Start");

                // UI 비활성화
                interactUI.SetActive(false);
            }
        }
        else
        {
            // 거리가 멀어지면 UI 비활성화
            if (interactUI.activeSelf)
                interactUI.SetActive(false);
        }
    }
}