using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using LitJson;
using System.IO;

public class AppMng : MngBase 
{
	internal static LuaEnv luaEnv = new LuaEnv(); //all lua behaviour shared one luaenv only!

	internal static float lastGCTime = 0;
	internal const float GCInterval = 1;//1 second 

	public static AppMng Instance {
		get;
		private set;
	}

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		main ();
	}

	void main()
	{
		PanelTool.Init ();
		ABTool.Init ();
		HttpSerMng.Instance.Init ();
		Init ();
		initFinsh ();
	}


	// Update is called once per frame
	void Update ()
	{
		if (Time.time - lastGCTime > GCInterval)
		{
			luaEnv.Tick();
			lastGCTime = Time.time;
		}
	}

	void initFinsh()
	{
		if(!System.IO.File.Exists(QPathHelper.GetAssetBundleOutPath()+"/ModulesConfig.txt"))
		{
			Debug.Log ("[AppMng.Init]未检测到ModulesConfig文件,开始拷贝资源文件夹");
			ABTool.Download (Application.streamingAssetsPath +"/"+ QPathHelper.GetPlatformName(),"hall/files.txt","...",(val)=>
				{
					Debug.LogFormat("[AppMng.Init]正在拷贝资源文件,已完成:{0}%",val*100);
					if(val == 1)
					{
						checkUpdates ();
					}
				});
		}
		else
		{
			checkUpdates ();
		}
	}

	public override void Init ()
	{
		base.Init ();
		Application.runInBackground=true;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	void checkUpdates()
	{
		Debug.Log ("[AppMng.checkUpdates]开始检查更新");
		PanelTool.InitPanel ("hall","systempop",1);
		PanelTool.OpenPanel ("hall","checkupdates",0,null);
	}

}

public class MngBase:MonoBehaviour
{
	public virtual void Init(){}
}
