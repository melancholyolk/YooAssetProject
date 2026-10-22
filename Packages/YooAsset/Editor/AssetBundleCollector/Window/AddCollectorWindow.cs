using System;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class AddCollectorWindow : AddWindow
    {
        public static void Open(AssetBundleCollectorGroup collector,Action onAccept)
        {
            var window = GetWindow<AddCollectorWindow>(true);
            window.maxSize = new Vector2(300, 100);
            window.onAccept = onAccept;
            window.AssetBundleCollector = collector;
            window.AssetCollectorType = collector.AssetCollectorType;
            window.OnInit();
        }
        
        public AssetBundleCollectorGroup AssetBundleCollector;
        // private int key;
        // private Action<int> onAccept;
        private Action onAccept;

        private void OnGUI()
        {
            index = EditorGUILayout.Popup("类型",index,options);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("OK"))
            {
                // key = AssetBundleCollector.GetCollectorKey(AssetCollectorType[options[index]].Key);
                onAccept?.Invoke();
                this.Close();
            }
        }
    }
}
