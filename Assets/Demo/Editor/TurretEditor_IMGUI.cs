using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(Turret_IMGUI))]
public class TurretEditor_IMGUI : Editor
{
    TurretBuilder m_TurretBuilder;

    // Hierarchy
    enum Module { Backpack, Left, Right, Cockpit, Tower, Base }
    private Rect m_MaterialPreviewRect;
    private Rect[] m_ModulePreviewRects;
    private Rect[] m_ModulePropertyFieldRects;

    // Styles
    GUIStyle m_RowStyle;
    GUIStyle m_MaterialPreviewStyle;
    GUIStyle m_PreviewBorderStyle;
    static readonly float k_MaterialRowHeight = 80f;
    static readonly float k_PreviewSize = 180f;
    static readonly float k_BorderWidth = 2f;

    public void OnEnable()
    {
        m_TurretBuilder = new TurretBuilder(this);

        // Hierarchy
        var moduleCount = Enum.GetValues(typeof(Module)).Length;
        m_ModulePreviewRects = new Rect[moduleCount];
        m_ModulePropertyFieldRects = new Rect[moduleCount];

        // Styles
        { // Create module row style with background color.
            var background = new Texture2D(1, 1);
            background.SetPixel(0, 0, new Color(0.192f, 0.192f, 0.192f));
            background.Apply();
            var state = new GUIStyleState();
            state.background = background;

            m_RowStyle = new GUIStyle();
            m_RowStyle.margin = new RectOffset(0, 0, 1, 1);
            m_RowStyle.normal = state;
        }

        { // Material preview background.
            var state = new GUIStyleState();
            m_MaterialPreviewStyle = new GUIStyle();
            m_MaterialPreviewStyle.normal = state;
        }
        { // Create hover border style.
            var background = new Texture2D(1, 1);
            background.SetPixel(0, 0, new Color(1f, 1f, 1f));
            background.Apply();
            var state = new GUIStyleState();
            state.background = background;

            m_PreviewBorderStyle = new GUIStyle();
            m_PreviewBorderStyle.normal = state;
        }
    }

    public override void OnInspectorGUI()
    {
        // Save then set global style state.
        var originalLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 60;

        using (new GUILayout.VerticalScope(m_RowStyle))
        {
            CreateMaterialPreview();
        }

        using (new GUILayout.HorizontalScope(m_RowStyle))
        {
            GUILayout.FlexibleSpace();
            CreatePreviewWindow(Module.Backpack);
            GUILayout.FlexibleSpace();
        }

        using (new GUILayout.HorizontalScope(m_RowStyle))
        {
            GUILayout.FlexibleSpace();
            CreatePreviewWindow(Module.Left);
            CreatePreviewWindow(Module.Right);
            GUILayout.FlexibleSpace();
        }

        using (new GUILayout.HorizontalScope(m_RowStyle))
        {
            GUILayout.FlexibleSpace();
            CreatePreviewWindow(Module.Cockpit);
            GUILayout.FlexibleSpace();
        }

        using (new GUILayout.HorizontalScope(m_RowStyle))
        {
            GUILayout.FlexibleSpace();
            CreatePreviewWindow(Module.Tower);
            GUILayout.FlexibleSpace();
        }

        using (new GUILayout.HorizontalScope(m_RowStyle))
        {
            GUILayout.FlexibleSpace();
            CreatePreviewWindow(Module.Base);
            GUILayout.FlexibleSpace();
        }

        // Restore global styles.
        EditorGUIUtility.labelWidth = originalLabelWidth;
    }

