using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

namespace YooAsset.Editor
{
    public class DefaultPackRule
    {
        /// <summary>
        /// AssetBundle文件的后缀名
        /// </summary>
        public const string AssetBundleFileExtension = "bundle";

        /// <summary>
        /// 原生文件的后缀名
        /// </summary>
        public const string RawFileExtension = "rawfile";

        /// <summary>
        /// Unity着色器资源包名称
        /// </summary>
        public const string ShadersBundleName = "unityshaders";


        public static PackRuleResult CreateShadersPackRuleResult()
        {
            PackRuleResult result = new PackRuleResult(ShadersBundleName, AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    /// 以文件路径作为资源包名
    /// 注意：每个文件独自打资源包
    /// 例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop_image_backgroud.bundle"
    /// 例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop_view_main.bundle"
    /// </summary>
    [DisplayName("资源包名: 文件路径")]
    public class PackSeparately : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            string bundleName = PathUtility.RemoveExtension(data.AssetPath);
            PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    /// 以父类文件夹路径作为资源包名
    /// 注意：文件夹下所有文件打进一个资源包
    /// 例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop_image.bundle"
    /// 例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop_view.bundle"
    /// </summary>
    [DisplayName("资源包名: 父类文件夹路径")]
    public class PackDirectory : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            string bundleName = Path.GetDirectoryName(data.AssetPath);
            PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    /// 以收集器路径下顶级文件夹为资源包名
    /// 注意：文件夹下所有文件打进一个资源包
    /// 例如：收集器路径为 "Assets/UIPanel"
    /// 例如："Assets/UIPanel/Shop/Image/backgroud.png" --> "assets_uipanel_shop.bundle"
    /// 例如："Assets/UIPanel/Shop/View/main.prefab" --> "assets_uipanel_shop.bundle"
    /// </summary>
    [DisplayName("资源包名: 收集器下顶级文件夹路径")]
    public class PackTopDirectory : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            string assetPath = data.AssetPath.Replace(data.CollectPath, string.Empty);
            assetPath = assetPath.TrimStart('/');
            string[] splits = assetPath.Split('/');
            if (splits.Length > 0)
            {
                if (Path.HasExtension(splits[0]))
                    throw new Exception($"Not found root directory : {assetPath}");
                string bundleName = $"{data.CollectPath}/{splits[0]}";
                PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
                return result;
            }
            else
            {
                throw new Exception($"Not found root directory : {assetPath}");
            }
        }
    }

    /// <summary>
    /// 以收集器路径作为资源包名
    /// 注意：收集的所有文件打进一个资源包
    /// </summary>
    [DisplayName("资源包名: 收集器路径")]
    public class PackCollector : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            string bundleName;
            string collectPath = data.CollectPath;
            if (AssetDatabase.IsValidFolder(collectPath))
            {
                bundleName = collectPath;
            }
            else
            {
                bundleName = PathUtility.RemoveExtension(collectPath);
            }

            PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    /// 以分组名称作为资源包名
    /// 注意：收集的所有文件打进一个资源包
    /// </summary>
    [DisplayName("资源包名: 分组名称")]
    public class PackGroup : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            string bundleName = data.GroupName;
            PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            return result;
        }
    }

    /// <summary>
    /// 打包原生文件
    /// </summary>
    [DisplayName("打包原生文件")]
    public class PackRawFile : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            string bundleName = data.AssetPath;
            PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.RawFileExtension);
            return result;
        }
    }

    /// <summary>
    /// 打包视频文件
    /// </summary>
    [DisplayName("打包视频文件")]
    public class PackVideoFile : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            string bundleName = data.AssetPath;
            string fileExtension = Path.GetExtension(data.AssetPath);
            fileExtension = fileExtension.Remove(0, 1);
            PackRuleResult result = new PackRuleResult(bundleName, fileExtension);
            return result;
        }
    }

    /// <summary>
    /// 打包着色器
    /// </summary>
    [DisplayName("打包着色器文件")]
    public class PackShader : IPackRule
    {
        public PackRuleResult GetPackRuleResult(PackRuleData data)
        {
            return DefaultPackRule.CreateShadersPackRuleResult();
        }
    }

    /// <summary>
    /// 打包着色器变种集合
    /// </summary>
    [DisplayName("打包着色器变种集合文件")]
    public class PackShaderVariants : IPackRule
    {
        public PackRuleResult GetPackRuleResult(PackRuleData data)
        {
            return DefaultPackRule.CreateShadersPackRuleResult();
        }
    }

    public class PackWithData { }

    /// <summary>
    /// 根据正则匹配出来的规则进行打包
    /// </summary>
    [DisplayName("收集符合相同正则的打包")]
    public class PackRegular : PackWithData,IPackRule
    {
        public PackRuleResult GetPackRuleResult(PackRuleData data)
        {
            var match = Regex.Match(data.AssetPath, data.Regular);
            PackRuleResult result;
            if (match.Success)
            {
                string groupKey = match.Value;
                result = new PackRuleResult(groupKey, DefaultPackRule.AssetBundleFileExtension);
                return result;
            }
            string bundleName = Path.GetDirectoryName(data.AssetPath);
            result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
            return result;
        }
    }
    
    /// <summary>
    /// 根据正则匹配出来的规则进行打包
    /// </summary>
    [DisplayName("根绝文件夹结构命名并剔除中文,Regular表示取前几个/的内容")]
    public class PackDirectoryWithoutChinese : PackWithData, IPackRule
    {
        public PackRuleResult GetPackRuleResult(PackRuleData data)
        {
            string bundleName = Path.GetDirectoryName(data.AssetPath);
            string limitedPath = bundleName;
            
            string cleanStr = Regex.Replace(data.Regular ?? "", @"[^\d-]", "");
            bool success = int.TryParse(cleanStr, out int limit);
            if (!string.IsNullOrEmpty(data.Regular) && success)
            {
                string[] parts = bundleName.Split('/', '\\');
                limitedPath = string.Join("/", parts.Take(Math.Min(limit, parts.Length)));
            }
            
            // 移除中文字符
            string pattern = @"[\u4e00-\u9fff]";
            var withoutChinese = Regex.Replace(limitedPath, pattern, "");
    
            // 清理多余的斜杠
            var clear = Regex.Replace(withoutChinese, @"[/\\]+", "/");
    
            PackRuleResult result = new PackRuleResult(clear, DefaultPackRule.AssetBundleFileExtension);
            return result;
        }
    }
}