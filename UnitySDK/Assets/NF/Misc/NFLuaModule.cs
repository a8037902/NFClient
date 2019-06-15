using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Google.Protobuf;
using XLua;

namespace NFSDK
{
    public class NFLuaModule : NFIModule
    {
        public static string PUBLIC_KEY = "BgIAAACkAABSU0ExAAQAAAEAAQBVDDC5QJ+0uSCJA+EysIC9JBzIsd6wcXa+FuTGXcsJuwyUkabwIiT2+QEjP454RwfSQP8s4VZE1m4npeVD2aDnY4W6ZNJe+V+d9Drt9b+9fc/jushj/5vlEksGBIIC/plU4ZaR6/nDdMIs/JLvhN8lDQthwIYnSLVlPmY1Wgyatw==";

        internal static LuaEnv luaEnv = new LuaEnv(); //all lua behaviour shared one luaenv only!
        internal static float lastGCTime = 0;
        internal const float GCInterval = 1;//1 second 

        private Action luaAfterInit;
        private Action luaExecute;
        private Action luaBeforeShut;
        private Action luaShut;

        private LuaTable scriptEnv;
        public override void Awake()
        {

        }

        public override void Init()
        {
#if UNITY_EDITOR
            luaEnv.AddLoader((ref string filepath) =>
            {
                filepath = Application.dataPath + "/NF/Lua/" + filepath.Replace('.', '/') + ".lua";
                if (File.Exists(filepath))
                {
                    return File.ReadAllBytes(filepath);
                }
                else
                {
                    return null;
                }
            });
#else //为了让手机也能测试
        luaEnv.AddLoader((ref string filepath) =>
        {
            filepath = filepath.Replace('.', '/') + ".lua";
            TextAsset file = (TextAsset)Resources.Load(filepath);
            if (file != null)
            {
                return file.bytes;
            }
            else
            {
                return null;
            }
        });
#endif
            //luaEnv.DoString(@"
            //require 'main'
            //");

            scriptEnv = luaEnv.NewTable();

            // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
            LuaTable meta = luaEnv.NewTable();
            meta.Set("__index", luaEnv.Global);
            scriptEnv.SetMetaTable(meta);
            meta.Dispose();

            scriptEnv.Set("self", this);

            luaEnv.DoString(@"require 'main'", "NFLuaModule", scriptEnv);

            Action luaInit = scriptEnv.Get<Action>("init");
            scriptEnv.Get("afterinit", out luaAfterInit);
            scriptEnv.Get("execute", out luaExecute);
            scriptEnv.Get("beforeshut", out luaBeforeShut);
            scriptEnv.Get("shut", out luaShut);

            if (luaInit != null)
            {
                luaInit();
            }

        }

        public override void Execute()
        {
            if (luaExecute != null)
            {
                luaExecute();
            }
            if (Time.time - lastGCTime > GCInterval)
            {
                luaEnv.Tick();
                lastGCTime = Time.time;
            }
        }

        public override void BeforeShut()
        {
            if (luaBeforeShut != null)
            {
                luaBeforeShut();
            }
        }

        public override void Shut()
        {
            if (luaShut != null)
            {
                luaShut();
            }
            luaAfterInit = null;
            luaExecute = null;
            luaBeforeShut = null;
            luaShut = null;
            scriptEnv.Dispose();
        }


        public NFLuaModule(NFIPluginManager pluginManager)
        {
            mPluginManager = pluginManager;
        }

        public override void AfterInit()
        {

            if (luaAfterInit != null)
            {
                luaAfterInit();
            }
        }

        public void Test()
        {
            Debug.Log("NFLuaModule:test");
        }

        public LuaEnv GetLuaEnv()
        {
            return luaEnv;
        }

    }

    //[LuaCallCSharp]
    //public static class LuaCallNF
    //{
    //    public static void Test(string str)
    //    {
    //        Debug.Log(str);
    //    }

    //    public static NFGUID CreateObject(NFGUID self, int nContainerID, int nGroupID, string strClassName, string strConfigIndex, NFDataList arg)
    //    {
    //        NFIObject obj = NFCPluginManager.Instance().FindModule<NFIKernelModule>().CreateObject(self, nContainerID, nGroupID, strClassName, strConfigIndex, arg);
    //        if (obj != null)
    //        {
    //            return obj.Self();
    //        }

    //        return new NFGUID();
    //    }

