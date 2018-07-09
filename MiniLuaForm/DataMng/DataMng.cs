using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using XLua;

public class DataMng : MngBase 
{
	public static DataMng Instance 
	{
		get;
		private set;
	}

	public static XLua.LuaTable Common;


	[DllImport ("__Internal")]
	static extern void _CopyTextToClipboard (string _str);

	void Awake()
	{
		Instance = this;
		Common = AppMng.luaEnv.NewTable ();
	}

	public void Del_PP_Key(string _key)
	{
		PlayerPrefs.DeleteKey (_key);
	}

	public void ReadText(string _path,Action<string> _callback)
	{
		if(_path.IndexOf("file:///")==-1 && _path.IndexOf("http")!=0)
		{
			_path = "file:///" + _path;
		}
		StartCoroutine (readText(_path,_callback));
	}

	IEnumerator readText (string _path,Action<string> _callback)
	{
		using(WWW w = new WWW(_path))
		{
			yield return w;
			if(!string.IsNullOrEmpty(w.error))
			{
//				Debug.LogErrorFormat ("[DataMng.readText]:{0}\n{1}",w.error,_path);
				HttpSerMng.Instance.HttpErr(_path,"[DataMng.readText]:"+w.error);
			}
			_callback (w.text);
			w.Dispose ();
		}
	}

	public void ReadTexture(string _path,Action<Texture2D> _callback)
	{
		if(_path.IndexOf("file:///")==-1 && _path.IndexOf("http")!=0)
		{
			_path = "file:///" + _path;
		}
		StartCoroutine (readTexture(_path,_callback));
	}

	IEnumerator readTexture (string _path,Action<Texture2D> _callback)
	{
		using(WWW w = new WWW(_path))
		{
			yield return w;
			yield return new WaitForEndOfFrame ();
			if(!string.IsNullOrEmpty(w.error))
			{
//				Debug.LogErrorFormat ("[DataMng.readTexture]:{0}\n{1}",w.error,_path);
				HttpSerMng.Instance.HttpErr(_path,"[DataMng.readTexture]:"+w.error);
			}
			_callback (w.texture);
			w.Dispose ();
		}
	}

	public static LuaTable Str_Sqlit(string _text,string _code,string _not)
	{
		LuaTable tab = AppMng.luaEnv.NewTable ();
		char code = _code[0];
		string[] strArr = _text.Split (code);
		int idx=1;
		for (int i = 0; i < strArr.Length; i++) 
		{	
			if(strArr[i]!=_not)
			{
				tab.Set <int,string>(idx,strArr[i]);
				idx++;
			}	
		}
		return tab;
	}

	public static Vector3 RandomVec3(Vector3 _v3,float _val)
	{
		return new Vector3(UnityEngine.Random.Range(-_val,_val),UnityEngine.Random.Range(-_val,_val),UnityEngine.Random.Range(-_val,_val))+_v3;
	}

	public static string TimeStampToDate(long timeStamp)
	{
		DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
		DateTime dt = startTime.AddSeconds(timeStamp);
		return dt.ToString("yyyy-MM-dd  HH:mm:ss");
	}



	public static void CopyTextToClipboard(string _str)
	{
		switch(Application.platform)
		{
		case RuntimePlatform.Android:

			AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
			jo.Call("CopyTextToClipboard",_str);
			break;
		case RuntimePlatform.IPhonePlayer:
			_CopyTextToClipboard (_str);
			break;
		}
	}

	public float BatteryLevel()
	{
		return SystemInfo.batteryLevel;
	}
}
