using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

public class ExtractorAnimationClip : AssetExtractorBase<AnimationClip, AnimationClip>
{
    protected override string assetType => "t:AnimationClip";
    
    protected override bool CheckAssetNeedIgnore(AnimationClip asset)
    {
        return asset.name.Contains("__preview__");
    }

    protected override AnimationClip MakeNewAsset()
    {
        return new AnimationClip();
    }

    protected override void SetupDestAsset(AnimationClip destAsset, string assetName)
    {
        destAsset.name = Path.GetFileNameWithoutExtension(assetName);
    }

    protected override void CopyAsset(AnimationClip sourceAsset, AnimationClip destAsset)
    {
        EditorUtility.CopySerialized(sourceAsset, destAsset);
    }

    protected override string MakeAssetName(in string nameTemplate, in string collectorName, in string filePathPick, AnimationClip sourceAsset)
    {
        var assetName = nameTemplate
            .Replace(AssetNameMark, sourceAsset.name)
            .Replace(CollectorNameMark, collectorName)
            .Replace(FilePathMark, filePathPick);
        return assetName;
    }
    
    protected override string PickFilePath(in string sourcePath, in string filePathPickRuleRegex)
    {
        
        var filePathPickRuleMatch = Regex.Match(sourcePath, filePathPickRuleRegex);
        if (!filePathPickRuleMatch.Success)
        {
            return "";
        }
        
        var groups = filePathPickRuleMatch.Groups;
        var pickStr = groups[1].Value;
        if (groups[2].Success)
        {
            pickStr += $"@{groups[2].Value}";
        }

        return pickStr;
    }
}