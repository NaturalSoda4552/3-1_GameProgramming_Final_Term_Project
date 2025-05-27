using UnityEngine;
using UnityEngine.UI;

public class BombInteract : MonoBehaviour
{
    [Header("플레이어 참조")]
    public Transform player;             // 플레이어 Transform

    [Header("상호작용 UI")]
    public GameObject interactUI;        // 'F키: Interact' 같은 안내
    public Text       timerText;         // 우측 상단에 띄울 타이머 UI

    [Header("상호작용 거리")]
    public float interactDistance = 3f;  // 이 거리 이내일 때 UI 노출

    [Header("이동 지점")]
    public Transform destinationPoint;   // 클릭 시 플레이어가 이동할 목표 지점

    void Start()
    {
        if (interactUI   != null) interactUI.SetActive(false);
        if (timerText    != null) timerText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null || interactUI == null || destinationPoint == null)
            return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist <= interactDistance)
        {
            // 가까워지면 상호작용 안내 표시
            if (!interactUI.activeSelf)
                interactUI.SetActive(true);

            // F키로 상호작용
            if (Input.GetKeyDown(KeyCode.F))
            {
                // 1) 플레이어를 폭탄 내부로 이동
                player.position = destinationPoint.position;

                // 2) 섹션 전환
                SceneLoader.Instance.ActivateSection("Scene2-InBomb");
                SceneLoader.Instance.DeactivateSection("Scene1-Start");

                // 3) UI 갱신: 안내 UI 끄고 타이머 켬
                interactUI.SetActive(false);
            }
        }
        else
        {
            if (interactUI.activeSelf)
                interactUI.SetActive(false);
        }

        
    }
}