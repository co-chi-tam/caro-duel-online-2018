using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SocketIO;
using SimpleSingleton;

public class CPlayer : CMonoSingleton<CPlayer> {

	#region Fields

	[SerializeField]	protected SocketIOComponent m_Socket;
	public SocketIOComponent socket { 
		get { return this.m_Socket; } 
		set { this.m_Socket = value; }
	}

	[SerializeField]	protected CPlayerData m_Data;
	public CPlayerData playerData { 
		 get { return this.m_Data; }
		 set { this.m_Data = value; }
	}

	[SerializeField]	protected CRoom m_Room;
	public CRoom room { 
		 get { return this.m_Room; }
		 set { this.m_Room = value; }
	}

	[SerializeField]	protected CSwitchScene m_SwitchScene;
	[Header("UI")]
	[SerializeField]	protected GameObject m_LoadingPanel;
	[SerializeField]	protected GameObject m_MessagePanel;
	[SerializeField]	protected Text m_MessageText;
	[SerializeField]	protected Button m_MessageOKButton;

	protected Dictionary<string, Action> m_SimpleEvent;

	protected CRoom[] m_Rooms = new CRoom[0];
	public CRoom[] rooms { 
		get { return this.m_Rooms; }
		set { this.m_Rooms = value; }
	}

	// Delay 3 second
	protected WaitForSeconds m_DelaySeconds = new WaitForSeconds(3f);

	#endregion

	#region Implementation MonoBehaviour

	protected override void Awake()
	{
		base.Awake();
		DontDestroyOnLoad(this.gameObject);
		this.m_SimpleEvent = new Dictionary<string, Action>();
	}

	protected virtual void Start()
	{
		// TEST
		socket.On("open", ReceiveOpenMsg);
		socket.On("boop", ReceiveBoop);
		socket.On("error", ReceiveErrorMsg);
		socket.On("msgError", ReceiveErrorMsg);
		socket.On("close", ReceiveCloseMsg);
		socket.On("disconnect", ReceiveCloseMsg);
		// ROOM
		socket.On("newJoinRoom", this.JoinRoomCompleted);
		socket.On("joinRoomFailed", this.JoinRoomFailed);
		socket.On("newLeaveRoom", this.LeaveRoomCompleted);
		socket.On("updateRoomStatus", this.UpdateRoomStatus);
		socket.On("clearRoom", this.ReceiveClearRoom);
		socket.On("msgChatRoom", this.ReceiveRoomChat);
		socket.On("msgWorldChat", this.ReceiveWorldChat);
		socket.On("playerNameSet", this.ReceivePlayerName);
		socket.On("turnIndexSet", this.ReceiveTurnIndex);
		socket.On("receiveChessPosition", this.ReceiveChessPosition);
		socket.On("receiveChessFail", this.ReceiveChessFail);
		// Test
		StartCoroutine("BeepBoop");
	}

	protected virtual void Update() {
		if (Input.GetKeyDown(KeyCode.Home) 
			|| Input.GetKeyDown(KeyCode.Escape)
			|| Input.GetKeyDown(KeyCode.Menu)) {
			this.Disconnect();
			this.SwithSceneTo("LoadingScene");
		}
	}

	protected virtual void OnApplicationQuit()
	{
		this.Disconnect();
	}

	// protected virtual void OnApplicationFocus(bool focusStatus)
	// {
// #if UNITY_ANDROID || UNITY_IOS 
// 		if (focusStatus == false) {
// 			this.Disconnect();
// 			this.SwithSceneTo("LoadingScene");
// 		}
// #endif
	// }

	// protected virtual void OnApplicationPause(bool pauseStatus)
	// {
// #if UNITY_ANDROID || UNITY_IOS 
// 		if (pauseStatus == true) {
// 			this.Disconnect();
// 			this.SwithSceneTo("LoadingScene");
// 		}
// #endif
	// }

	#endregion

	#region Main methods

	public virtual void Connect() {
		if (this.m_Socket != null) {
			this.m_Socket.Connect();
		}
	}

	public virtual void Disconnect() {
		if (this.m_Socket != null) {
			this.m_Socket.Close();
		}
	}

	private IEnumerator BeepBoop()
	{
		while (true) {
			// wait 3 seconds and continue
			yield return this.m_DelaySeconds;
			this.Connect();
			this.m_Socket.Emit("beep");
		}
	}

	public virtual void AddListener(string name, Action eventCallback) {
		if (this.m_SimpleEvent.ContainsKey(name))
			return;
		this.m_SimpleEvent.Add (name, eventCallback);
	}

	public virtual void CallbackEvent(string name) {
		if (this.m_SimpleEvent.ContainsKey(name) == false)
			return;
		this.m_SimpleEvent[name].Invoke();
	}  

	public virtual void RemoveListener(string name, Action eventCallback) {
		if (this.m_SimpleEvent.ContainsKey(name) == false)
			return;
		this.m_SimpleEvent.Remove (name);
	}
	
	public virtual void RemoveAllListener(string name, Action eventCallback) {
		this.m_SimpleEvent.Clear ();
	}

