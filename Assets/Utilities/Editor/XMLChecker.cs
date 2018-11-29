using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class XMLChecker : AssetPostprocessor
 {
     
    static void OnPostprocessAllAssets (
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths) 
    {
        foreach (string str in importedAssets)
        {
            //Debug.Log("Reimported Asset: " + str);
            string[] splitStr = str.Split('/', '.');
             
            string folder = splitStr[splitStr.Length-3];
            string fileName = splitStr[splitStr.Length-2];
            string extension = splitStr[splitStr.Length-1];
            //Debug.Log("File name: " + fileName);
            //Debug.Log("File type: " + extension);

            if (extension == "uxml")
            {
                var activeEditors = ActiveEditorTracker.sharedTracker?.activeEditors;
                var editor = activeEditors?.FirstOrDefault((e) => e.target.GetType() == typeof(Turret));
                if (editor != null)
                {
                    var turretEditor = editor as TurretEditor_UIElements;
                    turretEditor.SetVisualTree(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Demo/UI/" + "TurretEditorTemplate.uxml"));
                    turretEditor.CreateInspectorGUI().Bind(editor.serializedObject);
                }
            }
        }
    }
 }
