using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CUIPlayerInRoom : MonoBehaviour {

	[SerializeField]	protected Text m_PlayerName;
	[SerializeField]	protected GameObject m_PlayerInTurn;

	public virtual void SetInTurnActive(bool value) {
		this.m_PlayerInTurn.SetActive (value);
	}

	public virtual void SetPlayerName(string name) {
		this.m_PlayerName.text = name;
	}

}
