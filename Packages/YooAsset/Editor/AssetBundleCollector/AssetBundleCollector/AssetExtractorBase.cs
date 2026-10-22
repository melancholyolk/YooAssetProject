using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    internal interface IAssetExtractor
    {
        public void ExtractAsset(AssetBundleCollector collector,  string outputFolderPath, string assetNameTemplate, string filePathPickRuleRegex) { }
    }

    public abstract class AssetExtractorBase<T, T1> : IAssetExtractor where T : Object where T1 : Object
    {
        protected readonly string CollectorNameMark = "{CollectorName}";
        protected readonly string AssetNameMark = "{AssetName}";
        protected readonly string FilePathMark = "{FilePath}";

        /// <summary>
        /// 资源类型，使用这个格式： "t:{Type of Asset Class}"
        /// </summary>
        protected abstract string assetType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collector"></param>
        /// <param name="outputFolderPath"></param>
        /// <param name="assetNameTemplate">
        /// <br/><b>Need Support Mark Below:</b><br/>
        /// {CollectorName} - Collector Name<br/>
        /// {AssetName} - Asset Name<br/>
        /// {FilePath} - File Path Pick Rule<br/>
        /// </param>
        /// <param name="filePathPickRuleRegex"></param>
        public void ExtractAsset(AssetBundleCollector collector, string outputFolderPath, string assetNameTemplate, string filePathPickRuleRegex)
        {
            var duplicateAnimationDict = new Dictionary<string, List<string>>();
        
            var assetModifyTime = new Dictionary<string, long>();
            var fileNameExt = Path.GetExtension(assetNameTemplate);
            assetNameTemplate = Path.GetFileNameWithoutExtension(assetNameTemplate);
        
            var pathFirstMap = new Dictionary<string, string>();
            var pathCountMap = new Dictionary<string, int>();
            outputFolderPath = Path.Combine(collector.CollectPath, outputFolderPath).Replace("\\", "/");
            
            var collectorName = Path.GetFileName(collector.CollectPath);
            var guids = AssetDatabase.FindAssets( assetType, new [] { collector.CollectPath });
            foreach (var guid in guids)
            {
                var sourcePath = AssetDatabase.GUIDToAssetPath(guid);

                if (sourcePath.StartsWith(outputFolderPath))
                {
                    continue;
                }
            
                var filePathPick = PickFilePath(sourcePath, filePathPickRuleRegex);
                
                var latestSourceFileModifyDateTime = File.GetLastWriteTime(sourcePath).Ticks;
                
                var assets = AssetDatabase.LoadAllAssetsAtPath(sourcePath);
                for (var i = 0; i < assets.Length; i++)
                {
                    if (assets[i] is not T sourceAsset || CheckAssetNeedIgnore(sourceAsset))
                    {
                        continue;
                    }
                    
                    var assetName = MakeAssetName(assetNameTemplate, collectorName, filePathPick, sourceAsset);
                    
                    if (pathCountMap.TryGetValue(assetName, out var count))
                    {
                        count += 1;
                        pathCountMap[assetName] = count;

                        if (!duplicateAnimationDict.TryGetValue(assetName, out var list))
                        {
                            list = new ();
                            list.Add ( pathFirstMap[assetName] );
                        
                            duplicateAnimationDict[assetName] = list;
                        }
                        list.Add(sourcePath);
                    }
                    else
                    {
                        pathFirstMap[assetName] = sourcePath;
                        count = pathCountMap[assetName] = 0;
                    }
                
                    if (count > 0)
                    {
                        assetName += $" _${count}$_";
                    }
                
                    KeepDestFolderAvailable(outputFolderPath);
                    
                    var savePath = Path.Combine(outputFolderPath, assetName + fileNameExt);

                    var destAsset = AssetDatabase.LoadAssetAtPath<T1>(savePath);
                    
                    if (CheckDestFileIsLatest(savePath, latestSourceFileModifyDateTime, out var lastTime))
                    {
                        continue;
                    }
                
                    var assetExist = destAsset != null;
                    destAsset ??= MakeNewAsset();
                
                    CopyAsset(sourceAsset, destAsset);
                
                    SetupDestAsset(destAsset, assetName);
                    
                    if (assetExist)
                    {
                        EditorUtility.SetDirty(destAsset);
                        AssetDatabase.SaveAssetIfDirty(destAsset);
                    }
                    else
                    {
                        AssetDatabase.CreateAsset(destAsset, savePath);
                    }
                
                    assetModifyTime[savePath] = latestSourceFileModifyDateTime;

                    PostprocessDestAsset(destAsset, savePath);

                }
            }
            
            AssetDatabase.Refresh();
            
            foreach (var kv in assetModifyTime)
            {
                UpdateDestFileModifyDate(kv.Key, kv.Value);
            }
        
            if (duplicateAnimationDict.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var kv in duplicateAnimationDict)
                {
                    foreach (var path in kv.Value)
                    {
                        sb.AppendLine($"AnimationClip('{kv.Key}') Found In: " + path);
                    }
                }


                
                var color = new Color(0.91f, 0.75f, 0.17f);
                Debug.LogWarningFormat($"<b><color=#{{0:X2}}{{1:X2}}{{2:X2}}>!!!!!!!!!!!!!!! [{GetType().Name}] [Duplicate Asset Finding] !!!!!!!!!!!!!!!!!!!!!!!!!</color></b>\n{sb}", (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f));
            }
        }

        private void KeepDestFolderAvailable(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

        }

        protected virtual bool CheckAssetNeedIgnore(T asset)
        {
            return false;
        }

        protected abstract T1 MakeNewAsset();
        
        protected abstract void CopyAsset(T sourceAsset, T1 destAsset);
        
        protected abstract void SetupDestAsset(T1 destAsset, string assetName);

        protected virtual void PostprocessDestAsset(T1 destAsset, string savePath) { }

        protected abstract string PickFilePath(in string sourcePath, in string filePathPickRuleRegex);
        
        protected abstract string MakeAssetName(in string nameTemplate, in string collectorName, in string filePathPick, T sourceAsset);
        
        const string ModifyDataMark = "modifyDate:";
        
        static bool CheckDestFileIsLatest(string assetPath, long latestTime, out long lastTime)
        {
            var metaFilePath = assetPath + ".meta";
            // 检查文件是否存在
            if (!File.Exists(metaFilePath))
            {
                
                lastTime = -1;
            
                return false;
            }

            var allLines = File.ReadAllLines(metaFilePath);
            
            // 逐行读取 meta 文件
            foreach (var line in allLines)
            {
                // 查找包含 modifyDate 的行
                if (!line.Trim().StartsWith(ModifyDataMark))
                {
                    continue;
                }
                
                // 提取 modifyDate 的值
                var modifyDateStr = line.Split(':')[1].Trim();
                // 转换为 long 类型
                if (long.TryParse(modifyDateStr, out var modifyTime))
                {
                    lastTime = modifyTime;
                    
                    return modifyTime == latestTime;
                }
                break;
            }
            
            lastTime = -1;
            
            return false;
        }

        static void UpdateDestFileModifyDate(string assetPath, long latestModifyTime)
        {
            var metaFilePath = assetPath + ".meta";
            
            if (!File.Exists(metaFilePath)) return;
            
            var metaFileLines = File.ReadAllLines(metaFilePath);

            using (var writer = new StreamWriter(metaFilePath))
            {
                var customDataWritten = false;
                foreach (var line in metaFileLines)
                {
                    // 查找包含 modifyDate 的行
                    if (line.Trim().StartsWith(ModifyDataMark))
                    {
                        writer.WriteLine($"  {ModifyDataMark} {latestModifyTime}");
                        customDataWritten = true;
                    }
                    else
                    {
                        writer.WriteLine(line);
                    }
                }
                
                if(!customDataWritten)
                {
                    writer.WriteLine("customData:");
                    writer.WriteLine($"  {ModifyDataMark} {latestModifyTime}");
                }
            }
        }
    }
}