	public virtual void CancelUI() {
		if (this.m_LoadingPanel != null) {
			this.m_LoadingPanel.SetActive (false);
		}
		if (this.m_MessagePanel != null) {
			this.m_MessagePanel.SetActive (false);
		}
	}

	public virtual void DisplayLoading(bool value) {
		if (this.m_LoadingPanel != null) {
			this.m_LoadingPanel.SetActive (value);
		}
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

	public virtual void SwithSceneTo(string name, float after = -1f) {
		if (after <= 0) {
			this.m_SwitchScene.LoadScene (name);
		} else {
			this.m_SwitchScene.LoadSceneAfterSeconds (name, after);
		}
	}

	#endregion

	#region Send

	/// <summary> 
	/// Emit message.
	/// </summary>
	public virtual void Emit(string ev) {
		if (this.m_Socket != null) {
			this.m_Socket.Emit(ev);
		}
	}

	/// <summary> 
	/// Emit message.
	/// </summary>
	public virtual void Emit(string ev, JSONObject data) {
		if (this.m_Socket != null) {
			this.m_Socket.Emit(ev, data);
		}
	}

	/// <summary>
	/// Emit Set player name.
	/// Necessary emit first.
	/// </summary>
	public void SetPlayername(string value = "Norman") {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		var roomData = new JSONObject();
		roomData.AddField("playerName", value);
		this.Emit("setPlayername", roomData);
		this.DisplayLoading (true);
		#if UNITY_DEBUG
		Debug.Log ("SetPlayername");
		#endif
	}

	/// <summary>
	/// Emit Join random room.
	/// </summary>
	public void JoinOrCreateRoom() {
		var random = UnityEngine.Random.Range (1, this.m_Rooms.Length);
		this.JoinRoom ("room-" + random);
	}

	/// <summary>
	/// Emit join a room by name.
	/// </summary>
	public void JoinRoom(string roomName) {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		var roomData = new JSONObject();
		roomData.AddField("roomName", roomName);
		this.Emit("joinOrCreateRoom", roomData);
		this.DisplayLoading (true);
		#if UNITY_DEBUG
		Debug.Log ("JoinOrCreateRoom");
		#endif
	}

	/// <summary>
	/// Emit to get room info.
	/// </summary>
	public void GetRoomsStatus(Action callback = null) {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		this.Emit("getRoomsStatus");
		this.DisplayLoading (true);
		this.m_Rooms = new CRoom[0];
		this.RemoveListener("updateRoomsComplete", callback);
		this.AddListener("updateRoomsComplete", callback);
		#if UNITY_DEBUG
		Debug.Log ("GetRoomsStatus");
		#endif
	}

	/// <summary>
	/// Send room message chat.
	/// </summary>
	public void SendMessageRoomChat(string msg = "Hey, i'm Norman.") {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		var roomData = new JSONObject();
		roomData.AddField("message", msg);
		this.Emit("sendRoomChat", roomData);
		#if UNITY_DEBUG
		Debug.Log ("SendMessageRoomChat");
		#endif
	}

	/// <summary>
	/// Send World message chat.
	/// </summary>
	public void SendMessageWorldChat(string msg = "Hey, i'm Norman.") {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		var roomData = new JSONObject();
		roomData.AddField("message", msg);
		this.Emit("sendWorldChat", roomData);
		#if UNITY_DEBUG
		Debug.Log ("SendMessageWorldChat");
		#endif
	}

	/// <summary>
	/// Emit leave room.
	/// </summary>
	public void LeaveRoom() {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		this.Emit("leaveRoom");
		#if UNITY_DEBUG
		Debug.Log ("leaveRoom");
		#endif
	}

	/// <summary>
	/// Send chess position
	/// </summary>
	public void SendChessPosition(int x, int y) {
		if (this.m_Socket.IsConnected == false) {
			this.m_Socket.Connect();
		}
		var roomData = new JSONObject();
		roomData.AddField("posX", x);
		roomData.AddField("posY", y);
		roomData.AddField("turnIndex", this.m_Data.turnIndex);
		this.Emit("sendChessPosition", roomData);
		#if UNITY_DEBUG
		Debug.Log ("sendChessPosition");
		#endif
	}

	#endregion

	#region Receive

	/// <summary>
	/// Receive open connect message.
	/// </summary>
	public void ReceiveOpenMsg(SocketIOEvent e)
	{
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
		#endif
	}

	/// <summary>
	/// Receive beep and boop message.
	/// Keep connect between client and server.
	/// </summary>
	public void ReceiveBoop(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Boop received: " + e.name + " " + e.data);
		#endif
	}
	
	/// <summary>
	/// Receive error.
	/// </summary>
	public void ReceiveErrorMsg(SocketIOEvent e)
	{
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
		#endif
		this.ShowMessage (e.data.GetField("msg").ToString());
		this.DisplayLoading (false);
	}
	
	/// <summary>
	/// Receive close connect message.
	/// </summary>
	public void ReceiveCloseMsg(SocketIOEvent e)
	{	
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
		#endif
	}

	/// <summary>
	/// Receive player name message.
	/// Emit from SetPlayerName.
	/// </summary>
	public void ReceivePlayerName (SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log ("[SOCKET IO] Player name receive " + e.name + e.data);
		#endif
		this.m_Data.id = e.data.GetField("id").ToString().Replace ("\"","");
		this.m_Data.name = e.data.GetField("name").ToString().Replace("\"", "");
		this.DisplayLoading (false);
		this.m_SwitchScene.LoadScene ("DisplayRoomsScene");
		CSetupGameScene.ALREADY_SETUP = true;
	}

	/// <summary>
	/// Receive Join a room complete message.
	/// Change PlayCaro7x7Scene
	/// Emit from JoinOrCreateRoom or JoinRoom.
	/// </summary>
	public void JoinRoomCompleted(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Join room received: " + e.name + " " + e.data);
		#endif
		var room = e.data.GetField("roomInfo");
		this.m_Room = new CRoom();
		this.m_Room.roomName = room.GetField("roomName").ToString().Replace ("\"","");
		var players = room.GetField("players").list;
		this.m_Room.roomPlayes = new CPlayerData[players.Count];
		for (int i = 0; i < players.Count; i++)
		{
			var tmpPlayer = players[i];
			this.m_Room.roomPlayes[i] = new CPlayerData();
			this.m_Room.roomPlayes[i].name = tmpPlayer.GetField("playerName").ToString().Replace ("\"","");
		}
		this.DisplayLoading (players.Count < 2);
		this.m_SwitchScene.LoadScene ("PlayCaro7x7Scene");
	}

	/// <summary>
	/// Receive Join a room fail message.
	/// Emit from JoinOrCreateRoom or JoinRoom.
	/// </summary>
	public void JoinRoomFailed(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Join room failed received: " + e.name + " " + e.data);
		#endif
		this.DisplayLoading (false);
		this.ShowMessage (e.data.GetField("msg").ToString());
	}

	/// <summary>
	/// Receive turn index for game.
	/// After enough player in room.
	/// </summary>
	public void ReceiveTurnIndex(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Join room failed received: " + e.name + " " + e.data);
		#endif
		this.m_Data.turnIndex = int.Parse (e.data.GetField("turnIndex").ToString());
		this.DisplayLoading (false);
	}

	/// <summary>
	/// Receive chess position complete.
	/// And change turn play.
	/// </summary>
	public void ReceiveChessPosition(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Received chess position: " + e.name + " " + e.data);
		#endif
		this.DisplayLoading (false);
	}

	/// <summary>
	/// Receive chess position fail.
	/// </summary>
	public void ReceiveChessFail(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Received chess fail: " + e.name + " " + e.data);
		#endif
		this.DisplayLoading (false);
		this.ShowMessage (e.data.GetField("msg").ToString());
	}

	/// <summary>
	/// Receive leave message.
	/// Emit from LeaveRoom.
	/// </summary>
	public void ReceiveClearRoom(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Received clear room: " + e.name + " " + e.data);
		#endif
		this.DisplayLoading (false);
		this.m_SwitchScene.LoadScene ("DisplayRoomsScene");
		this.ShowMessage (e.data.GetField("msg").ToString());
	}

	/// <summary>
	/// Receive leave message.
	/// Emit from LeaveRoom.
	/// </summary>
	public void LeaveRoomCompleted(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Leave received: " + e.name + " " + e.data);
		#endif
		this.DisplayLoading (false);
		this.m_SwitchScene.LoadScene ("DisplayRoomsScene");
	}

	/// <summary>
	/// Receive Update Room Status message.
	/// Emit from GetRoomStatus.
	/// </summary>
	public void UpdateRoomStatus(SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log("[SocketIO] Received update room status: " + e.name + " " + e.data);
		#endif
		this.DisplayLoading (false);
		var receiveRooms = e.data.GetField("rooms").list;
		this.m_Rooms = new CRoom[receiveRooms.Count];
		for (int i = 0; i < receiveRooms.Count; i++)
		{
			var tmpRoom = receiveRooms[i];
			var tmpRoomName = tmpRoom.GetField("roomName").ToString().Replace("\"", "");
			var tmpRoomDisplay = tmpRoom.GetField("roomDisplay").ToString().Replace("\"", "");
			// var tmpRoomPlayers = tmpRoom.GetField("players").ToString().Replace("\"", "");
			this.m_Rooms[i] = new CRoom();
			this.m_Rooms[i].roomName = tmpRoomName;
			this.m_Rooms[i].roomDisplay = tmpRoomDisplay;
		}
		this.CallbackEvent("updateRoomsComplete");
	}

	/// <summary>
	/// Receive Chat message.
	/// Emit from SendMessageRoomChat.
	/// </summary>
	public void ReceiveRoomChat (SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log ("[SOCKET IO] Room chat receive " + e.name + e.data);
		#endif
	}

	/// <summary>
	/// Receive World Chat message.
	/// Emit from SendMessageWorldChat.
	/// </summary>
	public void ReceiveWorldChat (SocketIOEvent e) {
		#if UNITY_DEBUG
		Debug.Log ("[SOCKET IO] World chat receive " + e.name + e.data);
		#endif
	}

	#endregion

}
