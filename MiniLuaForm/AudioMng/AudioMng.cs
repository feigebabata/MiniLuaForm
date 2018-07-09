using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using XLua;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AudioMng : MngBase 
{
	[SerializeField]
	AudioSource bgAS;
	[SerializeField]
	AudioSource clickAS;

	Dictionary<string,Dictionary<string,AudioClip>> acs = new Dictionary<string, Dictionary<string, AudioClip>> ();
	List<AudioSource> once_AS_s = new List<AudioSource> ();

	public static AudioMng Instance 
	{
		get;
		private set;
	}

	float audioVal=0.5f;
	float musicVal=0.5f;

	void Awake()
	{
		Instance = this;
	}

	void Update()
	{
		if(Input.GetMouseButtonUp(0) && 
			EventSystem.current.currentSelectedGameObject!=null && 
			EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.Selectable>()!=null &&
			clickAS.clip!=null)
		{
			clickAS.Play ();
		}	
	}

	public override void Init ()
	{
		EventTool.Add ("hall","setting.audioVal","AudioMng",(_tab)=>
			{
				audioVal=_tab.Get<float>("val");
				clickAS.volume=audioVal;
			});
		EventTool.Add ("hall","setting.musicVal","AudioMng",(_tab)=>
			{
				musicVal=_tab.Get<float>("val");
				bgAS.volume=musicVal;
			});
		if(PlayerPrefs.HasKey("hall.config"))
		{
			Hashtable config = MiniJSON.jsonDecode(PlayerPrefs.GetString("hall.config")) as Hashtable;
			audioVal = float.Parse((config ["hall"] as Hashtable) ["audioVal"].ToString ());
			musicVal = float.Parse((config ["hall"] as Hashtable) ["musicVal"].ToString ());
		}
		bgAS.volume = musicVal;
		base.Init ();
	}

	public void PlayBG(string _modules,string _name)
	{
		getAC (_modules,_name,(ac)=>
			{
				bgAS.clip = ac;
				bgAS.Play();
			});
	}

	public void SetClick_AC(string _modules,string _name)
	{
		getAC (_modules,_name,(ac)=>
			{
				clickAS.clip = ac;
			});
	}

	public void PauseAllAudio()
	{
		AudioListener.pause = true;
	}

	public void UnPauseAllAudio()
	{
		AudioListener.pause = false;
	}



	public void PlayOnce(string _modules,string _name)
	{
		getAC (_modules,_name,(ac)=>
			{
				AudioSource temp_as = once_AS_s.Find((_as)=>{return !_as.isPlaying;});
				if(temp_as==null)
				{
					temp_as = gameObject.AddComponent<AudioSource>();
					once_AS_s.Add(temp_as);
				}
				temp_as.volume = audioVal;
				temp_as.clip = ac;
				temp_as.Play();
			});
	}

	void getAC(string _modules,string _name,Action<AudioClip> _callback)
	{
		if(!acs.ContainsKey(_modules))
		{
			acs.Add (_modules,new Dictionary<string, AudioClip>());
		}
		if(acs[_modules].ContainsKey(_name))
		{
			_callback (acs[_modules][_name]);
		}
		else
		{
			string path = "/"+_modules+"/audios.assetbundle";
			ABTool.LoadAC (path,_name,(ac)=>
				{
					acs[_modules].Add(_name,ac);
					_callback(ac);
				});
		}
	}

}
