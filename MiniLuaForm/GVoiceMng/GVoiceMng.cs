using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using gcloud_voice;

public class GVoiceMng :MngBase
{

	public static GVoiceMng Instance 
	{
		get;
		private set;
	}

	public bool IsCancelRecording=false;
	bool isPlaySucc=true;
	int curRecordIndex=0;
	bool isApplyMessageKey=true;
	IGCloudVoice m_voiceengine;
	string cacheDir,uploadPath,downloadPath;
	Queue<string> records = new Queue<string>();

	void Awake()
	{
		Instance = this;
		if(Application.platform== RuntimePlatform.WindowsEditor || Application.platform== RuntimePlatform.OSXEditor)
		{
			return;
		}
		cacheDir=Application.persistentDataPath+"/gvoiceCache/";
		uploadPath = Application.persistentDataPath+"/gvoiceCache/upload.dat";
		downloadPath = Application.persistentDataPath+"/gvoiceCache/download.dat";
		if(!System.IO.Directory.Exists(cacheDir))
		{
			System.IO.Directory.CreateDirectory (cacheDir);
		}
	}

	public override void Init ()
	{
		if(Application.platform== RuntimePlatform.WindowsEditor || Application.platform== RuntimePlatform.OSXEditor)
		{
			return;
		}
		m_voiceengine = GCloudVoice.GetEngine ();
		m_voiceengine.OnApplyMessageKeyComplete += onApplyMessageKeyComplete;
		m_voiceengine.OnUploadReccordFileComplete += onUploadReccordFileComplete;
		m_voiceengine.OnDownloadRecordFileComplete += onDownloadReccordFileComplete;
		m_voiceengine.OnPlayRecordFilComplete += onPlayRecordFilComplete;
		System.TimeSpan ts = System.DateTime.UtcNow - new System.DateTime(1970,1,1,0,0,0,0);
		string strTime =  System.Convert.ToInt64(ts.TotalSeconds).ToString(); 
		m_voiceengine.SetAppInfo("932849489","d94749efe9fce61333121de84123ef9b",strTime);
		m_voiceengine.Init();
		m_voiceengine.SetMode (GCloudVoiceMode.Messages);
		base.Init ();
	}

	public void ApplyMessageKey()
	{
		if(Application.platform== RuntimePlatform.WindowsEditor || Application.platform== RuntimePlatform.OSXEditor)
		{
			return;
		}
		if (isApplyMessageKey) 
		{
			m_voiceengine.ApplyMessageKey (15000);
			isApplyMessageKey = false;
		}
	}

	public void StartRecording()
	{
		if(Application.platform== RuntimePlatform.WindowsEditor || Application.platform== RuntimePlatform.OSXEditor)
		{
			return;
		}
		if(m_voiceengine!=null)
		{
			GCloudVoiceErr err = (GCloudVoiceErr)m_voiceengine.StartRecording (uploadPath);
			if(err != GCloudVoiceErr.GCLOUD_VOICE_SUCC)
			{
				Debug.LogError ("[GVoiceMng.StartRecording]:"+err);
			}
		}
	}

	public void StopRecording()
	{
		if(Application.platform== RuntimePlatform.WindowsEditor || Application.platform== RuntimePlatform.OSXEditor)
		{
			return;
		}
		if(m_voiceengine!=null)
		{
			GCloudVoiceErr err = (GCloudVoiceErr)m_voiceengine.StopRecording ();
			if(err != GCloudVoiceErr.GCLOUD_VOICE_SUCC)
			{
				Debug.LogError ("[GVoiceMng.StopRecording]StopRecording:"+err);
			}
			else
			{
				if(!IsCancelRecording)
				{
					GCloudVoiceErr uperr = (GCloudVoiceErr)m_voiceengine.UploadRecordedFile (uploadPath,60000);
					if(uperr != GCloudVoiceErr.GCLOUD_VOICE_SUCC)
					{
						Debug.LogError ("[GVoiceMng.StopRecording]UploadRecordedFile:"+uperr);
					}
				}
				else
				{
					Debug.Log ("取消上传录音");
				}
			}
		}
		IsCancelRecording = false;
	}

