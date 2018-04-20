using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioMng : MngBase 
{
	[SerializeField]
	AudioSource bgAS;

	Dictionary<string,Dictionary<string,AudioClip>> acs = new Dictionary<string, Dictionary<string, AudioClip>> ();
	List<AudioSource> once_AS_s = new List<AudioSource> ();

	public static AudioMng Instance 
	{
		get;
		private set;
	}

	void Awake()
	{
		Instance = this;
	}

	public override void Init ()
	{
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
