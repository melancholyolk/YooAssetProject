using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace YooAsset.Editor
{
    [Serializable]
    class YooAssetPackageBuildSettings : ScriptableObject
    {
        // private static string SettingPath => Path.Combine(Application.dataPath, "Settings", "YooAssetPackageBuildSettings.asset");
        private static string SettingPath => Path.Combine(Environment.CurrentDirectory, "ProjectSettings", "YooAssetPackageBuildSettings.asset");
        
        private static YooAssetPackageBuildSettings _setting;
        private static YamlDotNet.Serialization.IDeserializer _deserializer;
        private static YamlDotNet.Serialization.ISerializer _serializer;
        
        public static YooAssetPackageBuildSettings LoadOrCreate()
        {
            if (_setting)
            {
                return _setting;
            }
            
            // try
            {
                // var guids = AssetDatabase.FindAssets($"t:{nameof(YooAssetPackageBuildSettings)}");
                // if (guids.Length > 0)
                // {
                //     _setting = AssetDatabase.LoadAssetAtPath<YooAssetPackageBuildSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
                // }
                
                if (UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(SettingPath).FirstOrDefault() is not YooAssetPackageBuildSettings settings)
                {
                    settings = CreateInstance<YooAssetPackageBuildSettings>();
                }
                
                _setting = settings;
            }
            // catch(Exception e)
            // {
            //     _setting = null;
            // }
            
            return _setting;
        }

        public void Save()
        {
            UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new Object[]{this}, SettingPath,true);
            // if (File.Exists(SettingPath))
            // {
            //     EditorUtility.SetDirty(this);
            //     AssetDatabase.SaveAssetIfDirty(this);
            // }
            // else
            // {
            //     AssetDatabase.CreateAsset(this, SettingPath);
            //     AssetDatabase.Refresh();
            // }
        }
        
        [Serializable]
        class PipelineSetting
        {
            public bool useAssetDependencyDB;
            public bool clearBuildCache;
            public string encryptionClassName;
            public string buildingFileCopyParams;
            public EBuildingFileCopyOption buildingFileCopyOption;
            public EFileNameStyle fileNameStyle;
            public ECompressOption compressOption;
        }
        
        [Serializable]
        class PackageSetting
        {
            [SerializeField]
            private List<string> pipelineNames = new();

            [SerializeField] 
            private List<PipelineSetting> pipelineSettings = new();

            public string pipeline = nameof(EBuildPipeline.ScriptableBuildPipeline);

            public PipelineSetting GetOrCreatePipeline(string buildPipeline)
            {
                var index = pipelineNames.IndexOf(buildPipeline);
                if (index >= 0)
                {
                    return pipelineSettings[index];
                }
                
                pipelineNames.Add(buildPipeline);
                var newSetting = new PipelineSetting();
                pipelineSettings.Add(newSetting);
                return newSetting;
            }
        }
        
        [SerializeField]
        private List<string> packageNames = new ();
        [SerializeField]
        private List<PackageSetting> packageSettings = new ();

        private PackageSetting GetOrCreateSetting(string packageName)
        {
            var index = packageNames.IndexOf(packageName);
            if (index >= 0)
            {
                return packageSettings[index];
            }
            packageNames.Add(packageName);
            var newSetting = new PackageSetting();
            packageSettings.Add(newSetting);
            return newSetting;
        }
        private PipelineSetting GetOrCreateSetting(string packageName, string buildPipeline)
        {
            var setting = GetOrCreateSetting(packageName);
            var pipeline = setting.GetOrCreatePipeline(buildPipeline);
            return pipeline;
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool GetUseAssetDependencyDB(string packageName, string buildPipeline)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            return setting.useAssetDependencyDB;
        }
        public YooAssetPackageBuildSettings SetUseAssetDependencyDB(string packageName, string buildPipeline, bool useAssetDependencyDB)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            setting.useAssetDependencyDB = useAssetDependencyDB;
            return this;
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool GetClearBuildCache(string packageName, string buildPipeline)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            return setting.clearBuildCache;
        }
        public YooAssetPackageBuildSettings SetClearBuildCache(string packageName, string buildPipeline, bool clearBuildCache)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            setting.clearBuildCache = clearBuildCache;
            return this;
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public string GetPackageEncryptionClassName(string packageName, string buildPipeline)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            return setting.encryptionClassName;
        }
        public YooAssetPackageBuildSettings SetPackageEncryptionClassName(string packageName, string buildPipeline, string packageEncryptionClassName)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            setting.encryptionClassName = packageEncryptionClassName;
            return this;
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public string GetPackageBuildingFileCopyParams(string packageName, string buildPipeline)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            return setting.buildingFileCopyParams;
        }
        public YooAssetPackageBuildSettings SetPackageBuildingFileCopyParams(string packageName, string buildPipeline, string buildingFileCopyParams)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            setting.buildingFileCopyParams = buildingFileCopyParams;
            return this;
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public EBuildingFileCopyOption GetPackageBuildingFileCopyOption(string packageName, string buildPipeline)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            return setting.buildingFileCopyOption;
        }
        public YooAssetPackageBuildSettings SetPackageBuildingFileCopyOption(string packageName, string buildPipeline, EBuildingFileCopyOption buildingFileCopyOption)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            setting.buildingFileCopyOption = buildingFileCopyOption;
            return this;
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public EFileNameStyle GetPackageFileNameStyle(string packageName, string buildPipeline)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            return setting.fileNameStyle;
        }
        public YooAssetPackageBuildSettings SetPackageFileNameStyle(string packageName, string buildPipeline, EFileNameStyle fileNameStyle)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            setting.fileNameStyle = fileNameStyle;
            return this;
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public ECompressOption GetPackageCompressOption(string packageName, string buildPipeline)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            return setting.compressOption;
        }
        public YooAssetPackageBuildSettings SetPackageCompressOption(string packageName, string buildPipeline, ECompressOption compressOption)
        {
            var setting = GetOrCreateSetting(packageName, buildPipeline);
            setting.compressOption = compressOption;
            return this;
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public string GetPackagePipeline(string packageName)
        {
            var setting = GetOrCreateSetting(packageName);
            return setting.pipeline;
        }

        public YooAssetPackageBuildSettings SetPackagePipeline(string packageName, string pipeline)
        {
            GetOrCreateSetting(packageName).pipeline = pipeline;
            return this;
        }
    }
    public static class AssetBundleBuilderSettingHelper
    {

        private static YooAssetPackageBuildSettings GetSetting()
        {
            var setting = YooAssetPackageBuildSettings.LoadOrCreate();
            return setting;
        }
        
        //////////////////////////////////////////////////////////////////////////////////////////////////
        // BuildPipelineName
        public static string GetPackageBuildPipeline(string packageName) => GetSetting().GetPackagePipeline(packageName);
        public static void SetPackageBuildPipeline(string packageName, string buildPipeline) => GetSetting().SetPackagePipeline(packageName, buildPipeline);
        
        //////////////////////////////////////////////////////////////////////////////////////////////////
        // ECompressOption
        public static ECompressOption GetPackageCompressOption(string packageName, string buildPipeline) => GetSetting().GetPackageCompressOption(packageName, buildPipeline);
        public static void SetPackageCompressOption(string packageName, string buildPipeline, ECompressOption compressOption) => GetSetting().SetPackageCompressOption(packageName, buildPipeline, compressOption).Save();

        
        //////////////////////////////////////////////////////////////////////////////////////////////////
        // EFileNameStyle
        public static EFileNameStyle GetPackageFileNameStyle(string packageName, string buildPipeline) => GetSetting().GetPackageFileNameStyle(packageName, buildPipeline);
        public static void SetPackageFileNameStyle(string packageName, string buildPipeline, EFileNameStyle fileNameStyle) => GetSetting().SetPackageFileNameStyle(packageName, buildPipeline, fileNameStyle).Save();
        

        //////////////////////////////////////////////////////////////////////////////////////////////////
        /// EBuildingFileCopyOption
        public static EBuildingFileCopyOption GetPackageBuildingFileCopyOption(string packageName, string buildPipeline) => GetSetting().GetPackageBuildingFileCopyOption(packageName, buildPipeline);
        public static void SetPackageBuildingFileCopyOption(string packageName, string buildPipeline, EBuildingFileCopyOption buildingFileCopyOption) => GetSetting().SetPackageBuildingFileCopyOption(packageName, buildPipeline, buildingFileCopyOption).Save();
        

        //////////////////////////////////////////////////////////////////////////////////////////////////
        // BuildFileCopyParams
        public static string GetPackageBuildingFileCopyParams(string packageName, string buildPipeline) => GetSetting().GetPackageBuildingFileCopyParams(packageName, buildPipeline);
        public static void SetPackageBuildingFileCopyParams(string packageName, string buildPipeline, string buildingFileCopyParams) => GetSetting().SetPackageBuildingFileCopyParams(packageName, buildPipeline, buildingFileCopyParams).Save();
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////
        // EncryptionClassName
        public static string GetPackageEncryptionClassName(string packageName, string buildPipeline) => GetSetting().GetPackageEncryptionClassName(packageName, buildPipeline);
        public static void SetPackageEncryptionClassName(string packageName, string buildPipeline, string encryptionClassName) => GetSetting().SetPackageEncryptionClassName(packageName, buildPipeline, encryptionClassName).Save();

        //////////////////////////////////////////////////////////////////////////////////////////////////
        // ClearBuildCache
        public static bool GetPackageClearBuildCache(string packageName, string buildPipeline) => GetSetting().GetClearBuildCache(packageName, buildPipeline);
        public static void SetPackageClearBuildCache(string packageName, string buildPipeline, bool clearBuildCache) => GetSetting().SetClearBuildCache(packageName, buildPipeline, clearBuildCache).Save();

        //////////////////////////////////////////////////////////////////////////////////////////////////
        // UseAssetDependencyDB
        public static bool GetPackageUseAssetDependencyDB(string packageName, string buildPipeline) => GetSetting().GetUseAssetDependencyDB(packageName, buildPipeline);
        public static void SetPackageUseAssetDependencyDB(string packageName, string buildPipeline, bool useAssetDependencyDB) => GetSetting().SetUseAssetDependencyDB(packageName, buildPipeline, useAssetDependencyDB).Save();
    }

}