using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using XLua;

public class DoTweTool  
{
	public static void LocalMove(Transform _t,Vector3 _endPos,float _time,Action _callback)
	{
		_t.DOLocalMove (_endPos, _time).OnComplete (()=>
			{
				if(_callback!=null)
				{
					_callback();
				}
			});
	}

	public static void Move(Transform _t,Vector3 _endPos,float _time,Action _callback)
	{
		_t.DOMove (_endPos, _time).OnComplete (()=>
			{
				if(_callback!=null)
				{
					_callback();
				}
			});
	}

	public static void LocalRotate(Transform _t,Vector3 _v3,float _time,Action _callback)
	{
		_t.DOLocalRotate (_v3, _time).OnComplete (()=>
			{
				if(_callback!=null)
				{
					_callback();
				}
			});
	}

	public static void Scale(Transform _t,Vector3 _v3,float _time,Action _callback)
	{
		Debug.Log (_time);
		_t.DOScale (_v3, _time).OnComplete (()=>
			{
				if(_callback!=null)
				{
					_callback();
				}
			});
	}
}
