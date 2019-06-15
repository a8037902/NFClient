using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NFSDK
{
    public class NFObjPoolModule : NFIModule
    {

        public Dictionary<string, List<GameObject>> mDicPool;
        public Dictionary<GameObject, float> mDicTime;
        //public static Dictionary<string, PoolGameObject> poolOne = new Dictionary<string, PoolGameObject> { };
        private StringBuilder mStrBuilder;

        private float mFIntervalTime = 0;
        private float mFIdleTime = 5;

        //private Dictionary<NFGUID, GameObject> mGameObjectMap = new Dictionary<NFGUID, GameObject>();

        public NFObjPoolModule(NFIPluginManager pluginManager)
        {
            mPluginManager = pluginManager;
        }


        public override void Awake()
        {
        }
        public override void Init()
        {
            mDicPool = new Dictionary<string, List<GameObject>> { };
            mDicTime = new Dictionary<GameObject, float> { };

            mStrBuilder = new StringBuilder();
        }

        public override void AfterInit()
        {

        }

        public override void Execute() {
            mFIntervalTime += Time.deltaTime;
            if (mFIntervalTime >= mFIdleTime && mFIdleTime > 1)
            {
                mFIntervalTime = 0;
                //Debug.Log(_list.Count);
                //清除长时间没用的物体
                foreach (var item in mDicPool)
                {
                    for (int i = item.Value.Count - 1; i >= 0; i--)
                    {
                        //GameObject obj = item.Value[i];
                        if (Time.time - mDicTime[item.Value[i]] >= mFIdleTime)
                        {
                            mDicTime.Remove(item.Value[i]);
                            Object.Destroy(item.Value[i]);
                            item.Value.RemoveAt(i);
                            
                        }
                    }
                }
            }
        }
        public override void BeforeShut() { }
        public override void Shut() { }

        public Object Get(Object original, Vector3 position, Quaternion rotation, Transform parent=null)
        {
            mStrBuilder.Remove(0, mStrBuilder.Length);
            mStrBuilder.Append(original.name);
            mStrBuilder.Append("(Clone)");
            GameObject o;
            //池中存在，则从池中取出
            if (mDicPool.ContainsKey(mStrBuilder.ToString()) && mDicPool[mStrBuilder.ToString()].Count > 0)
            {
                List<GameObject> list = mDicPool[mStrBuilder.ToString()];
                o = list[0];
                list.Remove(list[0]);
                //重新初始化相关状态
                o.SetActive(true);
                o.transform.position = position;
                o.transform.rotation = rotation;

                if(parent!=null) o.transform.SetParent(parent);
            }
            //池中没有则实例化gameobejct
            else
            {
                if (parent != null)
                {
                    o = Object.Instantiate(original, position, rotation, parent) as GameObject;
                }
                else
                {
                    o = Object.Instantiate(original, position, rotation) as GameObject;
                }
            }
            return o;
        }

        public Object Return(GameObject o)
        {
            string key = o.name;
            //_stringBuilder.Remove(0, _stringBuilder.Length);
            //_stringBuilder.Append(key);
            if (!mDicPool.ContainsKey(key))
            {
                mDicPool[key] = new List<GameObject>();
                
            }

            List<GameObject> list = mDicPool[key];
            list.Add(o);
            if (mDicTime.ContainsKey(o))
            {
                mDicTime[o] = Time.time;
            }
            else
            {
                mDicTime.Add(o, Time.time);
            }
            //Debug.Log("GameObjectPool:Return=" + o.name);

            o.SetActive(false);
            return o;
        }

        public void Clear()
        {
            foreach (var item in mDicPool)
            {
                for (int i = item.Value.Count - 1; i >= 0; i--)
                {
                    mDicTime.Remove(item.Value[i]);
                    Object.Destroy(item.Value[i]);
                    item.Value.RemoveAt(i);
                }
            }
        }
    }

}