    //    public static double QueryPropertyFloat(NFGUID self, string strPropertyName)
    //    {
    //        return NFCPluginManager.Instance().FindModule<NFIKernelModule>().QueryPropertyFloat(self, strPropertyName);
    //    }

    //    public static Int64 QueryPropertyInt(NFGUID self, string strPropertyName)
    //    {
    //        return NFCPluginManager.Instance().FindModule<NFIKernelModule>().QueryPropertyInt(self, strPropertyName);
    //    }

    //    public static string QueryPropertyString(NFGUID self, string strPropertyName)
    //    {
    //        return NFCPluginManager.Instance().FindModule<NFIKernelModule>().QueryPropertyString(self, strPropertyName);
    //    }

    //    public static NFGUID QueryPropertyObject(NFGUID self, string strPropertyName)
    //    {
    //        return NFCPluginManager.Instance().FindModule<NFIKernelModule>().QueryPropertyObject(self, strPropertyName);
    //    }

    //    public static bool SetPropertyFloat(NFGUID self, string strPropertyName, double fValue)
    //    {
    //        return NFCPluginManager.Instance().FindModule<NFIKernelModule>().SetPropertyFloat(self, strPropertyName, fValue);
    //    }

    //    public static bool SetPropertyInt(NFGUID self, string strPropertyName, Int64 nValue)
    //    {
    //        return NFCPluginManager.Instance().FindModule<NFIKernelModule>().SetPropertyInt(self, strPropertyName, nValue);
    //    }

    //    public static bool SetPropertyString(NFGUID self, string strPropertyName, string strValue)
    //    {
    //        return NFCPluginManager.Instance().FindModule<NFIKernelModule>().SetPropertyString(self, strPropertyName, strValue);
    //    }

    //    public static bool SetPropertyObject(NFGUID self, string strPropertyName, NFGUID objectValue)
    //    {
    //        return NFCPluginManager.Instance().FindModule<NFIKernelModule>().SetPropertyObject(self, strPropertyName, objectValue);
    //    }

    //    private static Dictionary<string, Action<NFGUID, int, int, NFIObject.CLASS_EVENT_TYPE, string, string>> mClassEventHandlerDelegateMap = new Dictionary<string, Action<NFGUID, int, int, NFIObject.CLASS_EVENT_TYPE, string, string>>();
    //    //public static Action<NFGUID, int, int, NFIObject.CLASS_EVENT_TYPE, string, string> ClassEventHandlerDelegate = (self, nContainerID, nGroupID, eType, strClassName, strConfigIndex) =>
    //    //  {
    //    //      Debug.Log("TestDelegate in c#:" + strClassName);
    //    //  };
    //    public static void RegisterClassCallBack(string strClassName, Action<NFGUID, int, int, NFIObject.CLASS_EVENT_TYPE, string, string> handler)
    //    {
    //        if (!mClassEventHandlerDelegateMap.ContainsKey(strClassName))
    //        {
    //            mClassEventHandlerDelegateMap[strClassName]= handler;
    //        }else
    //        {
    //            mClassEventHandlerDelegateMap[strClassName] += handler;
    //        }
    //        NFCPluginManager.Instance().FindModule<NFIKernelModule>().RegisterClassCallBack(strClassName, OnClassEventHandler);
    //    }

    //    private static void OnClassEventHandler(NFGUID self, int nContainerID, int nGroupID, NFIObject.CLASS_EVENT_TYPE eType, string strClassName, string strConfigIndex)
    //    {
    //        if (mClassEventHandlerDelegateMap.ContainsKey(strClassName))
    //        {
    //            mClassEventHandlerDelegateMap[strClassName](self, nContainerID, nGroupID, eType, strClassName, strConfigIndex);
    //        }
    //        //if (ClassEventHandlerDelegate != null)
    //        //{
    //        //    ClassEventHandlerDelegate(self, nContainerID, nGroupID, eType, strClassName, strConfigIndex);
    //        //}
    //    }

