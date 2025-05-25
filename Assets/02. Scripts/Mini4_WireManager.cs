using UnityEngine;

public class Mini4_WireManager : MonoBehaviour
{
    [Header("Wire Material (Unlit/Color)")]
    public Material wireMaterial;
    public float    wireWidth = 0.05f;

    [Header("Mini4 Manager 참조")]
    public Mini4_Manager manager;

    Camera       cam;
    Mini4_Terminal     selectedLeft;
    LineRenderer currentLine;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))          TryBegin();
        if (currentLine != null)                  UpdateLine();
        if (Input.GetMouseButtonUp(0) && currentLine != null) EndLine();
    }

    void TryBegin()
    {
        // 왼쪽 터미널 클릭했는지 검사
        var hit = RaycastTerminal();
        if (hit != null && !hit.isRight && !hit.isConnected)
        {
            selectedLeft = hit;

            // 새로운 Wire 오브젝트 생성
            var go = new GameObject("Wire");
            currentLine = go.AddComponent<LineRenderer>();
            // 터미널의 머티리얼을 그대로 사용
            var rend = selectedLeft.GetComponent<Renderer>();
            if (rend != null && rend.sharedMaterial != null)
            {
                currentLine.material = rend.sharedMaterial;
            }
            currentLine.widthMultiplier = wireWidth;
            currentLine.positionCount   = 2;
            currentLine.useWorldSpace   = true;

            // 시작점 설정
            Vector3 p = selectedLeft.transform.position;
            currentLine.SetPosition(0, p);
            currentLine.SetPosition(1, p);

            // // ▶ 터미널의 머티리얼 색을 와이어 색으로 사용
            // var rend = selectedLeft.GetComponent<Renderer>();
            // if (rend != null)
            // {
            //     Color col = rend.sharedMaterial.color;
            //     currentLine.startColor = currentLine.endColor = col;
            // }
        }
    }

    void UpdateLine()
    {
        // 마우스 위치를 터미널 Y 높이 평면으로 프로젝트
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        float planeY = selectedLeft.transform.position.y;
        float t      = (planeY - ray.origin.y) / ray.direction.y;
        Vector3 pt   = ray.origin + ray.direction * t;
        currentLine.SetPosition(1, pt);
    }

    void EndLine()
    {
        var hit = RaycastTerminal();
        bool matched = false;

        if (hit != null && hit.isRight && !hit.isConnected && hit.id == selectedLeft.id)
        {
            // 올바른 매칭
            matched = true;
            currentLine.SetPosition(1, hit.transform.position);

            selectedLeft.isConnected = true;
            hit.isConnected          = true;
            manager.NotifyConnection();
        }

        if (!matched)
        {
            // 잘못된 연결이면 삭제
            Destroy(currentLine.gameObject);
        }

        currentLine  = null;
        selectedLeft = null;
    }

    Mini4_Terminal RaycastTerminal()
    {
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 50f))
            return hit.collider.GetComponent<Mini4_Terminal>();
        return null;
    }
}