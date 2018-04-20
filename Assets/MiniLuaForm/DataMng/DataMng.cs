using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DataMng : MngBase 
{
	public static DataMng Instance 
	{
		get;
		private set;
	}

	void Awake()
	{
		Instance = this;
	}

	public void ReadText(string _path,Action<string> _callback)
	{
		StartCoroutine (readText(_path,_callback));
	}

	IEnumerator readText (string _path,Action<string> _callback)
	{
		using(WWW w = new WWW(_path))
		{
			yield return w;
			if(!string.IsNullOrEmpty(w.error))
			{
				Debug.LogErrorFormat ("[DataMng.readText]:{0}\n{1}",w.error,_path);
			}
			_callback (w.text);
			w.Dispose ();
		}
	}
}
