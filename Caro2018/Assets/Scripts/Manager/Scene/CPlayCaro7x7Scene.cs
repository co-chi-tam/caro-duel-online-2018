﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CPlayCaro7x7Scene : MonoBehaviour {

	[SerializeField]	protected Text[] m_DisplayPlayers;
	[SerializeField]	protected GameObject[] m_CurrentTurnObjs;

	protected CPlayer m_Player;
	protected CGameManager m_GameManager;

	protected virtual void Start() {
		this.m_Player = CPlayer.GetInstance();
		this.m_GameManager = CGameManager.GetInstance();
		this.SetupPlayers();
	}

	protected virtual void LateUpdate() {
		var turnIndex = this.m_GameManager.turnIndex;
		this.m_CurrentTurnObjs[0].SetActive (!turnIndex);
		this.m_CurrentTurnObjs[1].SetActive (turnIndex);
	}

	protected virtual void SetupPlayers() {
		var maximumPlayer = this.m_Player.room.roomPlayes.Length > 2 ? 2 : this.m_Player.room.roomPlayes.Length;
		for (int i = 0; i < maximumPlayer; i++) {
			this.m_DisplayPlayers[i].text = this.m_Player.room.roomPlayes[i].name;
		}
	}

}