    // Draw Material Preview
    void CreateMaterialPreview()
    {
        var evt = Event.current;
        var property = serializedObject.FindProperty("Material");
        var rowRect = GUILayoutUtility.GetRect(k_MaterialRowHeight, k_MaterialRowHeight);

        // Need to save this rect during Repaint and use it during Layout for the AreaScope,
        // otherwise the AreaScope is 1 by 1 px.
        if (Event.current.type == EventType.Repaint)
            m_MaterialPreviewRect = rowRect;

        var hoverPreviewBorderStyle = GUIStyle.none;
        if (rowRect.Contains(evt.mousePosition))
            hoverPreviewBorderStyle = m_PreviewBorderStyle;

            // Scroll wheel can cycle through modules.
        if (rowRect.Contains(evt.mousePosition)
            && evt.type == EventType.ScrollWheel)
        {
            m_TurretBuilder.CycleModules("Material", (int)evt.delta.y);
            m_TurretBuilder.ResetAllModulePreviewEditors();
            m_TurretBuilder.RegenerateTurret();
            evt.Use();
        }

        using (var previewScope = new GUILayout.AreaScope(m_MaterialPreviewRect))
        {
            var material = property.objectReferenceValue as Material;
            var currentMaterialTexture = material?.mainTexture as Texture2D;

            if (currentMaterialTexture != null)
                m_MaterialPreviewStyle.normal.background = currentMaterialTexture;
            else if (material != null)
            {
                var background = new Texture2D(1, 1);
                background.SetPixel(0, 0, material.color);
                background.Apply();
                m_MaterialPreviewStyle.normal.background = background;
            }

            GUI.Box(new Rect(-30, -330, 1000, 1000), GUIContent.none, m_MaterialPreviewStyle);
        }

        using (var previewScope = new GUILayout.AreaScope(m_MaterialPreviewRect))
        {
            // Top hover border.
            GUILayout.Box(
                GUIContent.none, hoverPreviewBorderStyle,
                GUILayout.Height(k_BorderWidth));

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(property, GUILayout.Width(k_PreviewSize));
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        m_TurretBuilder.ResetAllModulePreviewEditors();
                        m_TurretBuilder.RegenerateTurret();
                    }
                }
                GUILayout.FlexibleSpace();
            }

            // Bottom hover border.
            GUILayout.Box(
                GUIContent.none, hoverPreviewBorderStyle,
                GUILayout.Height(k_BorderWidth));
        }
    }

    // Draw Turret Module Preview
    void CreatePreviewWindow(Module module)
    {
        // Local Vars
        var moduleName = module.ToString();
        var moduleIdx = (int)module;
        var evt = Event.current;

        // Hover Border Change
        var hoverPreviewBorderStyle = GUIStyle.none;
        if (m_ModulePreviewRects[moduleIdx].Contains(evt.mousePosition))
            hoverPreviewBorderStyle = m_PreviewBorderStyle;

        // Module Cycling via Scroll Wheel
        if (m_ModulePreviewRects[moduleIdx].Contains(evt.mousePosition)
            && evt.type == EventType.ScrollWheel)
        {
            m_TurretBuilder.CycleModules(moduleName, (int)evt.delta.y);
            m_TurretBuilder.ResetModulePreviewEditor(moduleName);
            m_TurretBuilder.RegenerateTurret();
            evt.Use();
        }

        using (new GUILayout.VerticalScope())
        {
            // Top hover border.
            GUILayout.Box(
                GUIContent.none, hoverPreviewBorderStyle,
                GUILayout.Height(k_BorderWidth));

            var previewRect =
                GUILayoutUtility.GetRect(k_PreviewSize, k_PreviewSize);

            // Need to save this rect during Repaint and use it during Layout for the AreaScope.
            if (evt.type == EventType.Repaint)
                m_ModulePreviewRects[moduleIdx] = previewRect;

            // Don't draw the interactive preview when the mouse is over the PropertyField
            // otherwise the preview steals the MouseDownEvent.
            var editor = m_TurretBuilder.GetPreviewEditor(moduleName);
            if (editor != null
                && !(evt.type == EventType.MouseDown
                    && m_ModulePropertyFieldRects[moduleIdx].Contains(evt.mousePosition)))
            {
                editor.OnInteractivePreviewGUI(previewRect, null);
            }

            using (new GUILayout.AreaScope(m_ModulePreviewRects[moduleIdx]))
            {
                GUILayout.FlexibleSpace();

                var property = serializedObject.FindProperty(moduleName);
                var fieldRect = EditorGUILayout.GetControlRect();

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(fieldRect, property);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    m_TurretBuilder.ResetModulePreviewEditor(moduleName);
                    m_TurretBuilder.RegenerateTurret();
                }

                if (Event.current.type == EventType.Repaint)
                {
                    // Translate the filed rect to be in "world" space outside the AreaScope.
                    fieldRect.x += m_ModulePreviewRects[moduleIdx].x;
                    fieldRect.y += m_ModulePreviewRects[moduleIdx].y;
                    m_ModulePropertyFieldRects[(int)module] = fieldRect;
                }
            }

            // Bottom hover border.
            GUILayout.Box(
                GUIContent.none, hoverPreviewBorderStyle,
                GUILayout.Height(k_BorderWidth));
        }
    }

    #region Inspector Plumbing
    //
    //
    //

    public void OnDisable()
    {
        m_TurretBuilder.Cleanup();
    }

    public override bool UseDefaultMargins()
    {
        return false;
    }

    public override bool RequiresConstantRepaint()
    {
        // So that the hover effect and Scroll Wheel changes are instant.
        return true;
    }

    //
    //
    //
    #endregion
}
