using System;
using System.Collections;
using System.Collections.Generic;
using XLua;
using UnityEngine;

public static class EventTool
{
	struct EventData
	{
		public string ID;
		public Action<LuaTable> Callback;
	}
	static Dictionary<string,Dictionary<string,Dictionary<string,EventData>>> allEvents = new Dictionary<string, Dictionary<string,Dictionary<string,EventData>>> ();

	static public void Clear(string _modules="")
	{
		if(string.IsNullOrEmpty(_modules))
		{
			allEvents.Clear ();
		}
		else
		{
			if (allEvents.ContainsKey (_modules)) 
			{
				allEvents.Remove (_modules);
			} 
			else 
			{
				Debug.LogWarning ("[EventMng.Clear]无此事件集:"+_modules);
			}
		}
	}

	static public void Add(string _modules,string _eventName,string _eventID,Action<LuaTable> _Callback)
	{
		EventData ed;
		ed.ID = _eventID;
		ed.Callback = _Callback;
		if(!allEvents.ContainsKey(_modules))
		{
			allEvents.Add (_modules,new Dictionary<string, Dictionary<string,EventData>>());
		}
		if (!allEvents [_modules].ContainsKey (_eventName)) 
		{
			allEvents [_modules].Add (_eventName,new Dictionary<string,EventData>());
		}
		if(allEvents [_modules][_eventName].ContainsKey(_eventID))
		{
			Debug.LogWarningFormat ("[EventMng.Add]事件ID已存在:{0}.{1}.{2}",_modules,_eventName,_eventID);
		}
		else
		{
			allEvents [_modules] [_eventName].Add (_eventID,ed);
		}
	}

	static public void Run(string _modules,string _eventName,LuaTable _tab)
	{
		if(!allEvents.ContainsKey(_modules))
		{
//			Debug.LogWarning ("[EventMng.Run]无此事件集:"+_modules);
			return;
		}
		if(!allEvents[_modules].ContainsKey(_eventName) || allEvents [_modules][_eventName].Count==0)
		{
			// Debug.LogWarningFormat ("[EventMng.Run]无此事件:{0}.{1}",_modules,_eventName);
		}
		else
		{
			foreach (var item in allEvents [_modules][_eventName]) 
			{
				item.Value.Callback (_tab);	
			}
		}
	}

	static public void Remove(string _modules,string _eventName,string _eventID)
	{

		if(!allEvents.ContainsKey(_modules))
		{
			Debug.LogWarning ("[EventMng.Remove]无此事件集:"+_modules);
			return;
		}
		if(!allEvents[_modules].ContainsKey(_eventName))
		{
			Debug.LogWarningFormat ("[EventMng.Remove]无此事件:{0}.{1}",_modules,_eventName);
		}
		if(string.IsNullOrEmpty(_eventID))
		{
			allEvents [_modules].Remove (_eventName);
		}
		else
		{
			if(allEvents [_modules][_eventName].ContainsKey(_eventID))
			{
				allEvents [_modules] [_eventName].Remove (_eventID);
			}
			else
			{
				Debug.LogWarningFormat ("[EventMng.Remove]无此事件ID:{0}.{1}.{2}",_modules,_eventName,_eventID);
			}
		}
	}
}
