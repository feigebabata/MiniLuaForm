using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using LitJson;
using System.IO;

public class ABDownLoad : MonoBehaviour 
{
	float m_downCount;
	public void Download(string _newFileIP, string _newFilesPath,string _oldFilesPath,Action<float> _peogress)
	{
		StartCoroutine (download(_newFileIP,_newFilesPath,_oldFilesPath,_peogress));
	}

	IEnumerator download(string _newFileIP, string _newFilesPath,string _oldFilesPath,Action<float> _peogress)
	{
		string newFilesJson;
		using(WWW loadNewFiles = new WWW (_newFileIP+"/"+_newFilesPath))
		{
			yield return loadNewFiles;
			if(!string.IsNullOrEmpty(loadNewFiles.error))
			{
				HttpSerMng.Instance.HttpErr(_newFileIP+"/"+_newFilesPath,"[ABDownLoad.download]文件下载失败:"+loadNewFiles.error);
			}
			newFilesJson = loadNewFiles.text;
			loadNewFiles.Dispose ();
		}

//		if (string.IsNullOrEmpty (newFilesJson)) 
//		{
//			downloadErr (_newFileIP+"/"+_newFilesPath);
//		}

		string oldFilesJson;

		using(WWW loadOldFiles = new  WWW (_oldFilesPath))
		{
			yield return loadOldFiles;
			oldFilesJson = loadOldFiles.text;
			loadOldFiles.Dispose ();
		}

		IDictionary kvs_new = null,kvs_old=null;
		kvs_new = JsonMapper.ToObject (newFilesJson)["files"] as IDictionary;
		if(!string.IsNullOrEmpty(oldFilesJson))
		{
			kvs_old = JsonMapper.ToObject (oldFilesJson)["files"] as IDictionary;
		}

		foreach (string k in kvs_new.Keys) 
		{
			if(kvs_old==null || !kvs_old.Contains(k) || kvs_new[k].ToString() != kvs_old[k].ToString())
			{
				string filePath = QPathHelper.GetAssetBundleOutPath () + "/" + k;
				initABFile (filePath);
				string downUrl = _newFileIP+"/" + k;
				using(UnityWebRequest uwr = new UnityWebRequest (downUrl))
				{
					uwr.downloadHandler = new DownloadHandlerFile (filePath);
					yield return uwr.SendWebRequest ();
					if (!string.IsNullOrEmpty (uwr.error)) 
					{
//						downloadErr (downUrl);
						HttpSerMng.Instance.HttpErr(downUrl,"[ABDownLoad.download]文件下载失败:"+uwr.error);
					}
					m_downCount++;
					if(_peogress!=null)
					{
						_peogress (m_downCount/kvs_new.Keys.Count);
					}
				}
			}
		}
		Destroy (this);
	}

//	void downloadErr(string _path)
//	{
//		Debug.LogError ("[ABDownLoad.download]文件下载失败:"+_path);
//		Destroy (this);
//	}

	void initABFile(string _path)
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