    //    private static Dictionary<string, Dictionary<NFGUID, Action<NFGUID, string, NFDataList.TData, NFDataList.TData>>> mPropertyEventHandlerDelegateMap = new Dictionary<string, Dictionary<NFGUID, Action<NFGUID, string, NFDataList.TData, NFDataList.TData>>>();
    //    public static void RegisterPropertyCallback(NFGUID self, string strPropertyName, Action<NFGUID, string, NFDataList.TData, NFDataList.TData> handler)
    //    {
    //        if (!mPropertyEventHandlerDelegateMap.ContainsKey(strPropertyName))
    //        {
    //            mPropertyEventHandlerDelegateMap[strPropertyName] = new Dictionary<NFGUID, Action<NFGUID, string, NFDataList.TData, NFDataList.TData>>();
    //            mPropertyEventHandlerDelegateMap[strPropertyName][self] = handler;
    //        }
    //        else
    //        {
    //            if (!mPropertyEventHandlerDelegateMap[strPropertyName].ContainsKey(self))
    //            {
    //                mPropertyEventHandlerDelegateMap[strPropertyName][self] = handler;
    //            }else
    //            {
    //                mPropertyEventHandlerDelegateMap[strPropertyName][self] += handler;
    //            }
    //        }
    //        NFCPluginManager.Instance().FindModule<NFIKernelModule>().RegisterPropertyCallback(self, strPropertyName, OnPropertyEventHandler);
    //    }

    //    private static void OnPropertyEventHandler(NFGUID self, string strProperty, NFDataList.TData oldVar, NFDataList.TData newVar)
    //    {
    //        if (mPropertyEventHandlerDelegateMap.ContainsKey(strProperty))
    //        {
    //            if (mPropertyEventHandlerDelegateMap[strProperty].ContainsKey(self))
    //            {
    //                mPropertyEventHandlerDelegateMap[strProperty][self]( self, strProperty, oldVar, newVar);
    //            }
    //        }
    //    }

    //    private static Dictionary<string, Dictionary<NFGUID, Action<NFGUID, string, NFIRecord.eRecordOptype, int, int, NFDataList.TData, NFDataList.TData>>> mRecordEventHandlerDelegateMap = new Dictionary<string, Dictionary<NFGUID, Action<NFGUID, string, NFIRecord.eRecordOptype, int, int, NFDataList.TData, NFDataList.TData>>>();
    //    public static void RegisterRecordCallback(NFGUID self, string strRecordName, Action<NFGUID, string, NFIRecord.eRecordOptype, int, int, NFDataList.TData, NFDataList.TData> handler)
    //    {
    //        if (!mRecordEventHandlerDelegateMap.ContainsKey(strRecordName))
    //        {
    //            mRecordEventHandlerDelegateMap[strRecordName] = new Dictionary<NFGUID, Action<NFGUID, string, NFIRecord.eRecordOptype, int, int, NFDataList.TData, NFDataList.TData>>();
    //            mRecordEventHandlerDelegateMap[strRecordName][self] = handler;
    //        }
    //        else
    //        {
    //            if (!mRecordEventHandlerDelegateMap[strRecordName].ContainsKey(self))
    //            {
    //                mRecordEventHandlerDelegateMap[strRecordName][self] = handler;
    //            }
    //            else
    //            {
    //                mRecordEventHandlerDelegateMap[strRecordName][self] += handler;
    //            }
    //        }
    //        NFCPluginManager.Instance().FindModule<NFIKernelModule>().RegisterRecordCallback(self, strRecordName, OnRecordEventHandler);
    //    }

    //    private static void OnRecordEventHandler(NFGUID self, string strRecordName, NFIRecord.eRecordOptype eType, int nRow, int nCol, NFDataList.TData oldVar, NFDataList.TData newVar)
    //    {
    //        if (mRecordEventHandlerDelegateMap.ContainsKey(strRecordName))
    //        {
    //            if (mRecordEventHandlerDelegateMap[strRecordName].ContainsKey(self))
    //            {
    //                mRecordEventHandlerDelegateMap[strRecordName][self](self, strRecordName, eType, nRow, nCol, oldVar, newVar);
    //            }
    //        }
    //    }

    //    //private static Dictionary<int, Action<NFDataList>> mEventHandlerDelegateMap = new Dictionary<int, Action<NFDataList>>();
    //    //public static void RegisterEventCallBack(int nEventID, Action<NFDataList> handler)
    //    //{
    //    //    if (!mEventHandlerDelegateMap.ContainsKey(nEventID))
    //    //    {
    //    //        mEventHandlerDelegateMap[nEventID] = handler;
    //    //    }
    //    //    else
    //    //    {
    //    //        mEventHandlerDelegateMap[nEventID] += handler;
    //    //    }
    //    //    NFCPluginManager.Instance().FindModule<NFIEventModule>().RegisterCallback(nEventID, OnEventHandler);
    //    //}

    //    //private static void OnEventHandler(NFDataList valueList)
    //    //{

    //    //}
    //}
}