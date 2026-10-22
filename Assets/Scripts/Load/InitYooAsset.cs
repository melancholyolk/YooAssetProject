using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

public class InitYooAsset : MonoBehaviour
{
    public static bool IsInitOver = false;
    
    public YooAssetReference SceneName;
    public EPlayMode EPlayMode = EPlayMode.EditorSimulateMode;
    public string Version = "0";

    async void Start()
    {
        YooAssets.Initialize();
        string packageName = YooAssetDefine.DEFAULT_PACKAGE;
        var package = YooAssets.TryGetPackage(packageName);
        if (package == null)
        {
            package = YooAssets.CreatePackage(packageName);
            await InitPackage(package);
            YooAssets.SetDefaultPackage(package);
        }

        Texture.streamingTextureDiscardUnusedMips = true;
        // var loader = new SpriteAtlasLoader();
        // loader.Init();
    }

    async UniTask InitPackage(ResourcePackage package)
    {
#if UNITY_EDITOR
        InitializeParameters initParameters = null;
        if (EPlayMode == EPlayMode.EditorSimulateMode)
        {
            var initParam = new EditorSimulateModeParameters();
            var buildResult = EditorSimulateModeHelper.SimulateBuild(package.PackageName);
            initParam.EditorFileSystemParameters =
                FileSystemParameters.CreateDefaultEditorFileSystemParameters(buildResult.PackageRootDirectory);
            initParameters = initParam;
        }
        else if (EPlayMode == EPlayMode.HostPlayMode)
        {
            string defaultHostServer = GetHostServerURL(package.PackageName);
            string fallbackHostServer = GetHostServerURL(package.PackageName);
            IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            var initParam = new HostPlayModeParameters();
            initParam.CacheFileSystemParameters =
                FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
            initParameters = initParam;
        }

#else
        string defaultHostServer = GetHostServerURL(package.PackageName);
        string fallbackHostServer = GetHostServerURL(package.PackageName);
        IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
        HostPlayModeParameters initParameters = new HostPlayModeParameters();
        initParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
#endif
        var initOperation = package.InitializeAsync(initParameters);
        await initOperation;
        if (initOperation.Status == EOperationStatus.Succeed)
            Debug.Log("资源包初始化成功！");
        else
            Debug.LogError($"资源包初始化失败：{initOperation.Error}");

        var operation = package.RequestPackageVersionAsync();
        await operation;

        if (operation.Status != EOperationStatus.Succeed)
        {
            Debug.LogError(operation.Error);
            return;
        }

        string packageVersion = operation.PackageVersion;
        Debug.Log($"Updated package Version : {packageVersion}");

        var operation2 = package.UpdatePackageManifestAsync(packageVersion);
        await operation2;

        if (operation2.Status != EOperationStatus.Succeed)
        {
            Debug.LogError(operation2.Error);
        }
        
        var downloader = package.CreateResourceDownloader(10, 3);
        if (downloader.TotalDownloadCount == 0)
        {
            IsInitOver = true;
            return;
        }
        
        downloader.BeginDownload();
        
        downloader.DownloadErrorCallback = OnDownloadErrorFunction;
        downloader.DownloadUpdateCallback  = OnDownloadProgressUpdateFunction;
        downloader.DownloadFinishCallback  = OnDownloadOverFunction;
        downloader.DownloadFileBeginCallback = OnStartDownloadFileFunction;
    }
    

    public void LoadScene()
    {
        if(IsInitOver)
            YooAssets.LoadSceneAsync(SceneName.GUID, LoadSceneMode.Single);
    }

    private string GetHostServerURL(string packageName)
    {
        string hostServerIP = "https://qiantan.zhuziplay.com/act_cdn/yooasset";
        return $"{hostServerIP}/{Version}/{packageName}";
    }

    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }

        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }

        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }
    
    /// <summary>
    /// 开始下载
    /// </summary>
    private void OnStartDownloadFileFunction(DownloadFileData data)
    {
        Debug.Log(string.Format("开始下载：文件名：{0}，文件大小：{1}", data.FileName, data.FileSize));
    }
    /// <summary>
    /// 更新中
    /// </summary>
    private void OnDownloadProgressUpdateFunction(DownloadUpdateData data)
    {
    }
    /// <summary>
    /// 下载出错
    /// </summary>
    private void OnDownloadErrorFunction(DownloadErrorData data)
    {
        Debug.LogError(string.Format($"{data.ErrorInfo}"));
    }
    /// <summary>
    /// 下载完成
    /// </summary>
    private void OnDownloadOverFunction(DownloaderFinishData data)
    {
        Debug.Log("下载" + (data.Succeed ? "成功" : "失败"));
        UpdateOver();
    }

    private void UpdateOver()
    {
        IsInitOver = true;
    }
}