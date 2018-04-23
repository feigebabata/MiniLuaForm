using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using XLua;
using LitJson;
using UnityEngine.Networking;

public class HttpSerMng : MngBase 
{
	public const int TimeOut = 5;

	public static HttpSerMng Instance 
	{
		get;
		private set;
	}
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

	public void ReqSerOnce(string _url,Action<string> _callback,string _json)
	{
//		gameObject.AddComponent<ReqSerOnce> ().Request (_url,_callback,_json);
		StartCoroutine (request(_url,_callback,_json));
	}

	IEnumerator request(string _url,Action<string> _callback,string _json)
	{
		WWWForm form = new WWWForm ();
		JsonData jd = JsonMapper.ToObject (_json);
		IDictionary keys = jd as IDictionary;
		foreach (string k in keys.Keys) 
		{
			form.AddField (k,jd[k].ToString());
		}
		using(UnityWebRequest uwr = UnityWebRequest.Post(_url,form))
		{
			uwr.downloadHandler = new DownloadHandlerBuffer ();
			uwr.timeout = HttpSerMng.TimeOut;
			yield return uwr.SendWebRequest ();
			if(uwr.error!=string.Empty)
			{
				if(_callback!=null)
				{
					_callback (uwr.downloadHandler.text);
					uwr.Dispose ();
				}
			}
			else
			{
//				Debug.LogErrorFormat ("[ReqSerOnce.request]请求异常:{0}",_url);
				HttpErr(_url,"[HttpSerMng.request]请求异常:"+uwr.error);
			}
		}
	}

	public void Clear(string _modules)
	{
		List<string> keys = new List<string> (reqLoops.Count);
		foreach (var item in reqLoops) 
		{
			keys.Add (item.Key);
		}
		foreach (var key in keys) 
		{
			if(_modules==string.Empty || _modules==key)
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
			Debug.LogErrorFormat ("[HttpSerMng.removeLoopReq]无此请求模块:{0}",_modules);
			return;
		}
		if(!reqLoops[_modules].ContainsKey(_code))
		{
			Debug.LogErrorFormat ("[HttpSerMng.removeLoopReq]无此请求:{0}.{1}",_modules,_code);
			return;
		}
		StopCoroutine (reqLoops[_modules][_code]);
		reqLoops [_modules].Remove (_code);
	}

	public void ReqSerLoop(string _modules,string _code,string _url,float _delay,Action<string> _callback,string _json)
	{
		if(!reqLoops.ContainsKey(_modules))
		{
			reqLoops.Add (_modules,new Dictionary<string, IEnumerator>());
		}
		if(reqLoops[_modules].ContainsKey(_code))
		{
			Debug.LogErrorFormat ("[HttpSerMng.reqSerLoop]已存在该请求:{0}.{1}",_modules,_code);
			return;
		}
		reqLoops[_modules][_code] = IReqSerLoop (_url,_delay,_callback,_json);
		StartCoroutine (reqLoops[_modules][_code]);
	}

	IEnumerator IReqSerLoop(string _url,float _delay,Action<string> _callback,string _json)
	{
		WWWForm form = new WWWForm ();
		JsonData jd = JsonMapper.ToObject (_json);
		IDictionary keys = jd as IDictionary;
		foreach (string k in keys.Keys) 
		{
			form.AddField (k,jd[k].ToString());
		}
		while (true) 
		{
			using(UnityWebRequest uwr = UnityWebRequest.Post(_url,form))
			{
				uwr.downloadHandler = new DownloadHandlerBuffer ();
				uwr.timeout = TimeOut;
				yield return uwr.SendWebRequest ();
				if(uwr.error!=string.Empty)
				{
					if(_callback!=null)
					{
						_callback (uwr.downloadHandler.text);
					}
				}
				else
				{
//					Debug.LogErrorFormat ("[HttpSerMng.IReqSerLoop]请求异常:{0}",_url);
					HttpErr(_url,"[HttpSerMng.IReqSerLoop]请求异常:"+uwr.error);
				}
				uwr.Dispose ();
			}
			yield return new WaitForSeconds (_delay);
		}
	}

	public void HttpErr(string _url,string _tip)
	{
		Debug.LogError (_tip+"\n"+_url);
	}
}
