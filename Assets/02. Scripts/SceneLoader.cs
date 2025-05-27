// --- SceneLoader.cs 수정 ---
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
        public string    key;
        public GameObject sectionObject;
    }

    [Header("Sections 관리 리스트")]
    public Section[] sections;

    // 내부 딕셔너리
    private Dictionary<string, GameObject> sectionDict;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSections();
        }
        else Destroy(gameObject);
    }

    private void InitializeSections()
    {
        sectionDict = new Dictionary<string, GameObject>();
        foreach (var sec in sections)
        {
            if (!string.IsNullOrEmpty(sec.key) && sec.sectionObject != null)
                sectionDict[sec.key] = sec.sectionObject;
            else
                Debug.LogWarning($"[SceneLoader] Invalid Section: key={sec.key}");
        }
    }

    public void ActivateSection(string key)
    {
        if (sectionDict.TryGetValue(key, out var obj))
            obj.SetActive(true);
        else
            Debug.LogWarning($"[SceneLoader] ActivateSection: no '{key}'");
    }

    public void DeactivateSection(string key)
    {
        if (sectionDict.TryGetValue(key, out var obj))
            obj.SetActive(false);
        else
            Debug.LogWarning($"[SceneLoader] DeactivateSection: no '{key}'");
    }

    /// 추가 ▶ 섹션 오브젝트를 외부에서 가져오기 위한 헬퍼
    public GameObject GetSectionObject(string key)
    {
        if (sectionDict != null && sectionDict.TryGetValue(key, out var obj))
            return obj;
        return null;
    }
    
}