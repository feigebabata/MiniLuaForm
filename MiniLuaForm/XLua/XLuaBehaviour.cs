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

	public LuaTable self;

	void Awake()
	{
		self = AppMng.luaEnv.NewTable();

		LuaTable meta = AppMng.luaEnv.NewTable();
		meta.Set("__index", AppMng.luaEnv.Global);
		self.SetMetaTable(meta);
		meta.Dispose();

		self.Set("this", this);
		foreach (var injection in injections)
		{
			self.Set(injection.name, injection.value);
		}

		AppMng.luaEnv.DoString(luaScript.text, luaScript.name, self);
		Action luaAwake = self.Get<Action>("awake");
		self.Get("start", out luaStart);
		self.Get("update", out luaUpdate);
		self.Get("ondestroy", out luaOnDestroy);

		if (luaAwake != null)
		{
			luaAwake();
		}
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
		self.Dispose();
		injections = null;
	}
}
