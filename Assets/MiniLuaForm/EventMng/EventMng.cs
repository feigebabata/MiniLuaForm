using System;
using System.Collections;
using System.Collections.Generic;
using XLua;
using UnityEngine;

public static class EventMng
{
	struct EventData
	{
		public string ID;
		public Action<LuaTable> Callback;
	}
	static Dictionary<string,Dictionary<string,Dictionary<string,EventData>>> allEvents = new Dictionary<string, Dictionary<string,Dictionary<string,EventData>>> ();

	static public void Clear(string _events)
	{
		if(_events==string.Empty)
		{
			allEvents.Clear ();
		}
		else
		{
			if (allEvents.ContainsKey (_events)) 
			{
				allEvents.Remove (_events);
			} 
			else 
			{
				Debug.LogWarning ("[EventMng.Clear]无此事件集:"+_events);
			}
		}
	}

	static public void Add(string _events,string _eventName,string _eventID,Action<LuaTable> _Callback)
	{
		EventData ed;
		ed.ID = _eventID;
		ed.Callback = _Callback;
		if(!allEvents.ContainsKey(_events))
		{
			allEvents.Add (_events,new Dictionary<string, Dictionary<string,EventData>>());
		}
		if (!allEvents [_events].ContainsKey (_eventName)) 
		{
			allEvents [_events].Add (_eventName,new Dictionary<string,EventData>());
		}
		if(allEvents [_events][_eventName].ContainsKey(_eventID))
		{
			Debug.LogWarningFormat ("[EventMng.Add]事件ID已存在:{0}.{1}.{2}",_events,_eventName,_eventID);
		}
		else
		{
			allEvents [_events] [_eventName].Add (_eventID,ed);
		}
	}

	static public void Run(string _events,string _eventName,LuaTable _tab)
	{
		if(!allEvents.ContainsKey(_events))
		{
			Debug.LogWarning ("[EventMng.Run]无此事件集:"+_events);
			return;
		}
		if(!allEvents[_events].ContainsKey(_eventName) || allEvents [_events][_eventName].Count==0)
		{
			Debug.LogWarningFormat ("[EventMng.Run]无此事件:{0}.{1}",_events,_eventName);
		}
		else
		{
			foreach (var item in allEvents [_events][_eventName]) 
			{
				item.Value.Callback (_tab);	
			}
		}
	}

	static public void Remove(string _events,string _eventName,string _eventID)
	{

		if(!allEvents.ContainsKey(_events))
		{
			Debug.LogWarning ("[EventMng.Remove]无此事件集:"+_events);
			return;
		}
		if(!allEvents[_events].ContainsKey(_eventName))
		{
			Debug.LogWarningFormat ("[EventMng.Remove]无此事件:{0}.{1}",_events,_eventName);
		}
		if(_eventID==string.Empty)
		{
			allEvents [_events].Remove (_eventName);
		}
		else
		{
			if(allEvents [_events][_eventName].ContainsKey(_eventID))
			{
				allEvents [_events] [_eventName].Remove (_eventID);
			}
			else
			{
				Debug.LogWarningFormat ("[EventMng.Remove]无此事件ID:{0}.{1}.{2}",_events,_eventName,_eventID);
			}
		}
	}
}