	public void ReceRecording(string _fileID)
	{
		if(Application.platform== RuntimePlatform.WindowsEditor || Application.platform== RuntimePlatform.OSXEditor)
		{
			return;
		}
		records.Enqueue (_fileID);
		downloadRecord ();
	}

	void downloadRecord()
	{
		if(isPlaySucc && records.Count>0)
		{
			isPlaySucc = false;
			string recordID = records.Dequeue ();
			GCloudVoiceErr err = (GCloudVoiceErr)m_voiceengine.DownloadRecordedFile (recordID,downloadPath,60000);
			if(err != GCloudVoiceErr.GCLOUD_VOICE_SUCC)
			{
				Debug.LogError ("[GVoiceMng.StopRecording]UploadRecordedFile:"+err);
				isPlaySucc = true;
				downloadRecord ();
			}
		}
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if (m_voiceengine != null) 
		{
			if (pauseStatus) 
			{
				m_voiceengine.Pause ();
			} 
			else 
			{
				m_voiceengine.Resume();
			}
		}
	}

	void Update()
	{
		if(m_voiceengine!=null)
		{
			m_voiceengine.Poll ();	
		}
	}

	void onApplyMessageKeyComplete(IGCloudVoice.GCloudVoiceCompleteCode code)
	{
		if (code == IGCloudVoice.GCloudVoiceCompleteCode.GV_ON_MESSAGE_KEY_APPLIED_SUCC) 
		{
			Debug.Log ("OnApplyMessageKeyComplete succ11");
		} 
		else 
		{
			Debug.Log ("OnApplyMessageKeyComplete error");
		}
	}

	void onUploadReccordFileComplete(IGCloudVoice.GCloudVoiceCompleteCode code,string filepath,string fileid)
	{
		if (code == IGCloudVoice.GCloudVoiceCompleteCode.GV_ON_UPLOAD_RECORD_DONE) 
		{
			Debug.Log ("OnUploadReccordFileComplete succ, filepath:" + filepath +" fileid len="+fileid.Length+ " fileid:" + fileid+" fileid len="+fileid.Length);

			XLua.LuaTable tab = AppMng.luaEnv.NewTable ();
			tab.Set<string,string> ("fileid",fileid);
			EventTool.Run ("niuniu","playroom.sendchat",tab);
		} 
		else 
		{
			Debug.Log ("OnUploadReccordFileComplete error");
		}
	}

	void onDownloadReccordFileComplete(IGCloudVoice.GCloudVoiceCompleteCode code,string filepath,string fileid)
	{
		if (code == IGCloudVoice.GCloudVoiceCompleteCode.GV_ON_DOWNLOAD_RECORD_DONE) 
		{
			Debug.Log ("OnDownloadRecordFileComplete succ, filepath:" + filepath + " fileid:" + fileid);
			GCloudVoiceErr err = (GCloudVoiceErr)m_voiceengine.PlayRecordedFile (filepath);
			if(err != GCloudVoiceErr.GCLOUD_VOICE_SUCC)
			{
				Debug.LogError ("[GVoiceMng.onDownloadReccordFileComplete]PlayRecordedFile:"+err);
				isPlaySucc = true;
				downloadRecord ();
			}
		} 
		else 
		{
			Debug.Log ("OnDownloadRecordFileComplete error");
		}
	}

	void onPlayRecordFilComplete(IGCloudVoice.GCloudVoiceCompleteCode code,string filepath)
	{
		if (code == IGCloudVoice.GCloudVoiceCompleteCode.GV_ON_PLAYFILE_DONE) 
		{
			Debug.Log ("OnPlayRecordFilComplete succ, filepath:" + filepath);
		} 
		else 
		{
			Debug.Log ("OnPlayRecordFilComplete error");
		}
		isPlaySucc = true;
		downloadRecord ();
	}
}
