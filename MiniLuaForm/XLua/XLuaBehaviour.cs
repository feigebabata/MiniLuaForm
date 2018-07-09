using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;

[LuaCallCSharp]
public class XLuaBehaviour : MonoBehaviour 
{
	public TextAsset luaScript;
	public Injection[] injections;

	private Action luaStart;
	private Action luaUpdate;
	private Action luaOnDestroy;
	private Action<string> LuaCustomize;

	public LuaTable main;

	void Awake()
	{
		main = AppMng.luaEnv.NewTable();

		LuaTable meta = AppMng.luaEnv.NewTable();
		meta.Set("__index", AppMng.luaEnv.Global);
		main.SetMetaTable(meta);
		meta.Dispose();

		main.Set("this", this);
		foreach (var injection in injections)
		{
			main.Set(injection.name, injection.value);
		}

		AppMng.luaEnv.DoString(luaScript.text, luaScript.name, main);
		Action luaAwake = main.Get<Action>("awake");
		main.Get("start", out luaStart);
		main.Get("update", out luaUpdate);
		main.Get("ondestroy", out luaOnDestroy);
		main.Get("customize", out LuaCustomize);
		
		if (luaAwake != null)
		{
			luaAwake();
		}
//		Debug.LogWarning (main);
//		Debug.LogWarning ("Awake:"+name);
	}

	// Use this for initialization
	void Start ()
	{
		if (luaStart != null)
		{
			luaStart();
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (luaUpdate != null)
		{
			luaUpdate();
		}
	}

	void OnDestroy()
	{
		if (luaOnDestroy != null)
		{
			luaOnDestroy();
		}
		luaOnDestroy = null;
		luaUpdate = null;
		luaStart = null;
		main.Dispose();
		injections = null;
	}

	public void Customize(string _event)
	{
		if(LuaCustomize!=null)
		{
			LuaCustomize (_event);
		}
	}
}
