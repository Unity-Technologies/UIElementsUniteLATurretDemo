using UnityEditor;

[CustomEditor(typeof(TurretRootSelector))]
public class TurretRootSelectorEditor : Editor
{
    public void OnEnable()
    {
        var selector = target as TurretRootSelector;
        if (selector != null && selector.turretRootObject != null)
            Selection.activeObject = selector.turretRootObject;
    }
}
