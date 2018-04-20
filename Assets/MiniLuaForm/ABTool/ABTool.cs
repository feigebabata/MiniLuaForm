using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;

public static class ABTool
{
	public struct LoadGOData
	{
		public string Path;
		public string Name;
		public Action<GameObject> Callback;
	}
	public struct LoadACData
	{
		public string Path;
		public string Name;
		public Action<AudioClip> Callback;
	}

	public struct LoadSpriteData
	{
		public string Path;
		public string Name;
		public Action<Sprite> Callback;
	}

	static GameObject m_root_GO;
	public static readonly string ResUrl = "http://"+QPathHelper.GetPlatformName();
	static List<LoadGOData> loadGOs = new List<LoadGOData>();
	static List<LoadACData> loadACs = new List<LoadACData>();
	static List<LoadSpriteData> loadSprites = new List<LoadSpriteData>();
	public static void Init()
	{
		m_root_GO = GameObject.Find ("ABTool");
	}
		

	public static void Download(string _newFileIP, string _newFilesPath,string _oldFilesPath,Action<float> _peogress)
	{
		m_root_GO.AddComponent<ABDownLoad> ().Download (_newFileIP,_newFilesPath,_oldFilesPath,_peogress);
	}

	public static void LoadGO(string _path,string _name,Action<GameObject> _callback)
	{
		LoadGOData data;
		data.Path = _path;
		data.Name = _name;
		data.Callback = _callback;
		loadGOs.Add (data);
		if(loadGOs.Count==1)
		{
			loadGO ();
		}
	}

	static void loadGO()
	{
		if(loadGOs.Count==0)
		{
			return;
		}
		LoadGOData data = loadGOs[0];
		AssetBundle ab = AssetBundle.LoadFromFile (QPathHelper.GetAssetBundleOutPath()+data.Path);
		if(ab!=null)
		{
			GameObject go = ab.LoadAsset<GameObject> (data.Name);
			if(go!=null)
			{
				data.Callback (go);
			}
			else
			{
				Debug.LogErrorFormat ("[ABMng.loadGO]资源加载为空:{0}.{1}",data.Path,data.Name);
			}
			ab.Unload (false);
			ab = null;
		}
		else
		{
			Debug.LogError ("[ABMng.loadGO]资源加载为空:"+data.Path);
		}
		loadGOs.RemoveAt (0);
		loadGO ();
	}

	public static void LoadAC(string _path,string _name,Action<AudioClip> _callback)
	{
		LoadACData data;
		data.Path = _path;
		data.Name = _name;
		data.Callback = _callback;
		loadACs.Add (data);
		if(loadACs.Count==1)
		{
			loadAC ();
		}
	}

	static void loadAC()
	{
		if(loadACs.Count==0)
		{
			return;
		}
		LoadACData data = loadACs[0];
		AssetBundle ab = AssetBundle.LoadFromFile (QPathHelper.GetAssetBundleOutPath()+data.Path);
		if(ab!=null)
		{
			AudioClip go = ab.LoadAsset<AudioClip> (data.Name);
			if(go!=null)
			{
				data.Callback (go);
			}
			else
			{
				Debug.LogErrorFormat ("[ABMng.loadAC]资源加载为空:{0}.{1}",data.Path,data.Name);
			}
			ab.Unload (false);
			ab = null;
		}
		else
		{
			Debug.LogError ("[ABMng.loadAC]资源加载为空:"+data.Path);
		}
		loadACs.RemoveAt (0);
		loadAC ();
	}

	public static void LoadSprite(string _path,string _name,Action<Sprite> _callback)
	{
		LoadSpriteData data;
		data.Path = _path;
		data.Name = _name;
		data.Callback = _callback;
		loadSprites.Add (data);
		if(loadSprites.Count==1)
		{
			loadSprite ();
		}
	}

	static void loadSprite()
	{
		if(loadSprites.Count==0)
		{
			return;
		}
		LoadSpriteData data = loadSprites[0];
		AssetBundle ab = AssetBundle.LoadFromFile (QPathHelper.GetAssetBundleOutPath()+data.Path);
		if(ab!=null)
		{
			Sprite go = ab.LoadAsset<Sprite> (data.Name);
			if(go!=null)
			{
				data.Callback (go);
			}
			else
			{
				Debug.LogErrorFormat ("[ABMng.loadSprite]资源加载为空:{0}.{1}",data.Path,data.Name);
			}
			ab.Unload (false);
			ab = null;
		}
		else
		{
			Debug.LogError ("[ABMng.loadSprite]资源加载为空:"+data.Path);
		}
		loadSprites.RemoveAt (0);
		loadSprite ();
	}

}
