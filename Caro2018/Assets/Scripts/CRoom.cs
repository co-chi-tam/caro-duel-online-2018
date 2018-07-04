using System;
using UnityEngine;

[Serializable]
public class CRoom {

	public string roomName;
	public CPlayer.CData[] roomPlayes;
	public string roomDisplay;

	public CRoom()
	{
		this.roomName = string.Empty;
		this.roomPlayes = new CPlayer.CData[0];
		this.roomDisplay = string.Empty;
	}
	
}
