
namespace YooAsset.Editor
{
    public struct AddressRuleData
    {
        public string AssetPath;
        public string CollectPath;
        public string GroupName;
        public string Regular;

        public AddressRuleData(string assetPath, string collectPath, string groupName, string regular)
        {
            AssetPath = assetPath;
            CollectPath = collectPath;
            GroupName = groupName;
            Regular = regular;
        }
    }

    /// <summary>
    /// 寻址规则接口
    /// </summary>
    public interface IAddressRule
    {
        string GetAssetAddress(AddressRuleData data);
    }
}