using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBase : MonoBehaviour
{
	public float AniTime;
	public PanelAniData[] EnterAnis;
	public PanelAniData[] ExitAnis;
	float aniStartTime;
	public void Open()
	{
		if(AniTime!=0 && EnterAnis!=null && EnterAnis.Length!=0)
		{
			StopAllCoroutines ();
			StartCoroutine (playOpenAni());
		}
		else
		{
			gameObject.SetActive (true);
		}
	}

	IEnumerator playOpenAni()
	{
		aniStartTime = Time.time;
		while(Time.time-aniStartTime<AniTime)
		{
			foreach (var ani in EnterAnis) 
			{
				switch (ani.AniType) 
				{
				case PanelAni.Scale:
					transform.localScale = Vector3.one * ani.AniCurve.Evaluate ((Time.time-aniStartTime)/AniTime);
					break;
				case PanelAni.MoveLeft:
					transform.localPosition = Vector3.right * ani.AniCurve.Evaluate ((Time.time-aniStartTime)/AniTime) * Screen.width;
					break;
				case PanelAni.MoveRight:
					transform.localPosition = Vector3.left * ani.AniCurve.Evaluate ((Time.time-aniStartTime)/AniTime) * Screen.width;
					break;
				case PanelAni.MoveUp:
					transform.localPosition = Vector3.down * ani.AniCurve.Evaluate ((Time.time-aniStartTime)/AniTime) * Screen.height;
					break;
				case PanelAni.MoveDown:
					transform.localPosition = Vector3.up * ani.AniCurve.Evaluate ((Time.time-aniStartTime)/AniTime) * Screen.height;
					break;
				}
				if(!gameObject.activeSelf)
				{
					gameObject.SetActive (true);
				}
				yield return new WaitForSeconds (Time.deltaTime);
			}
		}
		transform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
	}

	IEnumerator playCloseAni()
	{
		aniStartTime = Time.time;
		while(Time.time-aniStartTime<AniTime)
		{
			foreach (var ani in ExitAnis) 
			{
				switch (ani.AniType) 
				{
				case PanelAni.Scale:
					transform.localScale = Vector3.one * ani.AniCurve.Evaluate ((Time.time-aniStartTime)/AniTime);
					break;
				case PanelAni.MoveLeft:
					transform.localPosition = Vector3.left * ani.AniCurve.Evaluate ((Time.time-aniStartTime)/AniTime) * Screen.width;
					break;
				case PanelAni.MoveRight:
					transform.localPosition = Vector3.right * ani.AniCurve.Evaluate ((Time.time-aniStartTime)/AniTime) * Screen.width;
					break;
				case PanelAni.MoveUp:
					transform.localPosition = Vector3.up * ani.AniCurve.Evaluate ((Time.time-aniStartTime)/AniTime) * Screen.height;
					break;
				case PanelAni.MoveDown:
					transform.localPosition = Vector3.down * ani.AniCurve.Evaluate ((Time.time-aniStartTime)/AniTime) * Screen.height;
					break;
				}
				yield return new WaitForSeconds (Time.deltaTime);
			}
		}
		if(gameObject.activeSelf)
		{
			gameObject.SetActive (false);
		}
	}

	public void Close()
	{
		if(AniTime!=0 && ExitAnis!=null && ExitAnis.Length!=0)
		{
			StopAllCoroutines ();
			StartCoroutine (playCloseAni());
		}
		else
		{
			gameObject.SetActive (false);
		}
	}
}

[System.Serializable]
public struct PanelAniData
{
	public PanelAni AniType;
	public AnimationCurve AniCurve;
}

public enum PanelAni
{
	Scale,
	MoveLeft,
	MoveRight,
	MoveUp,
	MoveDown
}
