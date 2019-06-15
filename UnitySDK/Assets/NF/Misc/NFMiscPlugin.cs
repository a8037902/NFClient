using UnityEngine;
using System.Collections;

namespace NFSDK
{
    public class NFMiscPlugin : NFIPlugin
    {
        public NFMiscPlugin(NFIPluginManager pluginManager)
        {
            mPluginManager = pluginManager;
        }
        public override string GetPluginName()
        {
            return "NFMiscPlugin";
        }

        public override void Install()
        {
            Debug.Log("NFMiscPlugin Install");
            AddModule<NFObjPoolModule>(new NFObjPoolModule(mPluginManager));
            AddModule<NFSoundModule>(new NFSoundModule(mPluginManager));
            AddModule<NFStateModule>(new NFStateModule(mPluginManager));
            AddModule<NFLuaModule>(new NFLuaModule(mPluginManager));
        }
        public override void Uninstall()
        {
            Debug.Log("NFMiscPlugin Uninstall");

            mPluginManager.RemoveModule("NFObjPoolModule");
            mPluginManager.RemoveModule("NFSoundModule");
            mPluginManager.RemoveModule("NFStateModule");
            mPluginManager.RemoveModule("NFLuaModule");

            mModules.Clear();
        }
    }
}
