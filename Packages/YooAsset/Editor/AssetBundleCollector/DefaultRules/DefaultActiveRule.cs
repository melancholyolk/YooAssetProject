
namespace YooAsset.Editor
{
    [DisplayName("启用分组")]
    public class EnableGroup : IActiveRule
    {
        public bool IsActiveGroup(GroupData data)
        {
            return true;
        }
    }

    [DisplayName("禁用分组")]
    public class DisableGroup : IActiveRule
    {
        public bool IsActiveGroup(GroupData data)
        {
            return false;
        }
    }
    
    [DisplayName("仅在Editor下可以使用的分组")]
    public class EditoGroup : IActiveRule
    {
        public bool IsActiveGroup(GroupData data)
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }
    }
}