using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

public class LoadUIView : MonoBehaviour
{
    public YooAssetReference Reference;
    public Transform Root;

    public static List<GameObject> _objs;
    public static AssetHandle handle;
    

    public async void LoadObj()
    {        
        if(_objs == null)
            _objs = new ();
        if (handle == null)
        {
            handle = YooAssets.LoadAssetAsync<GameObject>(Reference.GUID);
            await handle;
        }
        var obj = Instantiate(handle.AssetObject,Root) as GameObject;
        _objs.Add(obj);
    }

    public void DestroyObj()
    {
        var count = _objs.Count;
        if(count == 0)
            return;
        if (count != 0)
        {
            var obj = _objs[count - 1];
            Destroy(obj);
            _objs.RemoveAt(count - 1);
        }

        if (count == 1)
        {
            handle.Release();
            handle = null;
            var package = YooAssets.GetPackage(YooAssetDefine.DEFAULT_PACKAGE);
            package.UnloadUnusedAssetsAsync();
        }
    }
}
