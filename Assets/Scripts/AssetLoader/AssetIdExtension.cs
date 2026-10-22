using YooAsset;


namespace ACT
{
    public static class AssetIdExtension
    {
        private static ResourcePackage _package;
        
        public static bool RuntimeKeyIsValid(this YooAssetReference assetId)
        {
            var guid = assetId.GUID;
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }

            if (_package == null)
            {
                _package = YooAssets.GetPackage(AssetDefine.DEFAULT_PACKAGE);
            }
            
            return _package.CheckLocationValid(guid);
        }
        
        
#if UNITY_EDITOR
        public static bool EditorKeyIsValid(this YooAssetReference assetId)
        {
            return YooAsset.Editor.AssetAddressHelper.TryGetAssetInfoByGuid(assetId.GUID, out _);
        }
#endif
    }
}