using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System.IO;

public class AppMng : MngBase 
{
	public static LuaEnv luaEnv = new LuaEnv(); //all lua behaviour shared one luaenv only!
	public static LuaTable Common ;
	internal static float lastGCTime = 0;
	internal const float GCInterval = 1;//1 second 
	public static string Version="201807061159";

	public static AppMng Instance 
	{
		get;
		private set;
	}

	void Awake()
	{
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		Screen.orientation = ScreenOrientation.AutoRotation;  
		Screen.autorotateToLandscapeLeft = true;  
		Screen.autorotateToLandscapeRight = true;  
		Screen.autorotateToPortrait = false;  
		Screen.autorotateToPortraitUpsideDown = false;  
		
		Instance = this;
	}

	void Start()
	{
		main ();
	}

	void main()
	{
		Init ();
		PanelTool.Init ();
		ABTool.Init ();
		HttpSerMng.Instance.Init ();
		AudioMng.Instance.Init ();
		ShareMng.Instance.Init ();
		GVoiceMng.Instance.Init ();
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
		string mcfgPath = QPathHelper.GetAssetBundleOutPath () + "/ModulesConfig.txt";
		if(!System.IO.File.Exists(mcfgPath))
		{
			Debug.Log ("[AppMng.Init]未检测到ModulesConfig文件,开始拷贝资源文件夹");
			ABTool.Download (Application.streamingAssetsPath +"/"+ QPathHelper.GetPlatformName(),"hall/files.txt","...",(val)=>
				{
					Debug.LogFormat("[AppMng.Init]正在拷贝hall资源文件,已完成:{0}%",val*100);
					if(val == 1)
					{
						DataMng.Instance.ReadText(mcfgPath,(_mcfg_json)=>
							{
								byte downCount=0;
								Hashtable mcfg = MiniJSON.jsonDecode(_mcfg_json) as Hashtable;
								Hashtable ms = mcfg["ms"] as Hashtable;
								foreach (var jd in ms.Values) 
								{
									string modules = (jd as Hashtable) ["modules"].ToString();
									ABTool.Download(Application.streamingAssetsPath +"/"+ QPathHelper.GetPlatformName(),modules+"/files.txt","...",(_val)=>
										{
											Debug.LogFormat("[AppMng.Init]正在拷贝{1}资源文件,已完成:{0}%",_val*100,modules);
											if(_val==1)
											{
												downCount++;
												if(downCount==ms.Count)
												{
													checkUpdates();
												}
											}
										});
								}

							});
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

	public void QuitApp()
	{
		Application.Quit ();
	}

	public void Restart()
	{
		CorMng.Instance.Clear ();
		HttpSerMng.Instance.Clear ();
		EventTool.Clear ();
		PanelTool.Clear ();

		AudioMng.Instance.Init ();

		PanelTool.InitPanel ("hall","systempop",1);
		PanelTool.InitPanel ("hall","loading",1);
		PanelTool.InitPanel ("hall","toast",1);
		LuaTable logindata = luaEnv.NewTable ();
		PanelTool.OpenPanel ("hall","login",0,logindata);
	}

	void checkUpdates()
	{
		Debug.Log ("[AppMng.checkUpdates]开始检查更新");
		PanelTool.InitPanel ("hall","systempop",1);
		PanelTool.InitPanel ("hall","loading",1);
		PanelTool.InitPanel ("hall","toast",1);
		PanelTool.OpenPanel ("hall","checkupdates",0,null);
	}

}

public class MngBase:MonoBehaviour
{
	public virtual void Init(){}
}
