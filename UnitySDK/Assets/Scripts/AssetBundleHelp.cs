using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetBundleHelp: MonoBehaviour
{

    public string _strHost = @"http://127.0.0.1:8023";
    public string _strExt = ".ab";
    public string _strManifestExt=".manifest";
    private static AssetBundleHelp _instance = null;
    private AssetBundleManifest mManifest = null;

    //public bool isOk = false;
    //public List<GameObject> objList=new List<GameObject>();
    public Dictionary<string, List<GameObject>> objDic=new Dictionary<string, List<GameObject>>();
    public Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();

    //保存所有需要加载的文件
    private List<string> mDependList = new List<string>();
    private int mDependCount = 0;
    //private int mLoadCount=0;
    private string mLoadPath;
    private AssetBundleCreateRequest mRequest;
    private WWW mWWW;
    private bool mIsLocal;
    //加载时回调
    Action<string, float, bool> mLoadCallBack;
    // Use this for initialization
    void Start () {
        DontDestroyOnLoad(this);
	}
	
	// Update is called once per frame
	void Update () {
        if (mLoadCallBack != null)
        {
            if (mIsLocal)
            {
                mLoadCallBack(mLoadPath, (mDependCount - mDependList.Count - 1 + mRequest.progress) / mDependCount, false);
            }else
            {
                mLoadCallBack(mLoadPath, (mDependCount - mDependList.Count - 1 + mWWW.progress) / mDependCount, false);
            }
        }
	}

    public GameObject GetGameObject(string key, string name)
    {
        GameObject obj=null;
        if (objDic.ContainsKey(key+_strExt))
        {
            foreach(var o in objDic[key + _strExt])
            {
                if (o.name == name)
                {
                    obj = o;
                    break;
                }
            }
        }
        return obj;
    }

    public AssetBundleHelp() {
        _instance = this;
    }

    public static AssetBundleHelp Instance()
    {
        if (_instance == null) _instance = new AssetBundleHelp();
        return _instance;
    }

    private string GetManifestName()
    {
        string strPath;
        if (RuntimePlatform.Android == Application.platform)
        {
            strPath = @"/Android";
        }
        else if (RuntimePlatform.IPhonePlayer == Application.platform)
        {
            strPath = @"/iOS";
        }
        else
        {
            strPath = @"/StandaloneWindows";
        }
        return strPath;
    }

    //public Object LoadFromLocal(string name, string path)
    //{
    //    AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + GetManifestName()+ _strExt);
    //    AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    //    //assetBundle.Unload(false);

    //    string[] strDependencies = manifest.GetAllDependencies(path);
    //    foreach (string str in strDependencies)
    //    {
    //        AssetBundle.LoadFromFile(Application.streamingAssetsPath + @"/" + str);
    //    }

    //    assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + @"/" + path);
    //    Object obj = assetBundle.LoadAsset(name);
    //    //assetBundle.Unload(false);
    //    AssetBundle.UnloadAllAssetBundles(false);
    //    return obj;
    //}

    //public IEnumerator LoadFromLocalAsync(string name, string path)
    //{
    //    AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + GetManifestName() + _strExt);
    //    yield return request;
    //    AssetBundle assetBundle = request.assetBundle; ;
    //    AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    //    //assetBundle.Unload(false);

    //    string[] strDependencies = manifest.GetAllDependencies(path);
    //    foreach (string str in strDependencies)
    //    {
    //        AssetBundle.LoadFromFile(Application.streamingAssetsPath + @"/" + str);
    //    }

    //    assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + @"/" + path);
    //    GameObject obj = assetBundle.LoadAsset<GameObject>(name);
    //    //assetBundle.Unload(false);
    //    AssetBundle.UnloadAllAssetBundles(false);

    //    GameObject.Instantiate(obj);
    //}
    public void Clear()
    {
        mLoadCallBack = null;
        mDependCount = 0;
        mDependList.Clear();
        mLoadPath = null;
    }
    public IEnumerator LoadAssetBundleManifest(Action callback, bool isLocal = true)
    {
        if (isLocal)
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + GetManifestName() + _strExt);
            yield return request;
            AssetBundle assetBundle = request.assetBundle; ;
            mManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            assetBundle.Unload(false);
        }
        else
        {
            WWW www = new WWW(_strHost + GetManifestName() + GetManifestName() + _strExt);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log("ERROR:" + www.error);
                yield break;

            }

            AssetBundle assetBundle = www.assetBundle;
            SaveWWWAssetBundle(www, GetManifestName() + _strExt);

            mManifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            assetBundle.Unload(false);
        }
        //isOk = true;
        callback();

        //manifest测试
        //Debug.Log(mManifest.GetAssetBundleHash("game/test" + _strExt).ToString());
    }

    //public IEnumerator LoadManifestFromLocalAsync(Action callback, string path)
    //{
    //    AssetBundleCreateRequest request;

    //    request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + @"/" + path + _strExt+ _strManifestExt);
    //    yield return request;
    //    //AssetBundle.UnloadAllAssetBundles(false);
    //    AssetBundle assetBundle = request.assetBundle;
    //    //assetBundle.LoadAsset<AssetBundleManifest>();
    //    callback();
    //}

    public IEnumerator Init(Action callback)
    {
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + GetManifestName());
        yield return request;
        AssetBundle assetBundle = request.assetBundle; ;
        AssetBundleManifest manifestLocal = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        assetBundle.Unload(false);

        WWW www = new WWW(_strHost + GetManifestName() + GetManifestName() );
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("ERROR:" + www.error);
            yield break;

        }

        assetBundle = www.assetBundle;

        AssetBundleManifest manifestWWW = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        mManifest = manifestWWW;
        assetBundle.Unload(false);

        //对比本地和远程的manifest
        bool isUpdate=false;
        string[] strLocalAssetBundles = manifestLocal.GetAllAssetBundles();
        string[] strWWWAssetBundles = manifestWWW.GetAllAssetBundles();
        if(strLocalAssetBundles.Length!= strWWWAssetBundles.Length)
        {
            isUpdate = true;
        }

        //对比两边的hash
        foreach(string str in strLocalAssetBundles)
        {
            if (manifestLocal.GetAssetBundleHash(str) != manifestWWW.GetAssetBundleHash(str))
            {
                //如果本地文件存在，则删除
                if (File.Exists(Application.streamingAssetsPath + @"/"+str))
                {
                    File.Delete(Application.streamingAssetsPath + @"/" + str);
                }
                isUpdate = true;
            }
        }

        if (isUpdate)
        {
            SaveWWWAssetBundle(www, GetManifestName() + _strExt);
        }
        callback();
    }


    private void SaveWWWAssetBundle(WWW www, string path)
    {
        Debug.Log("SaveWWWAssetBundle:" + path);
        FileInfo fi = new FileInfo(Application.streamingAssetsPath + @"/" + path);
        var di = fi.Directory;
        if (!di.Exists)
            di.Create();

        FileStream fs = File.Create(Application.streamingAssetsPath + @"/" + path); //path为你想保存文件的路径。
        fs.Write(www.bytes, 0, www.bytes.Length);
        fs.Close();
    }

    private void LoadAssetBundleToDic(AssetBundle ab, string key)
    {
        Debug.Log("LoadAssetBundleToDic:" + key);
        if (!abDic.ContainsKey(key))
        {
            abDic.Add(key, ab);
        }
        if (!objDic.ContainsKey(key))
        {
            objDic.Add(key, new List<GameObject>());
        }

        foreach (var o in ab.LoadAllAssets<GameObject>())
        {
            //GameObject.Instantiate(o);
            objDic[key].Add(o);
        }
    }

    private void RemoveAssetBundleFromDic(string key)
    {
        Debug.Log("RemoveAssetBundleFromDic:" + key);
        

    }

    public void UnloadAssetBundles(string key)
    {
        Debug.Log("UnloadAssetBundles:" + key);
        if (abDic.ContainsKey(key))
        {
            abDic[key].Unload(true);
            abDic.Remove(key);
        }

        if (objDic.ContainsKey(key))
        {
            objDic[key].Clear();
            objDic.Remove(key);
        }
    }

    public void UnloadAssetBundles(string key,bool depend)
    {
        if (depend)
        {
            string[] strDepends = mManifest.GetAllDependencies(key);
            foreach(var str in strDepends)
            {
                UnloadAssetBundles(str);
            }
        }
        UnloadAssetBundles(key);
    }

    /// <summary>
    /// 获取资源的依赖资源
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string[] GetAllDependencies(string key,bool hasExt=false)
    {
        string path = key;
        if (!hasExt) path += _strExt;
        return mManifest.GetAllDependencies(path);
    }

    private void GetAllDependenciesSave(string key)
    {
        if (abDic.ContainsKey(key)) return;
        mDependList.Add(key+_strExt);
        Debug.Log(key + _strExt);
        string[] strDepends = GetAllDependencies(key, false);
        foreach (var str in strDepends)
        {
            if (!abDic.ContainsKey(str))
            {
                //迭代依赖
                GetAllDependencies(str, true);
                mDependList.Add(str);
                Debug.Log(str);
            }
        }
    }

    #region 本地加载
    private void LoadFromLocalAsyncCallBack()
    {
        if (mDependList.Count > 0)
        {
            //依次加载
            mLoadPath = mDependList[0];
            StartCoroutine(LoadFromLocalAsync(/*"game/test"*/LoadFromLocalAsyncCallBack, mDependList[0], false));
            mDependList.RemoveAt(0);
        }else
        {
            mLoadCallBack(mLoadPath, 1, true);
            Clear();
        }
    }

    /// <summary>
    /// 本地方式异步加载指定的assetbundle文件,以及他的依赖文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerator LoadFromLocalAsync(Action callback, string path,bool isDepend)
    {
        //AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + GetManifestName() + _strExt);
        //yield return request;
        //AssetBundle assetBundle = request.assetBundle; ;
        //AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        //assetBundle.Unload(false);

        //AssetBundleCreateRequest mRequest;
        //if (isDepend)
        //{
        //    string[] strDependencies = mManifest.GetAllDependencies(path);
        //    foreach (string str in strDependencies)
        //    {
        //        request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + @"/" + str);
        //        yield return request;
        //        LoadAssetBundleToDic(request.assetBundle, str);
        //    }
        //}

        mRequest = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + @"/" + path);
        yield return mRequest;
        LoadAssetBundleToDic(mRequest.assetBundle, path);
        //AssetBundle.UnloadAllAssetBundles(false);
        callback();
    }

    /// <summary>
    /// 本地方式异步加载所有assetbundle文件
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadFromLocalAsync(Action callback)
    {
        //AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + GetManifestName() + _strExt);
        //yield return request;
        //AssetBundle assetBundle = request.assetBundle; ;
        //AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        //assetBundle.Unload(false);
        AssetBundleCreateRequest request;
        string[] strAssetBundles = mManifest.GetAllAssetBundles();
        foreach (string str in strAssetBundles)
        {
            request = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + @"/" + str);
            yield return request;
            LoadAssetBundleToDic(request.assetBundle, str);
        }

        //assetBundle.Unload(false);
        //AssetBundle.UnloadAllAssetBundles(false);
        //isOk = true;
        callback();

    }
    #endregion

    #region 远程加载
    /// <summary>
    /// www方式异步加载指定的assetbundle文件，并保存到本地
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public IEnumerator LoadFromWWWAsync(Action callback, string path)
    {
        mWWW = new WWW(_strHost + GetManifestName()+  @"/" + path);
        yield return mWWW;
        if (!string.IsNullOrEmpty(mWWW.error))
        {
            Debug.Log("ERROR:" + mWWW.error);
            yield break;
        }

        SaveWWWAssetBundle(mWWW, path);

        LoadAssetBundleToDic(mWWW.assetBundle, path);
        //AssetBundle.UnloadAllAssetBundles(false);
        callback();
    }

    /// <summary>
    /// www方式异步加载所有assetbundle文件，并保存到本地
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadFromWWWAsync(Action callback)
    {
        //WWW www = new WWW(_strHost + GetManifestName() + GetManifestName() + _strExt);
        //yield return www;
        //if (!string.IsNullOrEmpty(www.error))
        //{
        //    Debug.Log("ERROR:" + www.error);
        //    yield break;

        //}

        //AssetBundle assetBundle = www.assetBundle;
        //SaveWWWAssetBundle(www, GetManifestName() + _strExt);

        //AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        string[] strAssetBundles = mManifest.GetAllAssetBundles();
        foreach(var str in strAssetBundles)
        {
            WWW www = new WWW(_strHost + GetManifestName() +@"/"+ str);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log("ERROR:" + www.error);
                yield break;

            }
            SaveWWWAssetBundle(www, str);

            LoadAssetBundleToDic(www.assetBundle, str);
        }

        //AssetBundle.UnloadAllAssetBundles(false);
        callback();
    }

    private void LoadFromWWWAsyncCallBack()
    {
        if (mDependList.Count > 0)
        {
            //依次加载
            mLoadPath = mDependList[0];
            StartCoroutine(LoadFromWWWAsync(/*"game/test"*/LoadFromWWWAsyncCallBack, mDependList[0]));
            mDependList.RemoveAt(0);
        }
        else
        {
            mLoadCallBack(mLoadPath, 1, true);
            Clear();
        }
    }
    #endregion

    /// <summary>
    /// 加载key指定的assetBundle
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="key"></param>
    public void LoadAsync(Action<string, float, bool> callback, string key)
    {
        //GetAllDependencies("game/test");
        mLoadCallBack = callback;
        //获取所有需要加载的路径
        GetAllDependenciesSave(key);

        //计算
        mDependCount = mDependList.Count;

        //判断本地是否存在文件
        if (File.Exists(Application.streamingAssetsPath + @"/" + key + _strExt))
        {
            mIsLocal = true;
            LoadFromLocalAsyncCallBack();
        }else
        {
            mIsLocal = false;
            LoadFromWWWAsyncCallBack();
        }

        //StartCoroutine(LoadFromLocalAsync(/*"game/test"*/LoadFromLocalAsyncCallBack, "game/test",false));
        //StartCoroutine(AssetBundleHelp.Instance().LoadManifestFromLocalAsync(LoadManifestFromLocalAsyncCallBack, "game/test"));
    }
}
