using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using XLua;
using UnityEngine.Networking;

public class HttpSerMng : MngBase 
{
	public const int TimeOut = 10;

	public static HttpSerMng Instance 
	{
		get;
		private set;
	}

	const byte CAN_ERR_COUNT = 3;

	Dictionary<string,Dictionary<string,IEnumerator>> reqLoops = new Dictionary<string,Dictionary<string,IEnumerator>>();

	/// <summary>
	/// 是否有网络
	/// </summary>
	/// <returns><c>true</c> if is net; otherwise, <c>false</c>.</returns>
	public static bool IsNet()
	{
		return Application.internetReachability != NetworkReachability.NotReachable;
	}

	void Awake()
	{
		Instance = this;
	}


	public override void Init ()
	{
		base.Init ();
	}

	public void ReqSerOnce(string _url,string _json,Action<string> _callback)
	{
		StartCoroutine (request(_url,_callback,_json));
	}

	IEnumerator request(string _url,Action<string> _callback,string _json)
	{
		WWWForm form = new WWWForm ();
		Hashtable keys = MiniJSON.jsonDecode (_json) as Hashtable;
		foreach (string k in keys.Keys) 
		{
			form.AddField (k,keys[k].ToString());
		}
		byte errCount=0;
		while(true)
		{
			using(UnityWebRequest uwr = UnityWebRequest.Post(_url,form))
			{
				uwr.downloadHandler = new DownloadHandlerBuffer ();
				uwr.timeout = HttpSerMng.TimeOut;
				yield return uwr.SendWebRequest ();
				if(string.IsNullOrEmpty(uwr.error))
				{
					if(_callback!=null)
					{
						Hashtable result = null;
						try
						{
							result = MiniJSON.jsonDecode(uwr.downloadHandler.text) as Hashtable;
							MiniJSON.jsonEncode(result);
						}
						catch 
						{
							HttpErr (_url,"[HttpSerMng.request]Json解析异常："+uwr.downloadHandler.text);
						}

						if(int.Parse(result["code"].ToString())==-2)
						{
							AppMng.Instance.Restart ();
							LuaTable tab = AppMng.luaEnv.NewTable ();
							tab.Set ("tip",result["msg"].ToString());
							Action yes = () => 
							{
								PanelTool.ClosePanel("hall","systempop");
							};
							tab.Set("yes_cb",yes);
							PanelTool.OpenPanel ("hall","systempop",1,tab);
						}
						else
						{
							_callback (uwr.downloadHandler.text);
						}
						uwr.Dispose ();
						break;
					}
					else
					{
						break;
					}
				}
				else
				{
					errCount++;
					if (errCount > CAN_ERR_COUNT) 
					{
						HttpErr(_url,"[HttpSerMng.request]请求异常:"+uwr.error);
						break;
					}
				}
			}
		}

	}

	public void Clear(string _modules="")
	{
		if(string.IsNullOrEmpty(_modules))
		{
			StopAllCoroutines ();
			reqLoops.Clear ();
			return;
		}
		List<string> keys = new List<string> (reqLoops.Count);
		foreach (var item in reqLoops) 
		{
			keys.Add (item.Key);
		}
		foreach (var key in keys) 
		{
			if(_modules==key)
			{
				foreach (var k in reqLoops[key]) 
				{
					StopCoroutine (k.Value);
				}
				reqLoops [key].Clear ();
			}
		}
	}

	public void StopLoopReq(string _modules,string _code)
	{
		if(!reqLoops.ContainsKey(_modules))
		{
			Debug.LogWarningFormat ("[HttpSerMng.removeLoopReq]无此请求模块:{0}",_modules);
			return;
		}
		if(!reqLoops[_modules].ContainsKey(_code))
		{
			Debug.LogWarningFormat ("[HttpSerMng.removeLoopReq]无此请求:{0}.{1}",_modules,_code);
			return;
		}
		StopCoroutine (reqLoops[_modules][_code]);
		reqLoops [_modules].Remove (_code);
	}

	public void ReqSerLoop(string _modules,string _code,string _url,float _delay,string _json,Action<string> _callback)
	{
		if(!reqLoops.ContainsKey(_modules))
		{
			reqLoops.Add (_modules,new Dictionary<string, IEnumerator>());
		}
		if(reqLoops[_modules].ContainsKey(_code))
		{
			Debug.LogWarningFormat ("[HttpSerMng.reqSerLoop]已存在该请求:{0}.{1}",_modules,_code);
			return;
		}
		reqLoops[_modules][_code] = IReqSerLoop (_url,_delay,_callback,_json);
		StartCoroutine (reqLoops[_modules][_code]);
	}

	IEnumerator IReqSerLoop(string _url,float _delay,Action<string> _callback,string _json)
	{
		WWWForm form = new WWWForm ();
		Hashtable keys = MiniJSON.jsonDecode(_json) as Hashtable;
		foreach (string k in keys.Keys) 
		{
			form.AddField (k,keys[k].ToString());
		}
		Hashtable result;
		byte errCount = 0;
		while (true) 
		{
			using(UnityWebRequest uwr = UnityWebRequest.Post(_url,form))
			{
				uwr.downloadHandler = new DownloadHandlerBuffer ();
				uwr.timeout = TimeOut;
				yield return uwr.SendWebRequest ();
				if(string.IsNullOrEmpty(uwr.error))
				{
					if(_callback!=null)
					{
						try
						{
							result = MiniJSON.jsonDecode(uwr.downloadHandler.text) as Hashtable;
							MiniJSON.jsonEncode(result);
						}
						catch 
						{
							HttpErr (_url,"[HttpSerMng.IReqSerLoop]Json解析异常："+uwr.downloadHandler.text);
							continue;
						}
						if(int.Parse(result["code"].ToString())==-2)
						{
							AppMng.Instance.Restart ();
							LuaTable tab = AppMng.luaEnv.NewTable ();
							tab.Set ("tip",result["msg"].ToString());
							Action yes = () => 
							{
								PanelTool.ClosePanel("hall","systempop");
							};
							tab.Set("yes_cb",yes);
							PanelTool.OpenPanel ("hall","systempop",1,tab);
						}
						else
						{
							_callback (uwr.downloadHandler.text);
						}
						errCount = 0;
					}
				}
				else
				{
					errCount++;
					if(errCount>CAN_ERR_COUNT)
					{
						HttpErr(_url,"[HttpSerMng.IReqSerLoop]请求异常:"+uwr.error);
					}
				}
				uwr.Dispose ();
			}
			if(errCount==0)
			{
				yield return new WaitForSeconds (_delay);
			}
		}
	}

	public void HttpErr(string _url,string _tip)
	{
		Debug.LogError (_tip+"\n"+_url);
		AppMng.Instance.Restart ();
		LuaTable tab = AppMng.luaEnv.NewTable ();
		tab.Set ("tip",_tip);
		Action yes = () => 
		{
			PanelTool.ClosePanel("hall","systempop");
		};
		tab.Set("yes_cb",yes);
		PanelTool.OpenPanel ("hall","systempop",1,tab);
	}
}
