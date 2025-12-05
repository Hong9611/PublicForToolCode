using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class UnitEditorWindow : EditorWindow
{
    private enum Tab
    {
        Create,
        Edit
    }

    private Tab m_SelectedTab = Tab.Create;

    private string m_UnitName = "NewUnit";
    private string m_RCode = string.Empty;
    private GameObject m_SelectedPrefab;
    private Rank m_Rank;
    private int m_Atk;
    private float m_AtkSpeed;
    private float m_AtkRange;
    private float m_MoveSpeed;
    private bool m_UseSkill;
    private SkillType m_SelectedSkillType = SkillType.None;
    private AttackSO m_SelectedAttackSO;
    private AttackSO m_SelectedSkillAttackSO;

    private readonly List<GameObject> m_AvailablePrefabs = new List<GameObject>();
    private readonly List<AttackSO> m_AttackSOs = new List<AttackSO>();
    private readonly List<GameObject> m_CreatedPrefabs = new List<GameObject>();
    private int m_SelectedPrefabIndex;
    private int m_SelectedEditPrefabIndex;

    private bool m_IsPrefabLoaded;

    private const string Url = "https://script.google.com/macros/s/AKfycbzgNHGACErqvDoZVBtSovmmqoYjeNUZ0huhbNy-QwGX_c9kXYUaAyxeVjuJLgptw8Y/exec";

    [MenuItem("Tools/Unit Tool")]
    public static void ShowWindow()
    {
        GetWindow<UnitEditorWindow>("Unit Tool");
    }

    private void OnEnable()
    {
        LoadAvailablePrefabs();
        LoadAttackSOs();
        LoadCreatedPrefabs();
        UpdateRCode();
    }

    private void LoadAvailablePrefabs()
    {
        const string folderPath = "Assets/06. Assets/PolygonMiniFantasyCharacters/Prefabs/Characters";
        m_AvailablePrefabs.Clear();

        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning("캐릭터 프리팹 폴더를 찾을 수 없습니다: " + folderPath);
            return;
        }

        string[] prefabPaths = Directory.GetFiles(folderPath, "*.prefab");
        foreach (string prefabPath in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                m_AvailablePrefabs.Add(prefab);
            }
        }
    }

    private void LoadAttackSOs()
    {
        const string folderPath = "Assets/01. Scripts/Attack/AttackSOs";
        m_AttackSOs.Clear();

        string[] assetGuids = AssetDatabase.FindAssets("t:AttackSO", new[] { folderPath });
        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            AttackSO attackSo = AssetDatabase.LoadAssetAtPath<AttackSO>(assetPath);
            if (attackSo != null)
            {
                m_AttackSOs.Add(attackSo);
            }
        }
    }

    private void LoadCreatedPrefabs()
    {
        const string folderPath = "Assets/03. Prefabs/Character";
        m_CreatedPrefabs.Clear();

        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning("유닛 프리팹 폴더를 찾을 수 없습니다: " + folderPath);
            return;
        }

        string[] prefabPaths = Directory.GetFiles(folderPath, "*.prefab");
        foreach (string prefabPath in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null && prefab.GetComponent<Unit>() != null)
            {
                m_CreatedPrefabs.Add(prefab);
            }
        }
    }

    private void OnGUI()
    {
        DrawTabs();

        GUILayout.Space(10);

        switch (m_SelectedTab)
        {
            case Tab.Create:
                DrawCreateUnitGUI();
                break;
            case Tab.Edit:
                DrawEditUnitGUI();
                break;
        }
    }

    private void DrawTabs()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        bool createSelected =
            GUILayout.Toggle(m_SelectedTab == Tab.Create, "Create Unit Prefab", EditorStyles.toolbarButton);
        if (createSelected)
        {
            m_SelectedTab = Tab.Create;
        }

        bool editSelected =
            GUILayout.Toggle(m_SelectedTab == Tab.Edit, "Edit Unit Prefab", EditorStyles.toolbarButton);
        if (editSelected)
        {
            m_SelectedTab = Tab.Edit;
        }

        GUILayout.EndHorizontal();
    }

    private void DrawCreateUnitGUI()
    {
        GUILayout.Label("Unit Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        m_UnitName = EditorGUILayout.TextField("Name", m_UnitName);

        if (m_AvailablePrefabs.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("Select Model Prefab", EditorStyles.boldLabel);

            string[] prefabNames = new string[m_AvailablePrefabs.Count];
            for (int i = 0; i < m_AvailablePrefabs.Count; i++)
            {
                prefabNames[i] = m_AvailablePrefabs[i].name;
            }

            m_SelectedPrefabIndex = EditorGUILayout.Popup("Model", m_SelectedPrefabIndex, prefabNames);
            m_SelectedPrefab = m_AvailablePrefabs[m_SelectedPrefabIndex];
        }
        else
        {
            EditorGUILayout.HelpBox("사용 가능한 모델 프리팹이 없습니다.", MessageType.Info);
        }

        GUILayout.Space(10);
        m_Rank = (Rank)EditorGUILayout.EnumPopup("Rank", m_Rank);
        m_Atk = EditorGUILayout.IntField("Attack", m_Atk);
        m_AtkSpeed = EditorGUILayout.FloatField("Attack Speed", m_AtkSpeed);
        m_AtkRange = EditorGUILayout.FloatField("Attack Range", m_AtkRange);
        m_MoveSpeed = EditorGUILayout.FloatField("Move Speed", m_MoveSpeed);

        GUILayout.Space(10);
        m_UseSkill = EditorGUILayout.Toggle("Use Skill", m_UseSkill);

        if (m_UseSkill)
        {
            m_SelectedSkillType = (SkillType)EditorGUILayout.EnumPopup("Skill Type", m_SelectedSkillType);

            // AttackSO 수동 선택 + 자동 매핑(기본값)
            m_SelectedAttackSO = (AttackSO)EditorGUILayout.ObjectField(
                "Attack SO", m_SelectedAttackSO, typeof(AttackSO), false);
            m_SelectedSkillAttackSO = (AttackSO)EditorGUILayout.ObjectField(
                "Skill Attack SO", m_SelectedSkillAttackSO, typeof(AttackSO), false);

            if (m_SelectedPrefab != null)
            {
                if (m_SelectedAttackSO == null)
                {
                    m_SelectedAttackSO = m_AttackSOs.Find(so => so.name == m_SelectedPrefab.name);
                }

                if (m_SelectedSkillAttackSO == null)
                {
                    m_SelectedSkillAttackSO = m_AttackSOs.Find(so => so.name == m_SelectedPrefab.name);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("프리팹을 선택하세요.", MessageType.Warning);
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            UpdateRCode();
        }

        GUILayout.Space(15);
        if (GUILayout.Button("Create"))
        {
            CreateUnitPrefab();
            LoadCreatedPrefabs();
        }

        GUILayout.Space(5);
        EditorGUILayout.LabelField("RCode", m_RCode);
    }

    private void DrawEditUnitGUI()
    {
        GUILayout.Label("Edit Unit Prefab", EditorStyles.boldLabel);

        if (m_CreatedPrefabs.Count > 0)
        {
            GUILayout.BeginHorizontal();

            string[] prefabNames = new string[m_CreatedPrefabs.Count];
            for (int i = 0; i < m_CreatedPrefabs.Count; i++)
            {
                prefabNames[i] = m_CreatedPrefabs[i].name;
            }

            m_SelectedEditPrefabIndex = EditorGUILayout.Popup("Prefab", m_SelectedEditPrefabIndex, prefabNames, GUILayout.Width(300));

            if (GUILayout.Button("Load", GUILayout.Width(80)))
            {
                if (m_SelectedEditPrefabIndex >= 0 && m_SelectedEditPrefabIndex < m_CreatedPrefabs.Count)
                {
                    GameObject prefab = m_CreatedPrefabs[m_SelectedEditPrefabIndex];
                    LoadExistingPrefabInfo(prefab);
                    m_IsPrefabLoaded = true;
                }
            }

            GUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("Unit 컴포넌트가 포함된 생성된 프리팹이 없습니다.", MessageType.Warning);
        }

        GUILayout.Space(10);

        if (!m_IsPrefabLoaded)
        {
            return;
        }

        GUILayout.Label("Unit Settings", EditorStyles.boldLabel);

        m_UnitName = EditorGUILayout.TextField("Name", m_UnitName);

        GUILayout.Space(10);
        m_Rank = (Rank)EditorGUILayout.EnumPopup("Rank", m_Rank);
        m_Atk = EditorGUILayout.IntField("Attack", m_Atk);
        m_AtkSpeed = EditorGUILayout.FloatField("Attack Speed", m_AtkSpeed);
        m_AtkRange = EditorGUILayout.FloatField("Attack Range", m_AtkRange);
        m_MoveSpeed = EditorGUILayout.FloatField("Move Speed", m_MoveSpeed);

        GUILayout.Space(10);
        m_UseSkill = EditorGUILayout.Toggle("Use Skill", m_UseSkill);

        if (m_UseSkill)
        {
            m_SelectedSkillType = (SkillType)EditorGUILayout.EnumPopup("Skill Type", m_SelectedSkillType);

            m_SelectedAttackSO = (AttackSO)EditorGUILayout.ObjectField(
                "Attack SO", m_SelectedAttackSO, typeof(AttackSO), false);
            m_SelectedSkillAttackSO = (AttackSO)EditorGUILayout.ObjectField(
                "Skill Attack SO", m_SelectedSkillAttackSO, typeof(AttackSO), false);
        }

        GUILayout.Space(5);
        EditorGUILayout.LabelField("RCode", m_RCode);

        GUILayout.Space(15);
        if (GUILayout.Button("Edit"))
        {
            EditUnitPrefab();
        }
    }

    private void LoadExistingPrefabInfo(GameObject p_ExistingPrefab)
    {
        if (p_ExistingPrefab == null)
        {
            Debug.LogError("기존 프리팹이 null입니다.");
            return;
        }

        Unit unitComponent = p_ExistingPrefab.GetComponent<Unit>();
        if (unitComponent == null)
        {
            Debug.LogError("기존 프리팹에 Unit 컴포넌트가 없습니다.");
            return;
        }

        m_UnitName = unitComponent.Data.name;
        m_Rank = unitComponent.Data.rank;
        m_Atk = unitComponent.Data.atk;
        m_AtkSpeed = unitComponent.Data.atkSpeed;
        m_AtkRange = unitComponent.Data.atkRange;
        m_MoveSpeed = unitComponent.Data.moveSpeed;
        m_UseSkill = unitComponent.Data.skill;
        m_SelectedSkillType = unitComponent.Data.skillType;
        m_RCode = unitComponent.Data.rcode;

        // AttackSO / SkillAttackSO 로드 (리플렉션)
        PropertyInfo attackSoProperty = typeof(Unit).GetProperty(
            "AttackSO", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (attackSoProperty != null)
        {
            m_SelectedAttackSO = attackSoProperty.GetValue(unitComponent) as AttackSO;
        }

        PropertyInfo skillAttackSoProperty = typeof(Unit).GetProperty(
            "SkillAttackSO", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (skillAttackSoProperty != null)
        {
            m_SelectedSkillAttackSO = skillAttackSoProperty.GetValue(unitComponent) as AttackSO;
        }

        m_SelectedPrefab = p_ExistingPrefab;

        Repaint();
    }

    private void RegisterUnit()
    {
        if (!IsDataValid())
        {
            Debug.LogError("데이터가 온전하지 않습니다.");
            return;
        }

        WWWForm form = new WWWForm();
        form.AddField("order", "register");
        form.AddField("rcode", m_RCode);
        form.AddField("name", m_UnitName);
        form.AddField("rank", m_Rank.ToString().ToUpper());
        form.AddField("atk", m_Atk.ToString());
        form.AddField("atkSpeed", m_AtkSpeed.ToString());
        form.AddField("atkRange", m_AtkRange.ToString());
        form.AddField("moveSpeed", m_MoveSpeed.ToString());
        form.AddField("useSkill", m_UseSkill.ToString().ToUpper());
        form.AddField("skillType", m_SelectedSkillType.ToString().ToLower());

        Post(form);
    }

    private void Post(WWWForm p_Form)
    {
        UnityWebRequest request = UnityWebRequest.Post(Url, p_Form);
        request.SendWebRequest().completed += _ =>
        {
            try
            {
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error: {request.error}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                    return;
                }

                string response = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(response) && response.Contains("오류"))
                {
                    Debug.LogError("Google Apps Script 오류: " + response);
                }
                else
                {
                    Debug.Log("Form upload complete! Response: " + response);
                }
            }
            finally
            {
                request.Dispose();
            }
        };
    }

    private bool IsDataValid()
    {
        if (string.IsNullOrWhiteSpace(m_UnitName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(m_RCode))
        {
            return false;
        }

        if (!Enum.IsDefined(typeof(Rank), m_Rank))
        {
            return false;
        }

        if (m_Atk < 0 || m_AtkSpeed < 0f || m_AtkRange < 0f || m_MoveSpeed < 0f)
        {
            return false;
        }

        if (m_UseSkill)
        {
            if (!Enum.IsDefined(typeof(SkillType), m_SelectedSkillType) ||
                m_SelectedSkillType == SkillType.None)
            {
                return false;
            }

            if (m_SelectedAttackSO == null || m_SelectedSkillAttackSO == null)
            {
                return false;
            }
        }

        return true;
    }

    private void CreateUnitPrefab()
    {
        if (m_SelectedPrefab == null)
        {
            Debug.LogError("선택된 모델이 없습니다.");
            return;
        }

        string prefabPath = "Assets/03. Prefabs/Character/" + m_RCode + ".prefab";
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (existingPrefab != null)
        {
            bool deleteConfirmed = EditorUtility.DisplayDialog(
                "Prefab Exists",
                "같은 이름의 프리팹이 이미 존재합니다. 삭제하고 새롭게 만드시겠습니까?",
                "예",
                "아니오"
            );

            if (!deleteConfirmed)
            {
                m_SelectedTab = Tab.Edit;
                m_SelectedEditPrefabIndex = m_CreatedPrefabs.IndexOf(existingPrefab);
                LoadExistingPrefabInfo(existingPrefab);
                Repaint();
                return;
            }

            AssetDatabase.DeleteAsset(prefabPath);
            Debug.Log("기존 프리팹 삭제됨: " + prefabPath);
        }

        GameObject newUnitObject = new GameObject(m_RCode);

        NavMeshAgent agent = newUnitObject.AddComponent<NavMeshAgent>();
        agent.enabled = false; // 프리팹 기본값은 비활성화

        newUnitObject.AddComponent<CapsuleCollider>();
        Unit unitComponent = newUnitObject.AddComponent<Unit>();

        // Data 설정
        PropertyInfo dataProperty = typeof(Unit).GetProperty(
            "Data", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (dataProperty != null && dataProperty.CanWrite)
        {
            Character character = new Character
            {
                name = m_UnitName,
                rank = m_Rank,
                atk = m_Atk,
                atkSpeed = m_AtkSpeed,
                atkRange = m_AtkRange,
                moveSpeed = m_MoveSpeed,
                rcode = m_RCode,
                skill = m_UseSkill,
                skillType = m_SelectedSkillType
            };

            dataProperty.SetValue(unitComponent, character);
        }
        else
        {
            Debug.LogError("Data 속성 설정에 실패했습니다.");
            DestroyImmediate(newUnitObject);
            return;
        }

        newUnitObject.AddComponent<UnitController>();

        // Canvas 세팅
        const string canvasPrefabPath = "Assets/03. Prefabs/Character/Canvas.prefab";
        GameObject canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(canvasPrefabPath);
        if (canvasPrefab == null)
        {
            Debug.LogError("Canvas 프리팹 로드 실패: " + canvasPrefabPath);
            DestroyImmediate(newUnitObject);
            return;
        }

        GameObject canvasInstance = PrefabUtility.InstantiatePrefab(canvasPrefab) as GameObject;
        if (canvasInstance == null)
        {
            Debug.LogError("Canvas 프리팹 인스턴스화 실패");
            DestroyImmediate(newUnitObject);
            return;
        }

        canvasInstance.transform.SetParent(newUnitObject.transform, false);

        UnitCommandUI unitCommandUi = newUnitObject.AddComponent<UnitCommandUI>();
        Transform selectedTransform = canvasInstance.transform.Find("Selected");
        if (selectedTransform == null)
        {
            Debug.LogError("Canvas 프리팹에서 'Selected' 오브젝트를 찾을 수 없습니다.");
        }
        else
        {
            unitCommandUi.selectedUI = selectedTransform.gameObject;
        }

        // 모델 피벗 및 프리팹 인스턴스
        GameObject pivot = new GameObject("Pivot");
        pivot.transform.SetParent(newUnitObject.transform, false);
        pivot.transform.localPosition = Vector3.zero;

        GameObject modelInstance = PrefabUtility.InstantiatePrefab(m_SelectedPrefab) as GameObject;
        if (modelInstance == null)
        {
            Debug.LogError("모델 프리팹 인스턴스화 실패");
            DestroyImmediate(newUnitObject);
            return;
        }

        modelInstance.transform.SetParent(pivot.transform, false);

        // AttackSO / SkillAttackSO 설정
        if (m_UseSkill && m_SelectedAttackSO != null && m_SelectedSkillAttackSO != null)
        {
            PropertyInfo attackSoProperty = typeof(Unit).GetProperty(
                "AttackSO", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (attackSoProperty != null && attackSoProperty.CanWrite)
            {
                attackSoProperty.SetValue(unitComponent, m_SelectedAttackSO);
            }
            else
            {
                Debug.LogError("AttackSO 속성 설정에 실패했습니다.");
            }

            PropertyInfo skillAttackSoProperty = typeof(Unit).GetProperty(
                "SkillAttackSO", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (skillAttackSoProperty != null && skillAttackSoProperty.CanWrite)
            {
                skillAttackSoProperty.SetValue(unitComponent, m_SelectedSkillAttackSO);
            }
            else
            {
                Debug.LogError("SkillAttackSO 속성 설정에 실패했습니다.");
            }
        }

        PrefabUtility.SaveAsPrefabAsset(newUnitObject, prefabPath);
        DestroyImmediate(newUnitObject);

        RegisterUnit();
    }

    private void EditUnitPrefab()
    {
        if (m_SelectedPrefab == null)
        {
            Debug.LogError("선택된 유닛 프리팹이 없습니다.");
            return;
        }

        string prefabPath = AssetDatabase.GetAssetPath(m_SelectedPrefab);
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab == null)
        {
            Debug.LogError("기존 프리팹 로드 실패: " + prefabPath);
            return;
        }

        Unit unitComponent = existingPrefab.GetComponent<Unit>();
        if (unitComponent == null)
        {
            Debug.LogError("기존 프리팹에 Unit 컴포넌트가 없습니다.");
            return;
        }

        // Data 설정
        PropertyInfo dataProperty = typeof(Unit).GetProperty(
            "Data", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (dataProperty != null && dataProperty.CanWrite)
        {
            Character character = new Character
            {
                name = m_UnitName,
                rank = m_Rank,
                atk = m_Atk,
                atkSpeed = m_AtkSpeed,
                atkRange = m_AtkRange,
                moveSpeed = m_MoveSpeed,
                rcode = m_RCode,
                skill = m_UseSkill,
                skillType = m_SelectedSkillType
            };

            dataProperty.SetValue(unitComponent, character);
        }
        else
        {
            Debug.LogError("Data 속성 설정에 실패했습니다.");
            return;
        }

        // AttackSO / SkillAttackSO 설정
        if (m_UseSkill && m_SelectedAttackSO != null && m_SelectedSkillAttackSO != null)
        {
            PropertyInfo attackSoProperty = typeof(Unit).GetProperty(
                "AttackSO", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (attackSoProperty != null && attackSoProperty.CanWrite)
            {
                attackSoProperty.SetValue(unitComponent, m_SelectedAttackSO);
            }
            else
            {
                Debug.LogError("AttackSO 속성 설정에 실패했습니다.");
            }

            PropertyInfo skillAttackSoProperty = typeof(Unit).GetProperty(
                "SkillAttackSO", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (skillAttackSoProperty != null && skillAttackSoProperty.CanWrite)
            {
                skillAttackSoProperty.SetValue(unitComponent, m_SelectedSkillAttackSO);
            }
            else
            {
                Debug.LogError("SkillAttackSO 속성 설정에 실패했습니다.");
            }
        }

        PrefabUtility.SavePrefabAsset(existingPrefab);
        Debug.Log("프리팹 수정됨: " + prefabPath);

        RegisterUnit();
    }

    private void UpdateRCode()
    {
        string skillCode = m_UseSkill ? "1" : "0";
        int hash = ComputeStableHash(m_UnitName);
        int shortHash = Math.Abs(hash % 10000);
        m_RCode = $"un{(int)m_Rank}{skillCode}{shortHash:D4}";
    }

    private static int ComputeStableHash(string p_Text)
    {
        if (string.IsNullOrEmpty(p_Text))
        {
            return 0;
        }

        unchecked
        {
            int hash = 23;
            for (int i = 0; i < p_Text.Length; i++)
            {
                hash = hash * 31 + p_Text[i];
            }

            return hash;
        }
    }
}