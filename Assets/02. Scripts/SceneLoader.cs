using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [System.Serializable]
    public class Section
    {
        [Tooltip("구분 키 (예: 'Scene1-Start', 'Scene2-InBomb' 등)")]
        public string key;
        [Tooltip("활성/비활성으로 제어할 GameObject")]
        public GameObject sectionObject;
    }

    [Header("Sections 관리 리스트")]
    public Section[] sections;

    // 내부 딕셔너리
    private Dictionary<string, GameObject> sectionDict;

    void Awake()
    {
        // 싱글턴 초기화
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSections();
        }
        else Destroy(gameObject);
    }

    /// <summary>
    /// 섹션 리스트를 딕셔너리에 초기화
    /// </summary>
    private void InitializeSections()
    {
        sectionDict = new Dictionary<string, GameObject>();
        foreach (var sec in sections)
        {
            if (!string.IsNullOrEmpty(sec.key) && sec.sectionObject != null)
            {
                sectionDict[sec.key] = sec.sectionObject;
            }
            else
            {
                Debug.LogWarning($"[SceneLoader] 유효하지 않은 Section 항목: key={sec.key}, obj={sec.sectionObject}");
            }
        }
    }

    /// <summary>
    /// 등록된 Section을 활성화합니다.
    /// </summary>
    public void ActivateSection(string key)
    {
        if (sectionDict != null && sectionDict.TryGetValue(key, out var obj))
        {
            obj.SetActive(true);
        }
        else Debug.LogWarning($"[SceneLoader] ActivateSection: '{key}' 키로 등록된 섹션이 없습니다.");
    }

    /// <summary>
    /// 등록된 Section을 비활성화합니다.
    /// </summary>
    public void DeactivateSection(string key)
    {
        if (sectionDict != null && sectionDict.TryGetValue(key, out var obj))
        {
            obj.SetActive(false);
        }
        else Debug.LogWarning($"[SceneLoader] DeactivateSection: '{key}' 키로 등록된 섹션이 없습니다.");
    }

    
}