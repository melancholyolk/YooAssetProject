using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class LoadCube : MonoBehaviour
{
    public List<YooAssetReference> References;
    public Transform Root;
    public Text MemortLimit;
    
    public YooAssetReference EffectPrefab;
    private AssetHandle _effectHandle;
    private GameObject _effectObj;
    
    private List<AssetHandle> _handles = new List<AssetHandle>();
    private List<GameObject> _objs = new List<GameObject>();
    private int _headIndex;
    private Vector2 _offset = new Vector2(4, 3);
    private ResourcePackage package;

    private void Start()
    {
        Texture.streamingTextureDiscardUnusedMips = true;
        QualitySettings.streamingMipmapsMemoryBudget = 1;
        MemortLimit.text = QualitySettings.streamingMipmapsMemoryBudget.ToString();
    }

    public async void AddCube()
    {
        if(_headIndex >= References.Count)
            return;
        var handle = YooAssets.LoadAssetAsync<GameObject>(References[_headIndex].GUID);
        await handle;
        _handles.Add(handle);
        InstantiateSync(handle);
        _headIndex++;
    }
    
    public void RemoveCube()
    {
        if(_headIndex == 0)
            return;
        if (package == null)
            package = YooAssets.GetPackage(YooAssetDefine.DEFAULT_PACKAGE);
        var index = --_headIndex;
        Destroy(_objs[index]);
        _objs.RemoveAt(index);
        _handles[index].Release();
        _handles.RemoveAt(index);
        package.UnloadUnusedAssetsAsync();
    }

    private void InstantiateSync(AssetHandle handle)
    {
        var row = _headIndex / 5;
        var col = _headIndex % 5;
        var obj = handle.InstantiateSync();
        obj.transform.position = this.transform.position + new Vector3(col * _offset.y, -row * _offset.x, 20);
        _objs.Add(obj);
    }
    
    public void AddMip(float mip)
    {
        QualitySettings.streamingMipmapsMemoryBudget += mip;
        MemortLimit.text = QualitySettings.streamingMipmapsMemoryBudget.ToString();
    }

    public void RemoveMip(float mip)
    {
        QualitySettings.streamingMipmapsMemoryBudget -= mip;
        MemortLimit.text = QualitySettings.streamingMipmapsMemoryBudget.ToString();
    }
    
    public async void LoadEffect()
    {
        if(_effectHandle != null)
            return;
        var handle = YooAssets.LoadAssetAsync<GameObject>(EffectPrefab.GUID);
        await handle;
        _effectHandle = handle;
        _effectObj = handle.InstantiateSync();
    }

    public void RemoveEffect()
    {
        if(_effectHandle == null)
            return;
        if (package == null)
            package = YooAssets.GetPackage(YooAssetDefine.DEFAULT_PACKAGE);
       Destroy(_effectObj);
       _effectHandle.Release();
       package.UnloadUnusedAssetsAsync();
       _effectHandle = null;
       Resources.UnloadUnusedAssets();
    }
}
