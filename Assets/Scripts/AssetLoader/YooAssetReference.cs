using System;
using UnityEngine;
using YooAsset;
#if UNITY_EDITOR
using YooAsset.Editor;
#endif


[Serializable]
public class YooAssetReference
{
    [SerializeField] string _guid;

    public string GUID
    {
        get => _guid;
    }

    private static ResourcePackage _package;

    public YooAssetReference()
    {
    }

    public YooAssetReference(string guid)
    {
        _guid = guid;
    }

    public static bool operator ==(YooAssetReference left, YooAssetReference right)
    {
        if (ReferenceEquals(left, null))
            return ReferenceEquals(right, null);

        return left.Equals(right);
    }

    public static bool operator !=(YooAssetReference left, YooAssetReference right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;

        return Equals((YooAssetReference)obj);
    }

    public bool Equals(YooAssetReference other)
    {
        if (other is null) return false;

        return _guid == other._guid;
    }

    public override int GetHashCode()
    {
        return _guid.GetHashCode();
    }
}

[Serializable]
public class YooAssetReference<T> : YooAssetReference where T : UnityEngine.Object
{
    public YooAssetReference(string guid) : base(guid)
    {
    }
}