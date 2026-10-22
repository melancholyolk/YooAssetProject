using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using YooAsset;

namespace ACT.UI
{
    public class SpriteAtlasLoader
    {
        
        private Dictionary<string, SpriteAtlas> _loadedAtlas = new Dictionary<string, SpriteAtlas>(1000);
        private List<AssetHandle> _loadHandles = new List<AssetHandle>(128);

        public void Init()
        {
            SpriteAtlasManager.atlasRequested += RequestAtlas;
        }
        

        private async void RequestAtlas(string atlasName, Action<SpriteAtlas> callback)
        {
            if (_loadedAtlas.TryGetValue(atlasName, out var value))
            {
                callback.Invoke(value);
            }
            else
            {
                var loadHandle = YooAssets.LoadAssetAsync<SpriteAtlas>(atlasName);
                await loadHandle;
                if (loadHandle.Status != EOperationStatus.Succeed)
                {
                    UnityEngine.Debug.LogWarning($"Failed to load sprite atlas : {atlasName} !");
                    return;
                }

                var atlas = loadHandle.AssetObject as SpriteAtlas;
                _loadedAtlas.Add(atlasName, atlas);
                _loadHandles.Add(loadHandle);
                callback.Invoke(atlas);
                Debug.Log($"加载图集{atlasName}");
            }
        }
    }
}