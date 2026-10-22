using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace YooAsset.Editor
{
    public static class AssetAddressHelper
    {
        private static Dictionary<string, CollectAssetInfo> _addressGUIDMap;
        private static Dictionary<string, CollectAssetInfo> _addressPathMap;
        private static Dictionary<string, CollectAssetInfo> _addressMap;
        private static double _lastRequireTime = 0f;
        
        static AssetAddressHelper()
        {
            EditorSceneManager.sceneSaving += (_,_) => { CheckClick(); };
            EditorSceneManager.sceneSaved += (_) => { CheckClick(); };

            AssetDatabase.importPackageCompleted += (_) => { CheckClick(); };
            EditorApplication.projectChanged += () => { CheckClick(); };
        }
        
        static void CheckClick()
        {
            Event e = Event.current;
            if (e != null && e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.S)
            {
                _addressGUIDMap = null;
                _addressPathMap = null;
                _addressMap = null;
            }
        }
        public static RuleDisplayName GetIgnoreRuleIndex(List<RuleDisplayName> ignoreRuleList, string ruleName)
        {
            for (int i = 0; i < ignoreRuleList.Count; i++)
            {
                if (ignoreRuleList[i].ClassName == ruleName)
                    return ignoreRuleList[i];
            }
            return ignoreRuleList[0];
        }
        private static void GetAssetAddressMap(bool forceUpdate = false)
        {
            var requireTime = EditorApplication.timeSinceStartup;
            var delta = requireTime - _lastRequireTime;

            if (!forceUpdate)
            {
                if (_addressGUIDMap != null && _addressPathMap != null && _addressMap != null && delta < 300f)
                    return;
            }

            _lastRequireTime = EditorApplication.timeSinceStartup;      

            _addressGUIDMap ??= new();
            _addressGUIDMap.Clear();

            _addressPathMap ??= new();
            _addressPathMap.Clear();
            
            _addressMap ??= new();
            _addressMap.Clear();
            
            var setting = AssetBundleCollectorSettingData.Setting;
            if (setting == null)
                return;

            var ignoreRuleList = AssetBundleCollectorSettingData.GetIgnoreRuleNames();
            
            foreach (var package in setting.Packages)
            {
                var ignoreRule = AssetBundleCollectorSettingData.GetIgnoreRuleInstance(GetIgnoreRuleIndex(ignoreRuleList, package.IgnoreRuleName).ClassName);
                var command = new CollectCommand(package.PackageName,
                    ignoreRule);
                command.SimulateBuild = true;
                command.UniqueBundleName = setting.UniqueBundleName;
                command.UseAssetDependencyDB = true;
                command.EnableAddressable = package.EnableAddressable;
                command.LocationToLower = package.LocationToLower;
                command.IncludeAssetGUID = package.IncludeAssetGUID;
                command.AutoCollectShaders = package.AutoCollectShaders;

                foreach (var group in package.Groups)
                {
                    foreach (var collector in group.Collectors)
                    {
                        try
                        {
                            collector.CheckConfigError();
                            foreach (var asset in collector.GetAllCollectAssets(command, group))
                            {
                                _addressGUIDMap[asset.AssetInfo.AssetGUID] = asset;
                                _addressPathMap[asset.AssetInfo.AssetPath] = asset;
                                _addressMap[asset.Address] = asset;
                            }
                        }
                        catch(Exception e)
                        {
                            Debug.LogError(collector.CollectPath + $"有问题 {e}");
                            // ignored
                        }
                    }
                }
            }
        }
        
        private const string CopyAssetAddressMenu = "Assets/YooAsset/CopyAssetAddress";
        private const string CopyAssetAliasMenu = "Assets/YooAsset/CopyAssetAlias";
        
        [MenuItem(CopyAssetAddressMenu, true)]
        [MenuItem(CopyAssetAliasMenu, true)]
        private static bool CheckSelectAssetIsManagedByYooAsset()
        {
            var guids = Selection.assetGUIDs;
            if (guids.Length == 0)
            {
                return false;
            }
            GetAssetAddressMap();
            var guid = guids[0];
            var map = _addressGUIDMap;
            
            return map.ContainsKey(guid);
        }

        public static bool TryGetAssetInfoByGuid(string guid, out CollectAssetInfo assetInfo,bool forceUpdate = false)
        {
            GetAssetAddressMap(forceUpdate);
            return _addressGUIDMap.TryGetValue(guid, out assetInfo);
        }
        
        public static bool TryGetAssetInfoByPath(string id, out CollectAssetInfo assetInfo,bool forceUpdate = false)
        {
            GetAssetAddressMap(forceUpdate);
            return _addressPathMap.TryGetValue(id, out assetInfo);
        }
        
        public static bool TryGetAssetInfoByAddress(string id, out CollectAssetInfo assetInfo,bool forceUpdate = false)
        {
            GetAssetAddressMap(forceUpdate);
            return _addressMap.TryGetValue(id, out assetInfo);
        }

        public static Dictionary<string, CollectAssetInfo> GetAssetAddressMapWithGuid(bool forceUpdate = false)
        {
            GetAssetAddressMap(forceUpdate);
            return _addressGUIDMap;
        }

        public static Dictionary<string,CollectAssetInfo> GetAssetAddressMapWithPath(bool forceUpdate = false)
        {
            GetAssetAddressMap(forceUpdate);
            return _addressPathMap;
        }
        
        public static Dictionary<string,CollectAssetInfo> GetAssetAddressMapWithAddress(bool forceUpdate = false)
        {
            GetAssetAddressMap(forceUpdate);
            return _addressMap;
        }

        [MenuItem(CopyAssetAliasMenu)]
        static void CopyAssetAlias()
        {
            if (!TryGetAssetInfoByGuid(Selection.assetGUIDs[0], out var assetInfo)) 
                return;
            Debug.Log($"Copy Asset Alias => '{ assetInfo.Address }'");
            GUIUtility.systemCopyBuffer = assetInfo.Address;
        }
        
        [MenuItem(CopyAssetAddressMenu)]
        static void CopyAssetAddress()
        {
            if (!TryGetAssetInfoByGuid(Selection.assetGUIDs[0], out var assetInfo)) 
                return;
            Debug.Log($"Copy Asset Address => '{ assetInfo.Address }'");
            GUIUtility.systemCopyBuffer = assetInfo.Address.ToString();
        }

        public static T AssetDataBaseLoadAssetByGuid<T>(string guid) where T : UnityEngine.Object
        {
            if (TryGetAssetInfoByGuid(guid,
                    out var assetInfo))
            {
                return AssetDatabase.LoadAssetAtPath<T>(assetInfo.AssetInfo.AssetPath);
            }

            return null;
        }
        
        [MenuItem("YooAsset/By Rules/Refresh All Group", false, 10001)]
        public static void RefreshAllCollector()
        {
            foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
            {
                foreach (var group in package.Groups)
                {
                    group.RebuildCollector();
                }
            }
            EditorUtility.SetDirty(AssetBundleCollectorSettingData.Setting);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}