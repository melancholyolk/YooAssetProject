using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset;

namespace YooAsset
{
    public class YooAssetDefine
    {
        public const string DEFAULT_PACKAGE = "DefaultPackage";
        public const string DLL_PACKAGE = "Dll";
        public const string CLIENT_STATIC_PACKAGE = "ClientStatic";
        
        public static List<string> PACKAGES = new List<string>()
        {
            DEFAULT_PACKAGE,
            DLL_PACKAGE,
            CLIENT_STATIC_PACKAGE
        }; 
    }
}