using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Renderer), typeof(Collider))]
public class Mini4_Terminal : MonoBehaviour
{
    [HideInInspector] public int  id;
    [HideInInspector] public bool isRight;
    [HideInInspector] public bool isConnected;

    Renderer _rend;

    void Awake()
    {
        _rend = GetComponent<Renderer>();

        // Ensure each terminal has a unique material instance
        _rend.sharedMaterial = new Material(_rend.sharedMaterial);
        _rend.sharedMaterial.name = $"{gameObject.name}_MatInstance";

        // Collider를 Trigger로 세팅 (Raycast용)
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    /// <summary>
    /// 터미널마다 고유한 색을 보이도록 MaterialPropertyBlock에만 덮어씁니다.
    /// </summary>
    public void SetColor(Color c)
    {
        if (_rend == null) return;
        _rend.sharedMaterial.color = c;
    }

    void OnDrawGizmos()
    {
        Color g = isConnected ? Color.gray : (isRight ? Color.red : Color.blue);
        Gizmos.color = g;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

#if UNITY_EDITOR
        var style = new GUIStyle() { normal = { textColor = g } };
        Handles.Label(transform.position + Vector3.up * 0.4f, id.ToString(), style);
#endif
    }
}