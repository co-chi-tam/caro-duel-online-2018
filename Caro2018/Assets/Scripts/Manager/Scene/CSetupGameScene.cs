using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSetupGameScene : MonoBehaviour {

	[SerializeField]	protected InputField m_DisplayName;

	protected const string PLAYER_NAME = "PLAYER_NAME";

	protected CPlayer m_Player;
	public static bool ALREADY_SETUP = false;

	protected virtual void Start() {
		this.m_Player = CPlayer.GetInstance ();
		this.m_Player.CancelUI();
		var savedPlayerName = PlayerPrefs.GetString(PLAYER_NAME, string.Empty);
		// SAVE NAME
		this.m_DisplayName.text = savedPlayerName;
		// ALREADY_SETUP
		if (ALREADY_SETUP && string.IsNullOrEmpty (savedPlayerName) == false) {
			this.SubmitDisplayName (savedPlayerName); 
		}
		// AD
		CAdmobManager.ShowHideBanner(true);
	}

	public virtual void SubmitDisplayName(InputField displayNameInput) {
		if (displayNameInput == null) {
			return;
		}
		var playerName = displayNameInput.text;
		this.SubmitDisplayName (playerName);
	}

	public virtual void SubmitDisplayName(string playerName) {
		if (string.IsNullOrEmpty (playerName)) {
			this.m_Player.ShowMessage("User name must not empty.");
			return;
		}
		if (playerName.Length < 5) {
			this.m_Player.ShowMessage("User name must greater 5 character.");
			return;
		}
		this.m_Player.SetPlayername (playerName);
		PlayerPrefs.SetString(PLAYER_NAME, playerName);
	}

}
