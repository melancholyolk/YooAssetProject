using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class EditorTypeWindow : EditorWindow
    {
        private string selectedType = "";
        private Vector2 scroll;
        private string _editKey;
        private AssetCollectorType _collectorType;
        
        public static void Open(AssetCollectorType collectorType)
        {
            var window = GetWindow<EditorTypeWindow>();
            window._collectorType = collectorType;
        }

        private void OnGUI()
        {
            {
                var firstLine = EditorGUILayout.GetControlRect();

                EditorGUI.DrawRect(firstLine, new Color(0, 0, 0, 0.5f));

                var keyRect = firstLine;
                keyRect.width /= 2;
                GUI.Label(keyRect, "Type", "CenteredLabel");

                var valueRect = firstLine;
                valueRect.xMin = firstLine.xMax - (firstLine.width / 2) + 5;
                GUI.Label(valueRect, "ID", "CenteredLabel");
            }

            {
                scroll = EditorGUILayout.BeginScrollView(scroll);
                foreach (var pair in _collectorType.typeInfos)
                {
                    var type = pair.Type;
                    var key = pair.Key;
                    var line = EditorGUILayout.GetControlRect();
                    if (EditorGUI.Toggle(line, GUIContent.none, selectedType == pair.Type, "ToolbarButton"))
                    {
                        selectedType = pair.Type;
                    }

                    line.xMin += 20;
                    var keyRect = line;
                    keyRect.width /= 2;

                    var valueRect = line;
                    valueRect.xMin = line.xMax - (line.width / 2) + 5;

                    EditorGUI.BeginChangeCheck();
                    var newKey = EditorGUI.TextField(keyRect, pair.Type);
                    if (EditorGUI.EndChangeCheck() && !_collectorType.ContainType(newKey))
                    {
                        EditorApplication.delayCall += () =>
                        {
                            _collectorType.RemoveByType(type);
                            _collectorType.Add(newKey, key);
                        };
                    }


                    EditorGUI.BeginChangeCheck();
                    var newValue = EditorGUI.IntField(valueRect, pair.Key);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorApplication.delayCall += () =>
                        {
                            _collectorType[type].Key = newValue;
                        };
                    }
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            _editKey = EditorGUILayout.TextField(_editKey);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Add"))
            {
                for (int i = 1; i < (1 << 10) ; i++)
                {
                    if(!_collectorType.ContainKey(i))
                    {
                        _collectorType.Add(_editKey, i);
                        break;
                    }
                }
            }

            if (GUILayout.Button("Remove"))
            {
                _collectorType.RemoveByType(selectedType);
            }
        }
    }
}