using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSetupGameScene : MonoBehaviour {

	[SerializeField]	protected InputField m_DisplayName;

	protected const string PLAYER_NAME = "PLAYER_NAME";
	protected CPlayer m_Player;

	protected virtual void Start() {
		this.m_Player = CPlayer.GetInstance ();
		this.m_Player.CancelUI();
		// SAVE NAME
		this.m_DisplayName.text = PlayerPrefs.GetString(PLAYER_NAME, string.Empty);
	}

	public virtual void SubmitDisplayName(InputField displayNameInput) {;
		if (string.IsNullOrEmpty (displayNameInput.text)) {
			this.m_Player.ShowMessage("User name must not empty.");
			return;
		}
		if (displayNameInput.text.Length < 5) {
			this.m_Player.ShowMessage("User name must greater 5 character.");
			return;
		}
		var playerName = displayNameInput.text;
		this.m_Player.SetPlayername (playerName);
		PlayerPrefs.SetString(PLAYER_NAME, playerName);
	}

}
