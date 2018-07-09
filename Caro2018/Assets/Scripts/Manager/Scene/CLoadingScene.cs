using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SocketIO;

public class CLoadingScene : MonoBehaviour {

	protected CPlayer m_Player;
	protected WaitForSeconds m_DelaySeconds = new WaitForSeconds(3f);
	protected float m_MaximumTimer = 30f;

	protected virtual void Start() {
		this.m_Player = CPlayer.GetInstance ();
		this.m_Player.socket.Connect();
		this.m_Player.DisplayLoading (true);
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
		this.m_Player.ShowMessage ("Can not connect server. Please try again.", () => {
			this.SendRequestConnect ();
		});
	}

	protected void ReceveiWelcomeMsg(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Welcome received: " + e.name + " " + e.data);
		#endif
		this.m_Player.SwithSceneTo ("SetupGameScene");
		this.m_Player.DisplayLoading (false);
		StopCoroutine(this.HandleSendRequestConnect());
	}
	
}
