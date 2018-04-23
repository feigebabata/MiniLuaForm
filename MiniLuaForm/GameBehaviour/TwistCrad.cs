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

	void Awake()
	{
		rectSize = GetComponent<RectTransform> ().sizeDelta;
		shadow_T.gameObject.SetActive (false);
		facade_T.gameObject.SetActive (false);
		Debug.LogWarning (Screen.width+" , "+Screen.height);
		Debug.LogWarning (rectSize);
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
		if(curDirection!= Direction.None && space>0.5f)
		{
			changeDirection (Direction.None);
			return;
		}
	}

	public void BeginDrag(BaseEventData _data)
	{
		PointerEventData ped = _data as PointerEventData;
		startPos = ped.pressPosition;
		Vector2 pos = ped.pressPosition - new Vector2 (Screen.width/2,Screen.height/2);

		if(pos.x>rectSize.y*0.4f)
		{
			changeDirection (Direction.Left);
		}
		else if(pos.x<-rectSize.y*0.4f)
		{
			changeDirection (Direction.Right);
		}
		else if(pos.y>rectSize.x*0.4f)
		{
			changeDirection (Direction.Down);
		}
		else if(pos.y<-rectSize.x*0.4f)
		{
			changeDirection (Direction.Up);
		}
		else
		{
			curDirection = Direction.None;
		}

		Debug.LogWarning (curDirection);
	}

	public void EndDrag(BaseEventData _data)
	{
		changeDirection (Direction.None);
	}
}
