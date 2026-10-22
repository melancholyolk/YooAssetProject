#if UNITY_2019_4_OR_NEWER
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    public abstract class BuildPipelineViewerBase
    {
        protected const int StyleWidth = 400;
        protected const int LabelMinWidth = 190;

        protected string PackageName { private set; get; }
        protected string PipelineName { private set; get; }
        protected BuildTarget BuildTarget { private set; get; }

        /// <summary>
        /// 初始化视图
        /// </summary>
        public void InitView(string packageName, string pipelineName, BuildTarget buildTarget)
        {
            PackageName = packageName;
            PipelineName = pipelineName;
            BuildTarget = buildTarget;
        }

        /// <summary>
        /// 创建视图
        /// </summary>
        public abstract void CreateView(VisualElement parent);

        /// <summary>
        /// 获取默认版本
        /// </summary>
        protected virtual string GetDefaultPackageVersion()
        {
            int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
        }

        /// <summary>
        /// 创建加密类实例
        /// </summary>
        protected IEncryptionServices CreateEncryptionInstance()
        {
            var encyptionClassName = AssetBundleBuilderSettingHelper.GetPackageEncryptionClassName(PackageName, PipelineName);
            var encryptionClassTypes = EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
            var classType = encryptionClassTypes.Find(x => x.FullName.Equals(encyptionClassName));
            if (classType != null)
                return (IEncryptionServices)Activator.CreateInstance(classType);
            else
                return null;
        }

        #region UI元素通用处理方法
        protected void SetBuildOutputField(TextField textField)
        {
            // 输出目录
            string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            textField.SetValueWithoutNotify(defaultOutputRoot);
            textField.SetEnabled(false);
            UIElementsTools.SetElementLabelMinWidth(textField, LabelMinWidth);
        }
        protected void SetBuildVersionField(TextField textField)
        {
            // 构建版本
            textField.style.width = StyleWidth;
            textField.SetValueWithoutNotify(GetDefaultPackageVersion());
            UIElementsTools.SetElementLabelMinWidth(textField, LabelMinWidth);
        }
        protected void SetCompressionField(EnumField enumField)
        {
            // 压缩方式选项
            var compressOption = AssetBundleBuilderSettingHelper.GetPackageCompressOption(PackageName, PipelineName);
            enumField.Init(compressOption);
            enumField.SetValueWithoutNotify(compressOption);
            enumField.style.width = StyleWidth;
            enumField.RegisterValueChangedCallback(evt =>
            {
                AssetBundleBuilderSettingHelper.SetPackageCompressOption(PackageName, PipelineName, (ECompressOption)enumField.value);
            });
            UIElementsTools.SetElementLabelMinWidth(enumField, LabelMinWidth);
        }
        protected void SetOutputNameStyleField(EnumField enumField)
        {
            // 输出文件名称样式
            var fileNameStyle = AssetBundleBuilderSettingHelper.GetPackageFileNameStyle(PackageName, PipelineName);
            enumField.Init(fileNameStyle);
            enumField.SetValueWithoutNotify(fileNameStyle);
            enumField.style.width = StyleWidth;
            enumField.RegisterValueChangedCallback(evt =>
            {
                AssetBundleBuilderSettingHelper.SetPackageFileNameStyle(PackageName, PipelineName, (EFileNameStyle)enumField.value);
            });
            UIElementsTools.SetElementLabelMinWidth(enumField, LabelMinWidth);
        }
        protected void SetCopyBuildinFileOptionField(EnumField enumField, TextField tagField)
        {
            // 首包文件拷贝选项
            var buildinFileCopyOption = AssetBundleBuilderSettingHelper.GetPackageBuildingFileCopyOption(PackageName, PipelineName);
            enumField.Init(buildinFileCopyOption);
            enumField.SetValueWithoutNotify(buildinFileCopyOption);
            enumField.style.width = StyleWidth;
            enumField.RegisterValueChangedCallback(evt =>
            {
                AssetBundleBuilderSettingHelper.SetPackageBuildingFileCopyOption(PackageName, PipelineName, (EBuildingFileCopyOption)enumField.value);

                // 设置内置资源标签显隐
                bool tagsFiledVisible = buildinFileCopyOption == EBuildingFileCopyOption.ClearAndCopyByTags || buildinFileCopyOption == EBuildingFileCopyOption.OnlyCopyByTags;
                tagField.visible = tagsFiledVisible;
            });
            UIElementsTools.SetElementLabelMinWidth(enumField, LabelMinWidth);
        }
        protected void SetCopyBuildinFileTagsField(TextField textField)
        {
            // 首包文件拷贝参数
            var buildinFileCopyParams = AssetBundleBuilderSettingHelper.GetPackageBuildingFileCopyParams(PackageName, PipelineName);
            textField.SetValueWithoutNotify(buildinFileCopyParams);
            textField.RegisterValueChangedCallback(evt =>
            {
                AssetBundleBuilderSettingHelper.SetPackageBuildingFileCopyParams(PackageName, PipelineName, textField.value);
            });
            UIElementsTools.SetElementLabelMinWidth(textField, LabelMinWidth);
        }
        protected void SetClearBuildCacheToggle(Toggle toggle)
        {
            // 清理构建缓存
            bool clearBuildCache = AssetBundleBuilderSettingHelper.GetPackageClearBuildCache(PackageName, PipelineName);
            toggle.SetValueWithoutNotify(clearBuildCache);
            toggle.RegisterValueChangedCallback(evt =>
            {
                AssetBundleBuilderSettingHelper.SetPackageClearBuildCache(PackageName, PipelineName, toggle.value);
            });
            UIElementsTools.SetElementLabelMinWidth(toggle, LabelMinWidth);
        }
        protected void SetUseAssetDependencyDBToggle(Toggle toggle)
        {
            // 使用资源依赖数据库
            bool useAssetDependencyDB = AssetBundleBuilderSettingHelper.GetPackageUseAssetDependencyDB(PackageName, PipelineName);
            toggle.SetValueWithoutNotify(useAssetDependencyDB);
            toggle.RegisterValueChangedCallback(evt =>
            {
                AssetBundleBuilderSettingHelper.SetPackageUseAssetDependencyDB(PackageName, PipelineName, toggle.value);
            });
            UIElementsTools.SetElementLabelMinWidth(toggle, LabelMinWidth);
        }
        protected PopupField<Type> CreateEncryptionField(VisualElement container)
        {
            // 加密方法
            var encryptionClassTypes = EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
            if (encryptionClassTypes.Count > 0)
            {
                var encyptionClassName = AssetBundleBuilderSettingHelper.GetPackageEncryptionClassName(PackageName, PipelineName);
                int defaultIndex = encryptionClassTypes.FindIndex(x => x.FullName.Equals(encyptionClassName));
                if (defaultIndex < 0)
                    defaultIndex = 0;
                var encryptionField = new PopupField<Type>(encryptionClassTypes, defaultIndex);
                encryptionField.label = "Encryption";
                encryptionField.style.width = StyleWidth;
                encryptionField.RegisterValueChangedCallback(evt =>
                {
                    AssetBundleBuilderSettingHelper.SetPackageEncryptionClassName(PackageName, PipelineName, encryptionField.value.FullName);
                });
                container.Add(encryptionField);
                UIElementsTools.SetElementLabelMinWidth(encryptionField, LabelMinWidth);
                return encryptionField;
            }
            else
            {
                var encryptionField = new PopupField<Type>();
                encryptionField.label = "Encryption";
                encryptionField.style.width = StyleWidth;
                container.Add(encryptionField);
                UIElementsTools.SetElementLabelMinWidth(encryptionField, LabelMinWidth);
                return encryptionField;
            }
        }
        #endregion
    }
}
#endif