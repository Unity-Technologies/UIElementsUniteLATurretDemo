using UnityEngine;

public class Turret : MonoBehaviour
{
#if UNITY_EDITOR

    public GameObject Backpack;

    public GameObject Left;
    public GameObject Cockpit;
    public GameObject Right;

    public GameObject Tower;
    public GameObject Base;

    public Material Material;

    // For material preview persistence.

    public GameObject MaterialPreviewBackpack;

    public GameObject MaterialPreviewLeft;
    public GameObject MaterialPreviewCockpit;
    public GameObject MaterialPreviewRight;

    public GameObject MaterialPreviewTower;
    public GameObject MaterialPreviewBase;

#endif // UNITY_EDITOR
}