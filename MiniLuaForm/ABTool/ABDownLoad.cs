using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;

public class ABDownLoad : MonoBehaviour 
{
	float m_downCount=0;
	public void Download(string _newFileIP, string _newFilesPath,string _oldFilesPath,Action<float> _peogress)
	{
		StartCoroutine (download(_newFileIP,_newFilesPath,_oldFilesPath,_peogress));
	}

	IEnumerator download(string _newFileIP, string _newFilesPath,string _oldFilesPath,Action<float> _peogress)
	{
		string newFilesJson;
		string loadNewFileUrl = _newFileIP + "/" + _newFilesPath;
		if(loadNewFileUrl.IndexOf("file:///")==-1 && loadNewFileUrl.IndexOf("http")!=0)
		{
			loadNewFileUrl = "file:///" + loadNewFileUrl;
		}
		using(WWW loadNewFiles = new WWW (loadNewFileUrl))
		{
			yield return loadNewFiles;
			if(!string.IsNullOrEmpty(loadNewFiles.error))
			{
				HttpSerMng.Instance.HttpErr(loadNewFiles.url,"[ABDownLoad.download]文件下载失败:"+loadNewFiles.error);
			}
			newFilesJson = loadNewFiles.text;
			loadNewFiles.Dispose ();
		}

		string oldFilesJson;
		if(_oldFilesPath.IndexOf("file:///")==-1 && _oldFilesPath.IndexOf("http")!=0)
		{
			_oldFilesPath = "file:///" + _oldFilesPath;
		}
		using(WWW loadOldFiles = new  WWW (_oldFilesPath))
		{
			yield return loadOldFiles;
			oldFilesJson = loadOldFiles.text;
			loadOldFiles.Dispose ();
		}

		Hashtable kvs_new = null,kvs_old=null;
		kvs_new = (MiniJSON.jsonDecode (newFilesJson) as Hashtable) ["files"] as Hashtable;
		if(!string.IsNullOrEmpty(oldFilesJson))
		{
			kvs_old = (MiniJSON.jsonDecode (oldFilesJson) as Hashtable) ["files"] as Hashtable;
		}

		foreach (string k in kvs_new.Keys) 
		{
			if(kvs_old==null || !kvs_old.Contains(k) || kvs_new[k].ToString() != kvs_old[k].ToString())
			{
				string filePath = QPathHelper.GetAssetBundleOutPath () + "/" + k;
				InitABFile (filePath);
				string downUrl = _newFileIP+"/" + k;
				if(downUrl.IndexOf("file:///")==-1 && downUrl.IndexOf("http")!=0)
				{
					downUrl = "file:///" + downUrl;
				}
				using(UnityWebRequest uwr = new UnityWebRequest (downUrl))
				{
					uwr.downloadHandler = new DownloadHandlerFile (filePath);
					yield return uwr.SendWebRequest ();
					if (!string.IsNullOrEmpty (uwr.error)) 
					{
						HttpSerMng.Instance.HttpErr(uwr.url,"[ABDownLoad.download]文件下载失败:"+uwr.error);
					}
				}
			}
			m_downCount++;
			if(_peogress!=null)
			{
				_peogress (m_downCount/kvs_new.Keys.Count);
			}
		}
		Destroy (this);
	}

	public static void InitABFile(string _path)
	{
		if(File.Exists(_path))
		{
			File.Delete (_path);
		}
		else
		{
			int idx = _path.LastIndexOf ("/");
			string dir = _path.Substring (0,idx);
			if (!Directory.Exists (dir)) 
			{
				Directory.CreateDirectory(dir);
			}
		}
	}
}
