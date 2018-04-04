using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;

public class FGBBT : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
//		IDictionary dict = new IDictionary ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	IEnumerator req1()
	{
		string url = @"http://zhan.php0.jxcraft.net/php/home/login.php?path=login";

//		Dictionary<string,string> updata = new Dictionary<string, string> ();
//		updata.Add ("telephone","18892084672");
//		updata.Add ("password","000000");
//		string json = JsonMapper.ToJson (updata);
		WWWForm updata = new WWWForm ();
		updata.AddField ("telephone","18892084672");
		updata.AddField ("password","000000");
		UnityWebRequest uwr = UnityWebRequest.Post (url,updata);
//		Debug.Log (json);
//		byte[] bytes = System.Text.Encoding.UTF8.GetBytes (json);
//		uwr.uploadHandler = new UploadHandlerRaw (bytes);
//		uwr.SetRequestHeader("telephone","18892084672");
//		uwr.SetRequestHeader("password","000000");
		uwr.downloadHandler = new DownloadHandlerBuffer ();
		yield return uwr.Send();
		if(string.IsNullOrEmpty(uwr.error))
		{
			Debug.Log (uwr.downloadHandler.text);
		}
		else
		{

			Debug.Log (uwr.error);
		}
	}

	IEnumerator req2()
	{
		string url = @"http://zhan.php0.jxcraft.net/php/home/login.php?path=login";
		WWWForm updata = new WWWForm ();
		updata.AddField ("telephone","18892084672");
		updata.AddField ("password","000000");
		WWW uwr = new WWW (url,updata);
		yield return uwr;
		if(string.IsNullOrEmpty(uwr.error))
		{
			Debug.Log (uwr.text);
		}
		else
		{

			Debug.Log (uwr.error);
		}
	}
}
