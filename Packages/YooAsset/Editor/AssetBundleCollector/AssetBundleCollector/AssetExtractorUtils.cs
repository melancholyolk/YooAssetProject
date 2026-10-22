using System;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    public static class AssetExtractorUtils
    {
        public static Dictionary<string, Type> GetAssetExtractorMaps()
        {
            var extractorMap = new Dictionary<string, Type>();
            
            var checkInterface = typeof(IAssetExtractor);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface || type.IsAbstract || !checkInterface.IsAssignableFrom(type))
                    {
                        continue;
                    }
                    extractorMap.Add(type.Name, type);
                }
            }
            return extractorMap;
        }
    }
}