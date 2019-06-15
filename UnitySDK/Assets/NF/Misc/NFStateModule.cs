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
    public class NFStateModule : NFIModule
    {
        private BaseState mState;
        public BaseState state
        {
            get { return mState; }
            set { mState = value; }
        }
        public override void Awake()
        {
        }

        public override void Init()
        {
        }

        public override void Execute()
        {
            if (mState != null)
            {
                mState.Execute(this);
            }
        }

        public override void BeforeShut()
        {
        }

        public override void Shut()
        {
        }


        public NFStateModule(NFIPluginManager pluginManager)
        {
            mPluginManager = pluginManager;
        }

        public override void AfterInit()
        {


        }

    }
}