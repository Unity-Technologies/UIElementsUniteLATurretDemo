using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TurretBuilder
{
    private Turret m_Turret;
    private SerializedObject m_SerializedObject;

    private Dictionary<string, Editor> m_ModuleEditors;

    public TurretBuilder(Editor editor)
    {
        m_Turret = editor.target as Turret;
        m_SerializedObject = editor.serializedObject;
        m_ModuleEditors = new Dictionary<string, Editor>();

        if (m_Turret.Material == null)
        {
            CycleModules("Material", 0);
        }
    }

    public void Cleanup()
    {
        foreach (var editor in m_ModuleEditors.Values)
            if (editor != null)
                Editor.DestroyImmediate(editor);

        m_ModuleEditors.Clear();
    }

    #region Turret Generation
    //
    //
    // 

    private void RegenerateTurretInternal()
    {
        if (m_Turret == null)
            return;

        if (m_Turret.transform.childCount > 0)
        {
            var child = m_Turret.transform.GetChild(0).gameObject;
            GameObject.DestroyImmediate(child);
        }

        var @base = PrefabUtility.InstantiatePrefab(m_Turret.Base) as GameObject;
        if (@base != null)
        {
            @base.transform.SetParent(m_Turret.transform, false);
            var selector = @base.AddComponent<TurretRootSelector>();
            selector.turretRootObject = m_Turret.gameObject;
        }

        ApplyMaterialToObject(@base, m_Turret.Material);

        var tower = InstantiatePrefab(m_Turret.Tower, @base, "Mount_Top");
        if (tower == null)
            return;

        var cockpit = InstantiatePrefab(m_Turret.Cockpit, tower, "Mount_Top");
        if (cockpit == null)
            return;

        var leftWeapon = InstantiatePrefab(m_Turret.Left, cockpit, "Mount_Weapon_R");
        if (leftWeapon == null)
            return;

        var rightWeapon = InstantiatePrefab(m_Turret.Right, cockpit, "Mount_Weapon_L");
        if (rightWeapon == null)
            return;

        var backpack = InstantiatePrefab(m_Turret.Backpack, cockpit, "Mount_Backpack");
        if (backpack == null)
            return;
    }

    public void RegenerateTurret()
    {
        RegenerateTurretInternal();
        SceneView.RepaintAll();
    }

    public void ApplyMaterialToObject(GameObject obj, Material mat)
    {
        if (m_Turret == null)
            return;

        if (obj == null || mat == null)
            return;

        foreach (var renderer in obj.GetComponentsInChildren<MeshRenderer>())
            renderer.material = m_Turret.Material;

        foreach (var renderer in obj.GetComponentsInChildren<SkinnedMeshRenderer>())
            renderer.material = m_Turret.Material;
    }

    //
    //
    //
    #endregion Turret Generation

    #region Preview Editor Utilities
    //
    //
    //

    public Editor GetPreviewEditor(string moduleName)
    {
        if (!m_ModuleEditors.ContainsKey(moduleName) || m_ModuleEditors[moduleName] == null)
            ResetModulePreviewEditor(moduleName);

        return m_ModuleEditors[moduleName];
    }

    public void CycleModules(string moduleName, int increment)
    {
        var property = m_SerializedObject.FindProperty(moduleName);
        var instance = property.objectReferenceValue;

        if (moduleName.Contains("Left") || moduleName.Contains("Right"))
            moduleName = "Weapon";

        var assetGuids = AssetDatabase.FindAssets(null, new string[]{ "Assets/Modules/" + moduleName + "s" }).ToList();
        assetGuids.Sort();

        int newIndex = 0;
        if (instance != null)
        {
            var assetPath = AssetDatabase.GetAssetOrScenePath(instance);
            var prefabGuid = AssetDatabase.AssetPathToGUID(assetPath);
            int currentIndex = assetGuids.IndexOf(prefabGuid);
            newIndex = (currentIndex + (increment > 0 ? 1 : -1)) % assetGuids.Count();
            if (newIndex < 0)
                newIndex = assetGuids.Count() + newIndex;
        }

        var newGuid = assetGuids.ElementAt(newIndex);
        var newAssetPath = AssetDatabase.GUIDToAssetPath(newGuid);
        property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Object>(newAssetPath);
        m_SerializedObject.ApplyModifiedProperties();
    }

    public void ResetAllModulePreviewEditors()
    {
        var keys = m_ModuleEditors.Keys.ToList();
        foreach (var key in keys)
        {
            ResetModulePreviewEditor(key);
        }
    }

    public void ResetModulePreviewEditor(string moduleName)
    {
        DeletePreviewObject(moduleName);

        var previewObj = GetPreviewGameObject(moduleName);
        ApplyMaterialToObject(previewObj, m_Turret.Material);

        if (!m_ModuleEditors.ContainsKey(moduleName))
            m_ModuleEditors.Add(moduleName, null); 

        var editor = m_ModuleEditors[moduleName];
        if (editor != null)
            Editor.DestroyImmediate(editor);

        m_ModuleEditors[moduleName] = Editor.CreateEditor(previewObj);
    }

    //
    //
    //
    #endregion Preview Editor Utilities

    // private

    private void DeletePreviewObject(string moduleName)
    {
        var previewObjectProperty = m_SerializedObject.FindProperty("MaterialPreview" + moduleName);
        var previewObj = previewObjectProperty.objectReferenceValue as GameObject;
        if (previewObj == null)
            return;

        GameObject.DestroyImmediate(previewObj);
        previewObjectProperty.objectReferenceValue = null;
        m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private GameObject GetPreviewGameObject(string moduleName)
    {
        var objectProperty = m_SerializedObject.FindProperty(moduleName);
        var obj = objectProperty.objectReferenceValue as GameObject;
        if (obj == null)
        {
            CycleModules(moduleName, 0);
            obj = objectProperty.objectReferenceValue as GameObject;
        }

        var previewObjectProperty = m_SerializedObject.FindProperty("MaterialPreview" + moduleName);
        var previewObj = previewObjectProperty.objectReferenceValue as GameObject;
        if (previewObj == null)
        {
            previewObj = GameObject.Instantiate(obj) as GameObject;
            previewObj.name = "MODULE_PREVIEW_OBJ";
            previewObj.hideFlags = HideFlags.HideAndDontSave;
            previewObj.transform.Translate(new Vector3(1000, 0, 0));
            previewObjectProperty.objectReferenceValue = previewObj;
            m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        return previewObj;
    }

    private GameObject InstantiatePrefab(GameObject obj, GameObject parent, string parentMountName)
    {
        if (parent == null)
            return null;

        var mountPoint = parent.transform.Find(parentMountName);
        if (mountPoint == null)
            return null;

        var instance = PrefabUtility.InstantiatePrefab(obj) as GameObject;
        if (instance != null)
        {
            instance.transform.SetParent(mountPoint.transform, false);
            var selector = instance.AddComponent<TurretRootSelector>();
            selector.turretRootObject = m_Turret.gameObject;
        }

        ApplyMaterialToObject(instance, m_Turret.Material);

        return instance;
    }
}
