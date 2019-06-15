using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NFSDK;

public class UIPlayer : UIDialog
{
    private NFIKernelModule mKernelModule;
    private NFPlayerModule mPlayerModule;
    // Use this for initialization
    void Start () {
        mKernelModule = NFCPluginManager.Instance().FindModule<NFIKernelModule>();
        mPlayerModule = NFCPluginManager.Instance().FindModule<NFPlayerModule>();

        mKernelModule.RegisterPropertyCallback(mPlayerModule.mRoleID, NFrame.Player.HP, PropertyHPEventHandler);
        mKernelModule.RegisterPropertyCallback(mPlayerModule.mRoleID, NFrame.Player.MP, PropertyMPEventHandler);

        Debug.Log("HP:" + mKernelModule.QueryPropertyInt(mPlayerModule.mRoleID, NFrame.Player.HP));
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void PropertyHPEventHandler(NFGUID self, string strProperty, NFDataList.TData oldVar, NFDataList.TData newVar)
    {
        Debug.Log("PropertyHPEventHandler:" + newVar.IntVal());
    }

    void PropertyMPEventHandler(NFGUID self, string strProperty, NFDataList.TData oldVar, NFDataList.TData newVar)
    {
        Debug.Log("PropertyMPEventHandler:" + newVar.IntVal());
    }
}
