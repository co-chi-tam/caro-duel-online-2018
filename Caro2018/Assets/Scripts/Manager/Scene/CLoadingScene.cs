using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SocketIO;

public class CLoadingScene : MonoBehaviour {

	[SerializeField]	protected GameObject m_MessagePanel;
	[SerializeField]	protected Text m_MessageText;
	[SerializeField]	protected Button m_MessageOKButton;

	protected CPlayer m_Player;
	protected WaitForSeconds m_DelaySeconds = new WaitForSeconds(3f);
	protected float m_MaximumTimer = 30f;

	protected virtual void Start() {
		this.m_Player = CPlayer.GetInstance ();
		this.m_Player.socket.Connect();
		this.m_Player.socket.Off("welcome", this.ReceveiWelcomeMsg);
		this.m_Player.socket.On("welcome", this.ReceveiWelcomeMsg);
		this.SendRequestConnect ();
	}

	protected virtual void SendRequestConnect() {
		StartCoroutine (this.HandleSendRequestConnect());
	}

	protected IEnumerator HandleSendRequestConnect() {
		this.m_MaximumTimer = 30f;
		while (this.m_MaximumTimer >= 0f) {
			yield return this.m_DelaySeconds;
			this.m_Player.socket.Connect();
			this.m_MaximumTimer -= 3f;
		}
		this.ShowMessage ("Can not connect server. Please try again.", () => {
			this.SendRequestConnect ();
		});
	}

	protected void ReceveiWelcomeMsg(SocketIOEvent e) {
		Debug.Log("[SocketIO] Welcome received: " + e.name + " " + e.data);
		this.m_Player.SwithSceneTo ("SetupGameScene");
		StopCoroutine(this.HandleSendRequestConnect());
	}

	public virtual void ShowMessage(string text, UnityAction callback = null) {
		if (this.m_MessagePanel != null && this.m_MessageText != null) {
			this.m_MessagePanel.SetActive (true);
			this.m_MessageText.text = text;
			if (callback != null) {
				this.m_MessageOKButton.onClick.RemoveListener(callback);
				this.m_MessageOKButton.onClick.AddListener (callback);
			}
		}
	}
	
}
