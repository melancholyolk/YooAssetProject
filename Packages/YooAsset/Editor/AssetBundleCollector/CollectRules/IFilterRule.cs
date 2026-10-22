
namespace YooAsset.Editor
{
    public struct FilterRuleData
    {
        public string AssetPath;
        public string CollectPath;
        public string GroupName;
        public string Regular;

        public FilterRuleData(string assetPath, string collectPath, string groupName, string regular)
        {
            AssetPath = assetPath;
            CollectPath = collectPath;
            GroupName = groupName;
            Regular = regular;
        }
    }

    /// <summary>
    /// 资源过滤规则接口
    /// </summary>
    public interface IFilterRule
    {
        /// <summary>
        /// 是否为收集资源
        /// </summary>
        /// <returns>如果收集该资源返回TRUE</returns>
        bool IsCollectAsset(FilterRuleData data);
    }
}