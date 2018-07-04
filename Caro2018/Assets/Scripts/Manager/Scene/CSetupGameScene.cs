using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSetupGameScene : MonoBehaviour {

	protected CPlayer m_Player;

	protected virtual void Start() {
		this.m_Player = CPlayer.GetInstance ();
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
		this.m_Player.SetPlayername (displayNameInput.text);
	}

}
