using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cn.sharesdk.unity3d;
using System;
using XLua;
using System.Text.RegularExpressions;
using System.IO;

public class ShareMng : MngBase 
{
	public ShareSDK sdk;

	Action<LuaTable> getWeChatUserInfo;

	public static ShareMng Instance 
	{
		get;
		private set;
	}

	string screenshotPath;

	void Awake()
	{
		Instance = this;
		screenshotPath = Application.persistentDataPath + "/screenshot.png";
	}
	public override void Init ()
	{
		if(Application.platform== RuntimePlatform.WindowsEditor || Application.platform== RuntimePlatform.OSXEditor)
		{
			return;
		}
		sdk.Init ();
		sdk.shareHandler = OnShareResultHandler;
		sdk.showUserHandler = OnGetUserInfoResultHandler;
		sdk.authHandler = OnAuthHandler;
		sdk.getFriendsHandler = OnGetFriendsHandler;
		sdk.followFriendHandler = OnFollowFriendHandler;
		base.Init ();
	}

	public void GetWeChatUserInfo(Action<LuaTable> _callback)
	{
		getWeChatUserInfo = _callback;
		sdk.GetUserInfo (PlatformType.WeChat);
	}

	public void ShareWeChat(string _title,string _text,string _url,bool _isMoments)
	{
		ShareContent content = new ShareContent();
		content.SetText(_text);
		content.SetImageUrl(ABTool.ResUrl+"/thumblogo.jpg");
		content.SetTitle(_title);
		content.SetTitleUrl(_url);
		content.SetUrl(_url);
		content.SetShareType(ContentType.Webpage);
		if (_isMoments) 
		{
			sdk.ShareContent(PlatformType.WeChatMoments, content);
		}
		else 
		{
			sdk.ShareContent(PlatformType.WeChat, content);
		}
	}

	public void ScreenshotShare(bool _isMoments)
	{
		if (File.Exists (screenshotPath)) 
		{
			File.Delete (screenshotPath);
		}
		ScreenCapture.CaptureScreenshot ("screenshot.png");
		StartCoroutine (IscreenshotShare(_isMoments));
	}

	IEnumerator IscreenshotShare(bool _isMoments)
	{
		while(!File.Exists (screenshotPath))
		{
			yield return new WaitForEndOfFrame();
		}
		ShareContent content = new ShareContent();
		CutShareImage (screenshotPath);
		content.SetImagePath(screenshotPath.Replace(".png", "_min.jpg"));
		content.SetShareType(ContentType.Image);
		if (_isMoments) 
		{
			sdk.ShareContent(PlatformType.WeChatMoments, content);
		}
		else 
		{
			sdk.ShareContent(PlatformType.WeChat, content);
		}
	}

	void OnShareResultHandler (int reqID, ResponseState state, PlatformType type, Hashtable result)
	{
		if (state == ResponseState.Success)
		{
			print ("share successfully - share result :");
			print (MiniJSON.jsonEncode(result));
		}
		else if (state == ResponseState.Fail)
		{
			#if UNITY_ANDROID
			print ("fail! throwable stack = " + result["stack"] + "; error msg = " + result["msg"]);
			#elif UNITY_IPHONE
			print ("fail! error code = " + result["error_code"] + "; error msg = " + result["error_msg"]);
			#endif
		}
		else if (state == ResponseState.Cancel) 
		{
			print ("cancel !");
		}
	}

	void OnGetUserInfoResultHandler (int reqID, ResponseState state, PlatformType type, Hashtable result)
	{
		Debug.Log ("OnGetUserInfoResultHandler");
		if (state == ResponseState.Success)
		{
			print ("get user info result :");
			print (MiniJSON.jsonEncode(result));
			print ("AuthInfo:" + MiniJSON.jsonEncode (sdk.GetAuthInfo (PlatformType.WeChat)));
			print ("Get userInfo success !Platform :" + type );

			if(getWeChatUserInfo!=null)
			{
				LuaTable tab = AppMng.luaEnv.NewTable ();
				string nickname = result ["nickname"].ToString ();
				nickname = new Regex (@"([^\u4e00-\u9fa5a-zA-z0-9\s].*?)").Replace (nickname," ");
				tab.Set ("open_id",result["openid"].ToString());
				tab.Set ("user_name",nickname);
				tab.Set ("sex",result["sex"].ToString());
				tab.Set ("city",result["city"].ToString());
				tab.Set ("portrait",result["headimgurl"].ToString());
				tab.Set ("user_ip",Network.player.ipAddress);
				getWeChatUserInfo (tab);
			}
		}
		else if (state == ResponseState.Fail)
		{
			#if UNITY_ANDROID
			print ("fail! throwable stack = " + result["stack"] + "; error msg = " + result["msg"]);
			#elif UNITY_IPHONE
			print ("fail! error code = " + result["error_code"] + "; error msg = " + result["error_msg"]);
			#endif
		}
		else if (state == ResponseState.Cancel) 
		{
			print ("cancel !");
		}
	}

	void OnAuthHandler(int reqID, ResponseState state, PlatformType type, Hashtable result)
	{
		Debug.Log ("OnAuthHandler");
	}

	void OnGetFriendsHandler(int reqID, ResponseState state, PlatformType type, Hashtable result)
	{
		Debug.Log ("OnGetFriendsHandler");
	}

	void OnFollowFriendHandler(int reqID, ResponseState state, PlatformType type, Hashtable result)
	{
		Debug.Log ("OnFollowFriendHandler");
	}

	//缩小图片大小
	public void CutShareImage(string _ImgPath)
	{
		byte[] fileData = File.ReadAllBytes(_ImgPath);
		Texture2D tex = new Texture2D((int)Screen.width, (int)Screen.height, TextureFormat.RGB24, true);
		tex.LoadImage(fileData);
		float miniSize = Mathf.Max(tex.width, tex.height);
		float scale = 1200 / miniSize;
		if (scale > 1f)
			scale = 1f;
		Texture2D temp = ScaleTexture(tex, (int)(tex.width * scale), (int)(tex.height * scale));
		byte[] pngData = temp.EncodeToPNG();
		string miniImagePath = _ImgPath.Replace(".png", "_min.jpg");
		File.WriteAllBytes(miniImagePath, pngData);
		Destroy(tex);
		Destroy(temp);
	}
	public Texture2D ScaleTexture(Texture2D _Source, int _TargetWidth, int _TargetHeight)
	{
		Texture2D result = new Texture2D(_TargetWidth, _TargetHeight, _Source.format, true);
		Color[] rpixels = result.GetPixels(0);
		float incX = ((float)1 / _Source.width) * ((float)_Source.width / _TargetWidth);
		float incY = ((float)1 / _Source.height) * ((float)_Source.height / _TargetHeight);
		for (int px = 0; px < rpixels.Length; px++)
		{
			rpixels[px] = _Source.GetPixelBilinear(incX * ((float)px % _TargetWidth), incY * ((float)Mathf.Floor(px / _TargetWidth)));
		}
		result.SetPixels(rpixels, 0);
		result.Apply();
		return result;
	}
}
