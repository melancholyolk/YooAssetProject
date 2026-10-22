using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset;

namespace ACT.Extractors
{
    
    public class YooAssetProfiles
    {
        public static string CDN;
        public static string Folder;
        public static string LOCAL_BUILD_PATH = $"ServerData/{Folder}";
        public static string LOCAL_LOAD_PATH = $"ServerData/{Folder}";
        public static string REMOTE_BUILD_PATH = $"ServerData/{Folder}";
        public static string REMOTE_LOAD_PATH = $"{CDN}/{Folder}";
        
        public static string GetDefaultBuildOutputRoot()
        {
            string projectPath = Path.GetDirectoryName(Application.dataPath);
            return $"{projectPath}/ServerData";
        }

        public static Dictionary<string, EBuildBundleType> PACKAGE_BUNLE_TYPE = new Dictionary<string, EBuildBundleType>()
        {
            {YooAssetDefine.DEFAULT_PACKAGE,EBuildBundleType.AssetBundle},
#if USE_HYBRIDCLR
            {YooAssetDefine.DLL_PACKAGE,EBuildBundleType.AssetBundle},
#endif
            {YooAssetDefine.CLIENT_STATIC_PACKAGE,EBuildBundleType.AssetBundle}
        };
    }
}