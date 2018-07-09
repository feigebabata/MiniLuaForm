using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class TwistCrad : MonoBehaviour 
{
	public enum Direction
	{
		None,Up,Down,Left,Right
	}
	[SerializeField]
	Transform shadow_T;
	[SerializeField]
	Transform bgMask_T;
	[SerializeField]
	Transform facade_T;
	Direction curDirection ;
	Vector2 rectSize;
	bool isDown;
	Vector3 startPos = Vector3.zero;

	bool isshow=true;
	bool ispublic;
	bool iszhuang;

	public Action m_End;
	public Action<string> m_Changed;

	void Awake()
	{
		rectSize = GetComponent<RectTransform> ().sizeDelta;
		shadow_T.gameObject.SetActive (false);
		facade_T.gameObject.SetActive (false);
	}

	void changeDirection(Direction _direction)
	{
		if(curDirection==_direction)
		{
			return;
		}
		curDirection = _direction;
		StopAllCoroutines ();
		switch (_direction) 
		{
		case Direction.Up:
			shadow_T.localEulerAngles = Vector3.zero;
			facade_T.localEulerAngles = Vector3.zero;
			break;
		case Direction.Down:
			shadow_T.localEulerAngles = Vector3.forward*180;
			facade_T.localEulerAngles = Vector3.zero;
			break;
		case Direction.Left:
			shadow_T.localEulerAngles = Vector3.forward * -90;
			facade_T.localEulerAngles = Vector3.forward * 180;
			break;
		case Direction.Right:
			shadow_T.localEulerAngles = Vector3.forward * 90;
			facade_T.localEulerAngles = Vector3.forward * 180;
			break;
		}
		shadow_T.gameObject.SetActive (curDirection != Direction.None);
		facade_T.gameObject.SetActive (curDirection != Direction.None);
	}

	void RightTwist(float _rate)
	{
		facade_T.localPosition = Vector3.down*rectSize.y + Vector3.up*_rate*2*rectSize.y;
		bgMask_T.localPosition = Vector3.up * _rate*rectSize.y;
		shadow_T.localPosition = Vector3.down*(1-_rate)*rectSize.y ;
	}

	void LeftTwist(float _rate)
	{
		facade_T.localPosition = Vector3.up*rectSize.y - Vector3.up*_rate*2*rectSize.y;
		bgMask_T.localPosition = Vector3.down * _rate*rectSize.y;
		shadow_T.localPosition = Vector3.up*(1-_rate)*rectSize.y ;
	}
		
	void DownTwist(float _rate)
	{
		facade_T.localPosition = Vector3.left*rectSize.x + Vector3.right*_rate*2*rectSize.x;
		bgMask_T.localPosition = Vector3.left * _rate*rectSize.x;
		shadow_T.localPosition = Vector3.right*(1-_rate)*rectSize.x ;
	}

	void UpTwist(float _rate)
	{
		facade_T.localPosition = Vector3.right*rectSize.x - Vector3.right*_rate*2*rectSize.x;
		bgMask_T.localPosition = Vector3.right * _rate*rectSize.x;
		shadow_T.localPosition = Vector3.left*(1-_rate)*rectSize.x ;
	}

	public void Drag(BaseEventData _data)
	{
		if(ispublic && !iszhuang)
		{
			return;
		}
		if (!isshow) 
		{
			return;
		}
		PointerEventData ped = _data as PointerEventData;
		float space = 0;
		switch (curDirection) 
		{
		case Direction.Left:
			{
				space = startPos.x - ped.position.x;
				if (space < 0) 
				{
					space = 0;
				}
				if (space > rectSize.y) 
				{
					space = rectSize.y;
				}
				space = space / rectSize.y;
				LeftTwist (space);
			}
			break;
		case Direction.Right:
			{
				space = ped.position.x-startPos.x;
				if (space < 0) 
				{
					space = 0;
				}
				if (space > rectSize.y) 
				{
					space = rectSize.y;
				}
				space = space / rectSize.y;
				RightTwist (space);
			}
			break;
		case Direction.Up:
			{
				space = ped.position.y-startPos.y;
				if (space < 0) 
				{
					space = 0;
				}
				if (space > rectSize.x) 
				{
					space = rectSize.x;
				}
				space = space / rectSize.x;
				UpTwist (space);
			}
			break;
		case Direction.Down:
			{
				space = startPos.y-ped.position.y;
				if (space < 0) 
				{
					space = 0;
				}
				if (space > rectSize.x) 
				{
					space = rectSize.x;
				}
				space = space / rectSize.x;
				DownTwist (space);
			}
			break;
		}
		if(curDirection != Direction.None && m_Changed!=null)
		{
			Hashtable jd = new Hashtable();
			jd ["dir"] = (int)curDirection;
			jd ["val"] = space;
			m_Changed (MiniJSON.jsonEncode(jd));
		}
		if(curDirection!= Direction.None && space>0.5f)
		{
			isshow = false;
			showpoker ();
			return;
		}
	}

	void showpoker()
	{
		facade_T.localPosition = Vector3.zero;
		bgMask_T.gameObject.SetActive (false);
		shadow_T.gameObject.SetActive (false);
		if (m_End != null) 
		{
			m_End ();
		}
	}

	public void BeginDrag(BaseEventData _data)
	{
		if(ispublic && !iszhuang)
		{
			return;
		}
		if (!isshow) 
		{
			return;
		}
		PointerEventData ped = _data as PointerEventData;
		startPos = ped.pressPosition;
		Vector2 pos = ped.pressPosition - new Vector2 (Screen.width/2,Screen.height/2);
		pos *= 1920f / Screen.width;

		if(pos.x>rectSize.y*0.25f)
		{
			changeDirection (Direction.Left);
		}
		else if(pos.x<-rectSize.y*0.25f)
		{
			changeDirection (Direction.Right);
		}
		else if(pos.y>rectSize.x*0.25f)
		{
			curDirection = Direction.None;
		}
		else if(pos.y<-rectSize.x*0.25f)
		{
			changeDirection (Direction.Up);
		}
		else
		{
			curDirection = Direction.None;
		}

	}

	public void EndDrag(BaseEventData _data)
	{
		if(ispublic && !iszhuang)
		{
			return;
		}
		if (!isshow) 
		{
			return;
		}
		changeDirection (Direction.None);
	}

	public void SetPoker(Sprite _pk,object _public,object _zhuang)
	{
		facade_T.GetComponent<Image> ().sprite = _pk;
		isshow = true;
		ispublic = _public != null;
		iszhuang = _zhuang != null;
		changeDirection (Direction.None);
		bgMask_T.gameObject.SetActive (true);
		shadow_T.gameObject.SetActive (true);
	}

	public void SetChanged(string _json)
	{
		Hashtable jd = MiniJSON.jsonDecode(_json) as Hashtable;
		Direction dir = (Direction)int.Parse (jd ["dir"].ToString ());
		float val = float.Parse (jd ["val"].ToString ());
		changeDirection (dir);
		switch (dir) 
		{
		case Direction.Left:
			LeftTwist (val);
			break;
		case Direction.Right:
			RightTwist (val);
			break;
		case Direction.Up:
			UpTwist (val);
			break;
		case Direction.Down:
			DownTwist (val);
			break;
		}
		if( val>0.5f)
		{
			isshow = false;
			showpoker ();
			return;
		}
	}
}
