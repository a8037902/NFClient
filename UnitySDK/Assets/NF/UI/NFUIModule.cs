﻿using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace NFSDK
{
	public class NFUIModule: NFIModule
    {
        public enum Event : int
        {
            LoginSuccess,
            LoginFailure,
            WorldList,
            ServerList,
			SelectServerSuccess,
        };
   
        public override void Awake() {}
        public override void AfterInit() {}
        public override void Execute() { }
        public override void BeforeShut() {}
        public override void Shut() { }

        public NFUIModule(NFIPluginManager pluginManager)
        {
            mPluginManager = pluginManager;
		}
        
        public override void Init() 
        { 
        }

        public void ShowUI<T>(bool bPushHistory = true, NFDataList varList = null) where T : UIDialog
        {
            if (mCurrentDialog != null)
            {
                mCurrentDialog.gameObject.SetActive(false);
                mCurrentDialog = null;
            }

            string name = typeof(T).ToString();
            GameObject uiObject;
            if (!mAllUIs.TryGetValue(name, out uiObject))
            {
                GameObject perfb = Resources.Load<GameObject>("UI/" + typeof(T).ToString());
                uiObject = GameObject.Instantiate(perfb);
                uiObject.name = name;

                uiObject.transform.SetParent(NFCRoot.Instance().transform);

                mAllUIs.Add(name, uiObject);
            }
            else
            {
                uiObject.SetActive(true);
            }

            if (uiObject)
            {
                T panel = uiObject.GetComponent<T>();
                if (varList != null)
                    panel.mUserData = varList;
                
                mCurrentDialog = panel;
                
                uiObject.SetActive(true);

                if (bPushHistory)
                {
                    mDialogs.Enqueue(panel);
                }
            }
        }

        public void ShowUI(string name, bool bPushHistory, NFDataList varList)
        {
            if (mCurrentDialog != null)
            {
                mCurrentDialog.gameObject.SetActive(false);
                mCurrentDialog = null;
            }

            GameObject uiObject;
            if (!mAllUIs.TryGetValue(name, out uiObject))
            {
                GameObject perfb = Resources.Load<GameObject>("UI/" + name);
                uiObject = GameObject.Instantiate(perfb);
                uiObject.name = name;

                uiObject.transform.SetParent(NFCRoot.Instance().transform);

                mAllUIs.Add(name, uiObject);
            }
            //else
            //{
            //    uiObject.SetActive(true);
            //}

            if (uiObject)
            {
                UIDialog panel = uiObject.GetComponent(name) as UIDialog;
                if (varList != null)
                    panel.mUserData = varList;

                mCurrentDialog = panel;

                uiObject.SetActive(true);

                if (bPushHistory)
                {
                    mDialogs.Enqueue(panel);
                }
            }
        }

        public void CloseUI()
        {
            if (mCurrentDialog)
            {
                mCurrentDialog.gameObject.SetActive(false);
                mCurrentDialog = null;
            }
            mDialogs.Clear();
        }

        Dictionary<string, GameObject> mAllUIs = new Dictionary<string,GameObject>();
        Queue<UIDialog> mDialogs = new Queue<UIDialog>();
        private UIDialog mCurrentDialog = null;
    }
}