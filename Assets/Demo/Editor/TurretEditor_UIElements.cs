using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(Turret_UIElements))]
public class TurretEditor_UIElements : Editor
{
    TurretBuilder m_TurretBuilder;

    #region Enable/Disable
    //
    //
    //

    // Hierarchy
    VisualElement m_RootElement;
    VisualTreeAsset m_ModulesVisualTree;

    public void OnEnable()
    {
        m_TurretBuilder = new TurretBuilder(this);

        // Hierarchy
        m_RootElement = new VisualElement();
        m_ModulesVisualTree =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Demo/UI/TurretEditorTemplate.uxml");

        // Styles
        var stylesheet =
            AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Assets/Demo/UI/TurretEditorStyles.uss");
        m_RootElement.styleSheets.Add(stylesheet);

        // WILL BE FIXED: Unset padding on parent InspectorElement.
        m_RootElement.RegisterCallback<AttachToPanelEvent>(OnRootAttachToPanel);
    }

    public void OnDisable()
    {
        m_TurretBuilder.Cleanup();
    }

    //
    //
    //
    #endregion

    #region CreateInspectorGUI()
    //
    //
    //

    public override VisualElement CreateInspectorGUI()
    {
        // Reset root element and reuse.
        var root = m_RootElement;
        root.Clear();

        // Turn the UXML into a VisualElement hierarchy under root.
        m_ModulesVisualTree.CloneTree(root);

        // Create Module Previews
        root.Query(classes: new string[]{ "module-preview" })
            .ForEach((preview) =>
        {
            preview.Add(CreateModuleUI(preview.name));
        });

        // Change Detection
        root.RegisterCallback<ChangeEvent<Object>>(evt =>
        {
            var element = evt.target as ObjectField;
            var moduleName = element.bindingPath;

            if (moduleName == "Material")
            {
                UpdateMaterialPreview();
                m_TurretBuilder.ResetAllModulePreviewEditors();
            }
            else
            {
                m_TurretBuilder.ResetModulePreviewEditor(moduleName);
            }

            m_TurretBuilder.RegenerateTurret();
        });

        // Material Preview
        {
            var turret = target as Turret;
            var material = turret.Material;
            var texture = material?.mainTexture as Texture2D;
            var imageElement = root.Q("MaterialImage");

            imageElement.style.backgroundImage = null;
            if (texture != null)
                imageElement.style.backgroundImage = texture;
            else if (material != null)
                imageElement.style.backgroundColor = material.color;
            else
                imageElement.style.backgroundColor = Color.clear;
        }
        root.Q("MaterialImage").AddManipulator(new TextureDragger());

        // Module Cycling via Scroll Wheel
        root.Query(classes: new string[]{ "preview" }).ForEach((preview) =>
        {
            preview.RegisterCallback<WheelEvent>(evt =>
            {
                var element = evt.currentTarget as VisualElement;
                m_TurretBuilder.CycleModules(element.name, (int)evt.delta.y);
                evt.StopPropagation();
            });
        });

        return root;
    }

    //
    //
    //
    #endregion

    #region CreateModuleUI()
    //
    //
    //

    public VisualElement CreateModuleUI(string moduleName)
    {
        m_TurretBuilder.ResetModulePreviewEditor(moduleName);

        var imguiContainer = new IMGUIContainer(() =>
        {
            var editor = m_TurretBuilder.GetPreviewEditor(moduleName);
            if (editor != null)
                editor.OnInteractivePreviewGUI(
                    GUILayoutUtility.GetRect(180, 180), null);
        });
        imguiContainer.AddToClassList("module-viewport");

        var property = serializedObject.FindProperty(moduleName);
        var field = new PropertyField(property);
        imguiContainer.Add(field);

        return imguiContainer;
    }

    //
    //
    //
    #endregion

    #region Plumbing
    //
    //
    //

    private void UpdateMaterialPreview()
    {
        var turret = target as Turret;
        var imageElement = m_RootElement.Q("MaterialImage");

        if (turret.Material == null)
        {
            imageElement.style.backgroundImage = null;
            return;
        }

        var mainTexture = turret.Material?.GetTexture("_MainTex");
        imageElement.style.backgroundImage = mainTexture as Texture2D;
    }

    public void SetVisualTree(VisualTreeAsset asset)
    {
        m_ModulesVisualTree = asset;
    }

    public override bool UseDefaultMargins()
    {
        return false;
    }

    private void OnRootAttachToPanel(AttachToPanelEvent evt)
    {
        var inspectorElement = (evt.target as VisualElement).GetFirstAncestorOfType<InspectorElement>();
        if (inspectorElement == null)
            return;

        inspectorElement.style.paddingBottom = 0;
        inspectorElement.style.paddingLeft = 0;
        inspectorElement.style.paddingRight = 0;
        inspectorElement.style.paddingTop = 0;
    }

    //
    //
    //
    #endregion
}
