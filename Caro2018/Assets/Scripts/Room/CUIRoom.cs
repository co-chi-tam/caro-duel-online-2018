using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CUIRoom : MonoBehaviour {

	[SerializeField]	protected string m_RoomName = string.Empty;
	public string roomName { 
		get { return this.m_RoomName; } 
	}
	[SerializeField]	protected Button m_RoomButton;
	[SerializeField]	protected Sprite[] m_BackgroundSprites;
	[SerializeField]	protected Image m_RoomBackground;
	[SerializeField]	protected Text m_RoomDisplay;

	public virtual void SetRoom (int i, string name, string display, UnityAction callback = null) {
		this.m_RoomName = name;
		this.m_RoomDisplay.text = display;
		if (callback != null) {
			this.m_RoomButton.onClick.RemoveListener (callback);	
			this.m_RoomButton.onClick.AddListener (callback);
		}
		this.m_RoomBackground.sprite = this.m_BackgroundSprites[i % this.m_BackgroundSprites.Length];
	}
	
}
