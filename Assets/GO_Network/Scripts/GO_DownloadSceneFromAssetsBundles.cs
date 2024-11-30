using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static System.Net.WebRequestMethods;

public class GO_DownloadSceneFromAssetsBundles : MonoBehaviour
{

    [SerializeField] private string urlAssets;
    public bool loadingSceneComplete;
    private AssetBundle _bundle = null;
    private string _scenePath;

    public static GO_DownloadSceneFromAssetsBundles Instance;

    //public static event Action OnAssetsBundleStartLoading;
    public static event Action OnAssetsBundleLoadedScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;            
        }else
        {
            Destroy(gameObject);
        }
    }

    public void downloadScene(string _SceneName)
    {
        StartCoroutine(LoadSceneAssetBundle(_SceneName));
    }

    IEnumerator LoadSceneAssetBundle(string sceneName)//, Hash128 _hash)
    {
        loadingSceneComplete = false;
        string bundleUrl = urlAssets + sceneName.ToLower();
        if (_bundle != null)
        {
            _bundle.Unload(true);
        }

        yield return null;
        Resources.UnloadUnusedAssets();

        yield return null;
        GC.Collect();
        yield return null;

        if (bundleUrl != null)
        {
            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl))//, cachedBundle, 0))
            {
                Debug.Log("LOAD SCENE: " + bundleUrl);
                www.SendWebRequest();
                while (!www.isDone)
                {
                    Debug.Log("DOWNLOADING: " + www.downloadProgress * 100);
                    yield return new WaitForSeconds(.25f);                    

                }
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error loading Asset Bundle: " + www.error);
                    OnAssetsBundleLoadedScene?.Invoke();
                    loadingSceneComplete = true;
                    yield break;
                }
                else
                {
                    Debug.Log("\nSuccessful loading of the Asset Bundle: " + www.isDone);
                    _bundle = DownloadHandlerAssetBundle.GetContent(www);
                    if (string.IsNullOrEmpty(sceneName))
                    {
                        loadingSceneComplete = true;
                        yield break;
                    }

                    if (_bundle.isStreamedSceneAssetBundle)
                    {

                        string[] scenePaths = _bundle.GetAllScenePaths();
                        foreach (string path in scenePaths)
                            Debug.Log("PATH: " + path);
                        _scenePath = System.Array.Find(scenePaths, scene => scene.EndsWith(sceneName + ".unity"));
                        Debug.Log("SCENE NAME: " + sceneName);
                    }
                }
            }
        }
        else
        {
            _scenePath = sceneName;
        }

        if (!string.IsNullOrEmpty(_scenePath))
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(System.IO.Path.GetFileNameWithoutExtension(_scenePath));
            while (!asyncOperation.isDone)
            {
                float progreso = asyncOperation.progress;
                Debug.Log($"LOADING SCENE: {progreso * 100}%");
                yield return new WaitForSeconds(1);
            }          

            yield return new WaitForSeconds(.1f);

            if (!string.IsNullOrEmpty(_scenePath))
            {                
                OnAssetsBundleLoadedScene?.Invoke();   //SE CARGO LA SCENE BIEN             
            }
            yield return new WaitForSeconds(1);

            Debug.Log("UNLOAD " + _bundle);

            if (_bundle != null)
            {
                yield return null;
                _bundle.Unload(false);

                yield return null;
                Resources.UnloadUnusedAssets();

                yield return null;
            }

            System.GC.Collect();
        }
        loadingSceneComplete = true;
    }
}
