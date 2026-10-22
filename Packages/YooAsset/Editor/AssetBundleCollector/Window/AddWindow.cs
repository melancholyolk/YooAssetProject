using System;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class AddWindow : EditorWindow
    {

        public AssetCollectorType AssetCollectorType;
        protected int index;
        protected string[] options;

        public void OnInit()
        {
            var collectorType = AssetCollectorType.typeInfos;
            var count = collectorType.Count;
            options = new string[count];
            int index = 0;
            foreach (var type in collectorType)
            {
                options[index++] = type.Type;
            }
        }
    }
}