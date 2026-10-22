using System;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    // public class AddGroupWindow : AddWindow
    // {
    //     public static void Open(AssetBundleCollectorPackage package, Action<string,long> onAccept)
    //     {
    //         var window = GetWindow<AddGroupWindow>(true);
    //         window.maxSize = new Vector2(300, 100);
    //         window.onAccept = onAccept;
    //         window.selectedPackage = package;
    //         window.AssetCollectorType = package.AssetCollectorType;
    //         window.OnInit();
    //     }
    //
    //     public AssetBundleCollectorPackage selectedPackage;
    //     public string name = "Default Group";
    //     private long key;
    //     private Action<string,long> onAccept;
    //
    //     private void OnGUI()
    //     {
    //         name = EditorGUILayout.TextField("名称",name);
    //         GUILayout.FlexibleSpace();
    //         if (GUILayout.Button("OK"))
    //         {
    //             key = selectedPackage.GetColldctorKey(0);
    //             if (key >= (1 << ParameterDefine.GroupKeyLength))
    //                 throw new Exception("Group长度超过规定长度");
    //             
    //             onAccept?.Invoke(name,key);
    //             this.Close();
    //         }
    //     }
    // }
}
