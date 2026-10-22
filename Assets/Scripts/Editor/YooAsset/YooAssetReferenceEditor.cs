using System;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;
using Object = UnityEngine.Object;

namespace ACT
{
    [CustomPropertyDrawer(typeof(YooAssetReference))]
    public class YooAssetReferenceEditor : PropertyDrawer
    {
        private bool _showErrorInfo = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty assetIdProp = property.FindPropertyRelative("_guid");

            Rect mainLineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect labelRect = new Rect(mainLineRect.x, mainLineRect.y, EditorGUIUtility.labelWidth, mainLineRect.height);
            Rect fieldRect = new Rect(mainLineRect.x + EditorGUIUtility.labelWidth, mainLineRect.y,
                mainLineRect.width - EditorGUIUtility.labelWidth, mainLineRect.height);

            EditorGUI.LabelField(labelRect, label);

            var assetType = typeof(Object);
            var propertyType = fieldInfo?.FieldType;

            if (propertyType != null && propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(YooAssetReference<>))
            {
                assetType = propertyType.GetGenericArguments()[0];
            }

            Object currentAsset = null;
            _showErrorInfo = false;
            if (!string.IsNullOrEmpty(assetIdProp.stringValue))
            {
                if (AssetAddressHelper.TryGetAssetInfoByGuid(assetIdProp.stringValue, out var assetInfo))
                {
                    currentAsset = AssetDatabase.LoadAssetAtPath<Object>(assetInfo.AssetInfo.AssetPath);
                }
                else
                {
                    if (!string.IsNullOrEmpty(assetIdProp.stringValue))
                        currentAsset =AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(assetIdProp.stringValue));
                    _showErrorInfo = true;
                }
            }

            EditorGUI.BeginChangeCheck();
            var newAsset = EditorGUI.ObjectField(fieldRect, currentAsset, assetType, true);
            if (EditorGUI.EndChangeCheck())
            {
                if (newAsset == null)
                {
                    assetIdProp.stringValue = null;
                }
                else
                {
                    string assetPath = AssetDatabase.GetAssetPath(newAsset);
                    var guid = AssetDatabase.AssetPathToGUID(assetPath);

                    if (AssetAddressHelper.TryGetAssetInfoByGuid(guid,out var assetInfo))
                    {
                        assetIdProp.stringValue = assetInfo.AssetInfo.AssetGUID;
                    }
                    else
                    {
                        if (AssetAddressHelper.TryGetAssetInfoByGuid(guid,out var assetInfo2,true))
                        {
                            assetIdProp.stringValue = assetInfo2.AssetInfo.AssetGUID;
                        }
                        Debug.LogError($"Asset {newAsset.name} is not a valid YooAsset reference.");
                        EditorUtility.DisplayDialog("Invalid Asset",
                            $"Asset {newAsset.name} is not a valid YooAsset reference.", "OK");
                    }
                }
            }

            if (_showErrorInfo)
            {
                Rect errorRect = new Rect(
                    position.x,
                    position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                    position.width,
                    EditorGUIUtility.singleLineHeight
                );
                string showInfo = "";
                if (string.IsNullOrEmpty(assetIdProp.stringValue))
                    showInfo = assetIdProp.stringValue;
                else
                    showInfo = AssetDatabase.GUIDToAssetPath(assetIdProp.stringValue);
                GUI.color = new Color(1, 0.7f, 0.7f);
                EditorGUI.HelpBox(errorRect, "Invalid YooAsset Asset: " + showInfo, MessageType.Error);
                GUI.color = Color.white;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _showErrorInfo
                ? EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing
                : EditorGUIUtility.singleLineHeight;
        }
    }
}