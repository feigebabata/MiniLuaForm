using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;

public static class PanelTool 
{
	static Transform canves_T;
	static Dictionary<string,GameObject> panel_GOs = new Dictionary<string, GameObject>();

	public static void Init ()
	{
		canves_T = GameObject.Find ("Canvas").transform;
	}

	public static void Clear(string _modules="")
	{
		List<string> keys = new List<string> (panel_GOs.Count);
		foreach (string key in panel_GOs.Keys) 
		{
			keys.Add (key);	
		}
		foreach (var key in keys) 
		{
			if (string.IsNullOrEmpty(_modules)) 
			{
				GameObject.Destroy (panel_GOs[key]);
				panel_GOs.Remove (key);
			} 
			else 
			{
				if(key.Split('.')[0]==_modules)
				{
					GameObject.Destroy (panel_GOs[key]);
					panel_GOs.Remove (key);
				}
			}
		}
	}

	public static void InitPanel(string _modules,string _panelName,int _layer)
	{
		string key = _modules + "." + _panelName;
		if(panel_GOs.ContainsKey(key))
		{
			Debug.LogWarningFormat ("[PanelTool.InitPanel]已有此界面:{0}.{1}",_modules,_panelName);
			return;
		}
		string path = "/"+_modules+"/panels.assetbundle";
		ABTool.LoadGO (path,_panelName,(_go)=>
			{
				GameObject go = GameObject.Instantiate(_go);
				go.name = _go.name;
				go.transform.SetParent(canves_T.GetChild(_layer),false);
				panel_GOs[key]=go;
				go.SetActive(false);
			});
	}

	public static void OpenPanel(string _modules,string _panelName,int _layer,LuaTable _tab)
	{
		string key = _modules + "." + _panelName;
		if(panel_GOs.ContainsKey(key))
		{
			GameObject go = panel_GOs[key];
			go.transform.SetSiblingIndex(go.transform.parent.childCount-1);
			go.SetActive (true);
			go.GetComponent<PanelBase> ().Open ();
			go.GetComponent<XLuaBehaviour> ().main.Get<Action<LuaTable>> ("onopen")(_tab);
		}
		else
		{
			string path = "/"+_modules+"/panels.assetbundle";
			ABTool.LoadGO (path,_panelName,(_go)=>
				{
					GameObject go = GameObject.Instantiate(_go);
					go.name = _go.name;
					go.transform.SetParent(canves_T.GetChild(_layer),false);
					go.transform.SetSiblingIndex(go.transform.parent.childCount-1);
					go.GetComponent<PanelBase> ().Open ();
					go.GetComponent<XLuaBehaviour> ().main.Get<Action<LuaTable>> ("onopen")(_tab);
					panel_GOs[key]=go;
				});
		}
	}

	public static void ClosePanel(string _modules,string _panelName)
	{
		string key = _modules + "." + _panelName;
		if(panel_GOs.ContainsKey(key))
		{
			GameObject go = panel_GOs[key];
			if (go.activeSelf) 
			{
				go.GetComponent<PanelBase> ().Close ();
				LuaTable tab = go.GetComponent<XLuaBehaviour> ().main;
				tab.Get<Action> ("onclose")();
			}
		}
		else
		{
			Debug.LogWarningFormat ("[PanelTool.ClosePanel]无此界面:{0}.{1}",_modules,_panelName);
		}
	}
}
