using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Google.Protobuf;

namespace NFSDK
{
    public class NFSoundModule : NFIModule
    {
        private NFObjPoolModule mObjPoolModule;

        private GameObject mObjSound;
        private List<GameObject> mListObjSound;

        private float mFTime=0;

        private const float IntervalTime = 5;
        private const string SoundName = "Sound";

        private bool mIsSuspend = false;

        public override void Awake()
        {
        }

        public override void Init()
        {
        }

        public override void Execute()
        {
            mFTime += Time.deltaTime;
            if (mFTime > IntervalTime)
            {
                mFTime -= IntervalTime;

                //丢弃播放完的声音
                for (int i = mListObjSound.Count - 1; i >= 0; i--)
                {
                    if (!mListObjSound[i].GetComponent<AudioSource>().isPlaying)
                    {                        
                        Return(mListObjSound[i]);
                    }                    
                }
            }
        }

        public override void BeforeShut()
        {
        }

        public override void Shut()
        {
        }


        public NFSoundModule(NFIPluginManager pluginManager)
        {
            mPluginManager = pluginManager;
        }

        public override void AfterInit()
        {
            mObjPoolModule = mPluginManager.FindModule<NFObjPoolModule>();

            mObjSound = new GameObject(SoundName);
            mObjSound.AddComponent<AudioSource>().playOnAwake = false;

            UnityEngine.Object.DontDestroyOnLoad(mObjSound);

            mListObjSound = new List<GameObject>();
        }

        public void Play(AudioClip clip,bool loop=false)
        {
            GameObject obj = mObjPoolModule.Get(mObjSound, Vector3.zero, Quaternion.identity) as GameObject;
            AudioSource audio = obj.GetComponent<AudioSource>();
            audio.clip = clip;
            audio.loop = loop;
            if (mIsSuspend)
            {
                audio.volume = 0;
            }
            audio.Play();
            mListObjSound.Add(obj);
        }

        public void Suspend()
        {
            mIsSuspend = true;
            for (int i = mListObjSound.Count - 1; i >= 0; i--)
            {
                mListObjSound[i].GetComponent<AudioSource>().volume = 0;
                Return(mListObjSound[i]);
            }
        }

        public void Recovery()
        {
            mIsSuspend = false;
            for (int i = mListObjSound.Count - 1; i >= 0; i--)
            {
                mListObjSound[i].GetComponent<AudioSource>().volume = 1;
            }
        }

        //还给对象池
        private void Return(GameObject obj)
        {
            mObjPoolModule.Return(obj);
            mListObjSound.Remove(obj);
        }

        public void Stop(AudioClip clip)
        {
            for (int i = mListObjSound.Count - 1; i >= 0; i--)
            {
                //GameObject obj = item.Value[i];
                if (mListObjSound[i].GetComponent<AudioSource>().clip == clip)
                {
                    Return(mListObjSound[i]);
                }
            }
        }

        public void StopAll()
        {
            for (int i = mListObjSound.Count - 1; i >= 0; i--)
            {
                //GameObject obj = item.Value[i];
                mListObjSound[i].GetComponent<AudioSource>().Stop();
                Return(mListObjSound[i]);
            }
        }
    }
}