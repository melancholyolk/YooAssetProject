using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace YooAsset.Editor
{
    [Serializable]
    public class AssetBundleCollectorRule
    {
        public string path;
        [FormerlySerializedAs("collectors")] public List<AutoCollectBundleSetting> collectorRules = new ();

        [Serializable]
        public class AutoCollectBundleSetting
        {
            public string collectorRegular;

            public string type;
                
            public bool enableExtractor;
            
            public ECollectorType collectType;
            
            public string addressRuleName;
            
            public string addressRegular;
                
            public string packRuleName;
                
            public string filterRuleName;
                        
            public string packRegular;
            
            public string collectRegular;
                
            public string assetTags;

            public List<ExtractorSetting> extractorSettings = new ();
            
            public AutoCollectBundleSetting(ECollectorType collectType ,string type,string addressRuleName,string addressRegular ,string packRuleName, string filterRuleName, string collectorRegular,string packRegular,string collectRegular, string assetTags)
            {
                this.collectType = collectType;
                this.type = type;
                this.addressRuleName = addressRuleName;
                this.addressRegular = addressRegular;
                this.packRuleName = packRuleName;
                this.filterRuleName = filterRuleName;
                this.collectorRegular = collectorRegular;
                this.packRegular = packRegular;
                this.collectRegular = collectRegular;
                this.assetTags = assetTags;
            }
                
            public AutoCollectBundleSetting() : this(ECollectorType.MainAssetCollector,nameof(AddressByPath) ,"","",nameof(PackDirectory), nameof(CollectAll), "","","", "")
            {
            }

        }
        
        
        [Serializable]
        public class ExtractorSetting
        {
            public string extractorName;
            public string extractorFolderPath;
            public string assetNameTemplate;
            public string filePathPickRule;
        }

    }
}