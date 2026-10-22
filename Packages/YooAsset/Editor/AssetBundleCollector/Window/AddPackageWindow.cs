using System;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{

    // public class AddPackageWindow : AddWindow
    // {
    //     public static void Open(Action<string,int> onAccept)
    //     {
    //         var window = GetWindow<AddPackageWindow>(true);
    //         window.maxSize = new Vector2(300, 100);
    //         window.onAccept = onAccept;
    //         window.AssetCollectorType = AssetBundleCollectorSettingData.Setting.AssetCollectorType;
    //         window.OnInit();
    //     }
    //
    //     public string name = "DefaultPackage";
    //     private int key;
    //     private Action<string,int> onAccept;
    //
    //     private void OnGUI()
    //     {
    //         name = EditorGUILayout.TextField("名称",name);
    //         GUILayout.FlexibleSpace();
    //         if (GUILayout.Button("OK")) 
    //         {
    //             key = AssetBundleCollectorSettingData.Setting.GetColldctorPackageKey();
    //             if(key >= (1 << ParameterDefine.PackageKeyLength))
    //                 throw new Exception("Package长度超过规定长度");
    //             onAccept?.Invoke(name,key);
    //             this.Close();
    //         }
    //     }
    // }
}
