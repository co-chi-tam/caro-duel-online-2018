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

	[Serializable]
	public class CData {
		public string id;
		public string name;
		public int turnIndex;

		public CData()
		{
			this.id = string.Empty;
			this.name = string.Empty;
			this.turnIndex = -1;
		}

		public CData(CData value)
		{
			this.id = value.id;
			this.name = value.name;
			this.turnIndex = value.turnIndex;
		}
	}

	[SerializeField]	protected SocketIOComponent m_Socket;
	public SocketIOComponent socket { 
		get { return this.m_Socket; } 
		set { this.m_Socket = value; }
	}

	[SerializeField]	protected CData m_Data;
	public CData playerData { 
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
		socket.On("playerNameSet", this.ReceivePlayerName);
		socket.On("turnIndexSet", this.ReceiveTurnIndex);
		socket.On("receiveChessPosition", this.ReceiveChessPosition);
		socket.On("receiveChessFail", this.ReceiveChessFail);
		// UI
		this.CancelUI ();
	}

	protected virtual void Update() {
		// if (Input.GetKeyDown(KeyCode.A)) {
		// 	this.SetPlayername("Norman");
		// }
		// if (Input.GetKeyDown(KeyCode.B)) {
		// 	this.JoinOrCreateRoom();
		// }
		// if (Input.GetKeyDown(KeyCode.C)) {
		// 	this.SendMessageRoomChat("HAHAHA");
		// }
		// if (Input.GetKeyDown(KeyCode.D)) {
		// 	this.SendChessPosition(0, 1);
		// }
		if (Input.GetKeyDown(KeyCode.Escape)) {
			this.DisplayLoading (false);
		}
	}

	protected virtual void OnApplicationQuit()
	{
		this.LeaveRoom();
	}

	protected virtual void OnApplicationFocus(bool focusStatus)
	{
#if UNITY_ANDROID || UNITY_IOS 
		if (focusStatus == false) {
			this.LeaveRoom();
		} else {
		    this.m_Socket.Connect();
		}
#endif
	}

	protected virtual void OnApplicationPause(bool pauseStatus)
	{
#if UNITY_ANDROID || UNITY_IOS 
		if (pauseStatus == true) {
			this.LeaveRoom();
		} else {
		    this.m_Socket.Connect();
		}
#endif
	}

	#endregion

	#region Main methods

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

	#region Receive

	public void ReceiveOpenMsg(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
	}
	
	public void ReceiveErrorMsg(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Error received: " + e.name + " " + e.data);
		this.ShowMessage (e.data.GetField("msg").ToString());
		this.DisplayLoading (false);
	}
	
	public void ReceiveCloseMsg(SocketIOEvent e)
	{	
		Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
	}

	public void ReceivePlayerName (SocketIOEvent e) {
		Debug.Log ("[SOCKET IO] Player name receive " + e.name + e.data);
		this.m_Data.id = e.data.GetField("id").ToString().Replace ("\"","");
		this.m_Data.name = e.data.GetField("name").ToString().Replace("\"", "");
		this.DisplayLoading (false);
		this.m_SwitchScene.LoadScene ("DisplayRoomsScene");
	}

	public void JoinRoomCompleted(SocketIOEvent e) {
		Debug.Log("[SocketIO] Join room received: " + e.name + " " + e.data);
		var room = e.data.GetField("roomInfo");
		this.m_Room = new CRoom();
		this.m_Room.roomName = room.GetField("roomName").ToString().Replace ("\"","");
		var players = room.GetField("players").list;
		this.m_Room.roomPlayes = new CData[players.Count];
		for (int i = 0; i < players.Count; i++)
		{
			var tmpPlayer = players[i];
			this.m_Room.roomPlayes[i] = new CData();
			this.m_Room.roomPlayes[i].name = tmpPlayer.GetField("playerName").ToString().Replace ("\"","");
		}
		this.DisplayLoading (players.Count < 2);
		this.m_SwitchScene.LoadScene ("PlayCaro7x7Scene");
	}

	public void JoinRoomFailed(SocketIOEvent e) {
		Debug.Log("[SocketIO] Join room failed received: " + e.name + " " + e.data);
		this.m_Room = new CRoom();
		this.DisplayLoading (false);
		this.ShowMessage (e.data.GetField("msg").ToString());
	}

	public void ReceiveTurnIndex(SocketIOEvent e) {
		Debug.Log("[SocketIO] Join room failed received: " + e.name + " " + e.data);
		this.m_Data.turnIndex = int.Parse (e.data.GetField("turnIndex").ToString());
		this.DisplayLoading (false);
	}

	public void ReceiveChessPosition(SocketIOEvent e) {
		Debug.Log("[SocketIO] Received chess position: " + e.name + " " + e.data);
		this.DisplayLoading (false);
	}

	public void ReceiveChessFail(SocketIOEvent e) {
		Debug.Log("[SocketIO] Received chess fail: " + e.name + " " + e.data);
		this.DisplayLoading (false);
		this.ShowMessage (e.data.GetField("msg").ToString());
	}

	public void ReceiveClearRoom(SocketIOEvent e) {
		Debug.Log("[SocketIO] Received clear room: " + e.name + " " + e.data);
		this.DisplayLoading (false);
		this.m_SwitchScene.LoadScene ("DisplayRoomsScene");
		this.ShowMessage (e.data.GetField("msg").ToString());
	}

	public void UpdateRoomStatus(SocketIOEvent e) {
		Debug.Log("[SocketIO] Received update room status: " + e.name + " " + e.data);
		this.DisplayLoading (false);
		var receiveRooms = e.data.GetField("rooms").list;
		this.m_Rooms = new CRoom[receiveRooms.Count];
		for (int i = 0; i < receiveRooms.Count; i++)
		{
			var tmpRoom = receiveRooms[i];
			var tmpRoomName = tmpRoom.GetField("roomName").ToString().Replace("\"", "");
			var tmpRoomPlayers = tmpRoom.GetField("players").ToString().Replace("\"", "");
			this.m_Rooms[i] = new CRoom();
			this.m_Rooms[i].roomDisplay = string.Format ("[{0}]: {1}/2", tmpRoomName, tmpRoomPlayers);
		}
		this.CallbackEvent("updateRoomsComplete");
	}

	public void LeaveRoomCompleted(SocketIOEvent e) {
		Debug.Log("[SocketIO] Leave received: " + e.name + " " + e.data);
		this.DisplayLoading (false);
		this.m_SwitchScene.LoadScene ("DisplayRoomsScene");
	}

	public void ReceiveRoomChat (SocketIOEvent e) {
		Debug.Log ("[SOCKET IO] Room chat receive " + e.name + e.data);
	}

	#endregion

	#region Send

	public void SetPlayername(string value = "Norman") {
		var roomData = new JSONObject();
		roomData.AddField("playerName", value);
		socket.Emit("setPlayername", roomData);
		this.DisplayLoading (true);
	}

	public void JoinOrCreateRoom() {
		var random = UnityEngine.Random.Range (1, 10);
		this.JoinRoom ("room-" + random);
	}

	public void JoinRoom(string roomName) {
		var roomData = new JSONObject();
		roomData.AddField("roomName", roomName);
		socket.Emit("joinOrCreateRoom", roomData);
		Debug.Log ("JoinOrCreateRoom");
		this.DisplayLoading (true);
	}

	public void GetRoomsStatus(Action callback = null) {
		socket.Emit("getRoomsStatus");
		Debug.Log ("GetRoomsStatus");
		this.DisplayLoading (true);
		this.m_Rooms = new CRoom[0];
		this.RemoveListener("updateRoomsComplete", callback);
		this.AddListener("updateRoomsComplete", callback);
	}

	public void SendMessageRoomChat(string msg = "Hey, i'm Norman.") {
		var roomData = new JSONObject();
		roomData.AddField("message", msg);
		socket.Emit("sendRoomChat", roomData);
		Debug.Log ("SendMessageChat");
	}

	public void LeaveRoom() {
		socket.Emit("leaveRoom");
		Debug.Log ("leaveRoom");
	}

	public void SendChessPosition(int x, int y) {
		var roomData = new JSONObject();
		roomData.AddField("posX", x);
		roomData.AddField("posY", y);
		roomData.AddField("turnIndex", this.m_Data.turnIndex);
		socket.Emit("sendChessPosition", roomData);
		Debug.Log ("sendChessPosition");
	}

	#endregion

}
