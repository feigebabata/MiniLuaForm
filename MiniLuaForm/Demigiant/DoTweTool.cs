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
}
