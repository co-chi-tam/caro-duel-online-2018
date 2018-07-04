using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CDisplayRoomScene : MonoBehaviour {

	[SerializeField]	protected Text[] m_DisplayRooms;
	protected CPlayer m_Player;

	protected virtual void Start() {
		this.m_Player = CPlayer.GetInstance ();
		this.RefreshRoomsStatus();
	}

	protected virtual void UpdateRoomUI() {
		var rooms = this.m_Player.rooms;
		for (int i = 0; i < rooms.Length; i++)
		{
			var tmpRoom = rooms[i];
			this.m_DisplayRooms[i].text = tmpRoom.roomDisplay;
		}
	}

	public virtual void RefreshRoomsStatus() {
		this.m_Player.GetRoomsStatus(this.UpdateRoomUI);
	}

	public virtual void JoinRoom(string name) {
		this.m_Player.JoinRoom(name);
	}

	public virtual void SubmitJoinOrCreateRoom() {;
		this.m_Player.JoinOrCreateRoom();
	}

}
