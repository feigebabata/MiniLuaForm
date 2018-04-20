using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using System;

public class CorMng : MngBase 
{
	public static CorMng Instance {
		get;
		private set;
	}

	void Awake()
	{
		Instance = this;
	}

	public override void Init ()
	{
		base.Init ();
	}

	public void Delay(float _delayTime,LuaTable _tab,Action<LuaTable> _callback)
	{
		StartCoroutine (delay(_delayTime,_tab,_callback));
	}

	IEnumerator delay(float _delayTime,LuaTable _tab,Action<LuaTable> _callback)
	{
		yield return new WaitForSeconds (_delayTime);
		_callback (_tab);
	}
}
