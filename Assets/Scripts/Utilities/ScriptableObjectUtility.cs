using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectUtility
{

    public static void CreateAsset<T>(T obj, string name, string type) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        asset = obj;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            switch (type) {
                case "attack":
                    path = "Assets/Stats/NPCAttackTypes/";
                    break;
                case "moving":
                    path = "Assets/Stats/MovingTypes/";
                    break;
                case "health":
                    path = "Assets/Stats/HealthType/";
                    break;
                default:
                    return;
            }
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + name + ".asset");
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    public static void OverwriteAsset<T>(T obj, string name) where T : ScriptableObject
    {
        string path = AssetDatabase.GetAssetPath(obj);
        AssetDatabase.RenameAsset(path, name);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
